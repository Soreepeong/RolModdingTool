using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using SynergyLib.FileFormat.CryEngine.CryDefinitions.Structs;
using SynergyLib.Util;
using SynergyLib.Util.MathExtras;

namespace SynergyLib.FileFormat.CryEngine.CryModelElements;

public class Controller {
    private Matrix4x4 _relativeBindPoseMatrix;
    private Matrix4x4 _inverseRelativeBindPoseMatrix;

    public readonly uint Id;
    public readonly string Name;
    private readonly ObservableCollection<Controller> _children;

    public Controller(uint id, string name) {
        Id = id;
        Name = name;
        _children = new();
        _children.CollectionChanged += ChildrenOnCollectionChanged;
    }

    public Controller(uint id, string name, Matrix4x4 relativeBindPoseMatrix) : this(id, name) {
        Id = id;
        Name = name;

        _relativeBindPoseMatrix = relativeBindPoseMatrix;
        _inverseRelativeBindPoseMatrix = Matrix4x4.Invert(relativeBindPoseMatrix, out var inverted)
            ? inverted
            : throw new InvalidDataException();
    }

    public Controller(IReadOnlyList<CompiledBone> compiledBones) : this(
        compiledBones,
        compiledBones.Select((x, i) => (x, i)).Single(x => x.x.ParentOffset == 0).i) { }

    private Controller(IReadOnlyList<CompiledBone> compiledBones, int index) : this(
        compiledBones[index].ControllerId,
        compiledBones[index].Name) {
        var bone = compiledBones[index];

        var hasParent = compiledBones[index].ParentOffset != 0;
        _relativeBindPoseMatrix = Matrix4x4.Transpose(bone.LocalTransformMatrix.Transformation);
        if (hasParent) {
            var parentBone = compiledBones[index + bone.ParentOffset];
            var parentInverseAbsoluteBindPose = Matrix4x4.Transpose(parentBone.WorldTransformMatrix.Transformation);
            _relativeBindPoseMatrix = parentInverseAbsoluteBindPose * _relativeBindPoseMatrix;
        }

        _inverseRelativeBindPoseMatrix = Matrix4x4.Invert(_relativeBindPoseMatrix, out var inverted)
            ? inverted
            : throw new InvalidDataException();

        for (var i = 0; i < bone.ChildCount; i++)
            _children.Add(new(compiledBones, index + bone.ChildOffset + i));

        if (!hasParent) {
            if (Count != compiledBones.Count)
                throw new InvalidDataException("Number of bones do not match after tree generation.");

            foreach (var (c1, c2) in GetEnumeratorBreadthFirst().Zip(compiledBones)) {
                if (c1.Id != c2.ControllerId)
                    throw new InvalidDataException("Order of controllers do not match tree generation.");
            }
        }
    }

    public Controller? Parent { get; private set; }

    public IList<Controller> Children => _children;

    public uint CalculatedId => Crc32.CryE.Get(Name);

    public int Count => 1 + _children.Sum(x => x.Count);

    public Matrix4x4 AbsoluteBindPoseMatrix {
        get => Parent is null
            ? _relativeBindPoseMatrix
            : Parent.AbsoluteBindPoseMatrix * _relativeBindPoseMatrix;
        set {
            if (!Matrix4x4.Invert(value, out var inverseAbsoluteBindPoseMatrix))
                throw new InvalidOperationException();

            _relativeBindPoseMatrix = Parent is null
                ? value
                : Parent.InverseAbsoluteBindPoseMatrix * value;
            _inverseRelativeBindPoseMatrix = Parent is null
                ? inverseAbsoluteBindPoseMatrix
                : inverseAbsoluteBindPoseMatrix * Parent.AbsoluteBindPoseMatrix;
        }
    }

    public Matrix4x4 InverseAbsoluteBindPoseMatrix {
        get => Parent is null
            ? _inverseRelativeBindPoseMatrix
            : _inverseRelativeBindPoseMatrix * Parent.InverseAbsoluteBindPoseMatrix;
        set {
            if (!Matrix4x4.Invert(value, out var absoluteBindPoseMatrix))
                throw new InvalidOperationException();

            _relativeBindPoseMatrix = Parent is null
                ? absoluteBindPoseMatrix
                : Parent.AbsoluteBindPoseMatrix * absoluteBindPoseMatrix;
            _inverseRelativeBindPoseMatrix = Parent is null
                ? value
                : value * Parent.InverseAbsoluteBindPoseMatrix;
        }
    }

