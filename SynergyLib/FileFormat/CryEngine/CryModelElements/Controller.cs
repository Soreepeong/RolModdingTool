using System;
using System.Collections.Generic;
using System.Diagnostics;
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

    public Controller? Parent;
    public readonly List<Controller> Children = new();

    public Controller(uint id, string name, in Matrix4x4 absoluteBindPoseMatrix, Controller? parent = null) {
        Id = id;
        Name = name;
        Parent = parent;
        Parent?.Children.Add(this);

        if (!Matrix4x4.Invert(absoluteBindPoseMatrix, out var inverseAbsoluteBindPoseMatrix))
            throw new InvalidOperationException();

        _relativeBindPoseMatrix = Parent is null
            ? absoluteBindPoseMatrix
            : Parent.InverseAbsoluteBindPoseMatrix * absoluteBindPoseMatrix;
        _inverseRelativeBindPoseMatrix = Parent is null
            ? inverseAbsoluteBindPoseMatrix
            : inverseAbsoluteBindPoseMatrix * Parent.AbsoluteBindPoseMatrix;
    }

    public Controller(string name, in Matrix4x4 bindPoseMatrix, Controller? parent = null)
        : this(Crc32.CryE.Get(name), name, bindPoseMatrix, parent) { }

    public uint CalculatedId => Crc32.CryE.Get(Name);

    public int Depth => 1 + (Parent?.Depth ?? 0);

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
        get =>
            Matrix4x4.Decompose(
                _inverseRelativeBindPoseMatrix,
                out _,
                out var r,
                out var t)
                ? Tuple.Create(t, r)
                : throw new InvalidOperationException();
        set => InverseRelativeBindPoseMatrix = Matrix4x4.Multiply(
            Matrix4x4.CreateFromQuaternion(value.Item2),
            Matrix4x4.CreateTranslation(value.Item1));
    }

    public Controller CloneInto(Controller parent, IList<Controller> controllerList) {
        var res = new Controller(Id, Name, AbsoluteBindPoseMatrix, parent);
        controllerList.Add(res);
        foreach (var c in Children)
            c.CloneInto(res, controllerList);
        return res;
    }

    public static List<Controller> ListFromCompiledBones(IReadOnlyList<CompiledBone> bones) {
        var controllers = new List<Controller>(bones.Count);
        foreach (var compiledBone in bones)
            controllers.Add(
                new(
                    compiledBone.ControllerId,
                    compiledBone.Name,
                    Matrix4x4.Transpose(compiledBone.LocalTransformMatrix.Transformation),
                    compiledBone.ParentOffset == 0
                        ? null
                        : controllers[controllers.Count + compiledBone.ParentOffset]));
        return controllers;
    }

    public static List<CompiledBone> ToCompiledBonesList(IReadOnlyList<Controller> controllers) {
        var bones = new List<CompiledBone>(controllers.Count);
        var pendingControllers = controllers
            .Where(x => x.Parent is null)
            .Select(x => (controller: x, parentIndex: -1))
            .ToList();
        while (pendingControllers.Any()) {
            var (controller, parentIndex) = pendingControllers.First();
            pendingControllers.RemoveAt(0);
            if (controller.Parent is not null) {
                if (bones[parentIndex].ChildCount++ == 0)
                    bones[parentIndex].ChildOffset = bones.Count - parentIndex;
            }

            bones.Add(
                new() {
                    ChildCount = 0,
                    ChildOffset = 0,
                    ControllerId = controller.Id,
                    LimbId = uint.MaxValue,
                    LocalTransformMatrix =
                        Matrix3x4.CreateFromMatrix4x4(Matrix4x4.Transpose(controller.AbsoluteBindPoseMatrix)),
                    Mass = 0,
                    Name = controller.Name,
                    ParentOffset = controller.Parent is null ? bones.Count : parentIndex - bones.Count,
                    PhysicsLive = new(),
                    PhysicsDead = default,
                    WorldTransformMatrix =
                        Matrix3x4.CreateFromMatrix4x4(Matrix4x4.Transpose(controller.InverseAbsoluteBindPoseMatrix)),
                });
            pendingControllers.AddRange(controller.Children.Select(x => (controller: x, bones.Count - 1)));
        }

        Debug.Assert(bones.Count == controllers.Count);

        return bones;
    }

    public static List<BoneEntity> ToBoneEntityList(IReadOnlyList<Controller> controllers) {
        var result = new List<BoneEntity>(controllers.Count);

        void WriteController(Controller controller, int parentId) {
            var boneId = result.Count;
            result.Add(
                new() {
                    BoneId = boneId,
                    ChildCount = controller.Children.Count,
                    ControllerId = controller.Id,
                    ParentId = parentId,
                    Physics = new(),
                    Properties = string.Empty,
                });

            foreach (var c in controller.Children)
                WriteController(c, boneId);
        }

        foreach (var c in controllers)
            if (c.Parent is null)
                WriteController(c, -1);

        Debug.Assert(result.Count == controllers.Count);

        return result;
    }

    public override string ToString() => Children.Any()
        ? $"{nameof(CryModel)}: {Name} #{Id:X08}"
        : $"{nameof(CryModel)}: {Name} #{Id:X08} (Leaf)";
}