    public Matrix4x4 RelativeBindPoseMatrix {
        get => _relativeBindPoseMatrix;
        set {
            if (!Matrix4x4.Invert(value, out _inverseRelativeBindPoseMatrix))
                throw new InvalidOperationException();
            _relativeBindPoseMatrix = value;
        }
    }

    public Matrix4x4 InverseRelativeBindPoseMatrix {
        get => _inverseRelativeBindPoseMatrix;
        set {
            if (!Matrix4x4.Invert(value, out _relativeBindPoseMatrix))
                throw new InvalidOperationException();
            _inverseRelativeBindPoseMatrix = value;
        }
    }

    public Tuple<Vector3, Quaternion> Decomposed {
        get {
            Matrix4x4.Decompose(
                _inverseRelativeBindPoseMatrix,
                out _,
                out var r,
                out var t);
            return Tuple.Create(t, r);
        }
        set => InverseRelativeBindPoseMatrix = Matrix4x4.Multiply(
            Matrix4x4.CreateFromQuaternion(value.Item2),
            Matrix4x4.CreateTranslation(value.Item1));
    }

    private void ChildrenOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e) {
        if (e.OldItems is not null)
            foreach (var c in e.OldItems.Cast<Controller>()) {
                Debug.Assert(c.Parent is not null);
                c.Parent = null;
            }

        if (e.NewItems is not null)
            foreach (var c in e.NewItems.Cast<Controller>()) {
                Debug.Assert(c.Parent is null);
                c.Parent = this;
            }
    }

    public IEnumerable<Controller> GetEnumeratorDepthFirst() {
        yield return this;
        foreach (var child in _children.SelectMany(x => x.GetEnumeratorDepthFirst()))
            yield return child;
    }

    public IEnumerable<Controller> GetEnumeratorBreadthFirst(bool yieldThis = true) {
        if (yieldThis)
            yield return this;
        foreach (var child in _children)
            yield return child;
        foreach (var child in _children.SelectMany(x => x.GetEnumeratorBreadthFirst(false)))
            yield return child;
    }

    public override string ToString() => _children.Any()
        ? $"{nameof(Controller)}: {Name} #{Id:X08}"
        : $"{nameof(Controller)}: {Name} #{Id:X08} (Leaf)";

    public IEnumerable<CompiledBone> ToCompiledBonesList() {
        var boneId = 0;
        var nextAvailableIndex = 1;
        var controllerToBoneId = new Dictionary<Controller, int>();
        foreach (var c in GetEnumeratorBreadthFirst()) {
            yield return new() {
                ChildCount = c.Children.Count,
                ChildOffset = nextAvailableIndex,
                ControllerId = c.Id,
                LimbId = uint.MaxValue,
                LocalTransformMatrix = Matrix3x4.CreateFromMatrix4x4(Matrix4x4.Transpose(c.AbsoluteBindPoseMatrix)),
                Mass = 0,
                Name = c.Name,
                ParentOffset = c.Parent is null ? 0 : controllerToBoneId[c.Parent] - boneId,
                PhysicsLive = new(),
                PhysicsDead = default,
                WorldTransformMatrix =
                    Matrix3x4.CreateFromMatrix4x4(Matrix4x4.Transpose(c.InverseAbsoluteBindPoseMatrix)),
            };
            controllerToBoneId[c] = boneId;
            nextAvailableIndex += c.Children.Count;
            boneId++;
        }
    }

    public IEnumerable<BoneEntity> ToBoneEntityList() {
        var boneId = 0;
        var controllerToBoneId = new Dictionary<Controller, int>();
        foreach (var c in GetEnumeratorDepthFirst()) {
            yield return new() {
                BoneId = boneId,
                ChildCount = c._children.Count,
                ControllerId = c.Id,
                ParentId = c.Parent is null ? -1 : controllerToBoneId[c.Parent],
                Physics = new(),
                Properties = string.Empty,
            };

            controllerToBoneId[c] = boneId;
            boneId++;
        }
    }
}
