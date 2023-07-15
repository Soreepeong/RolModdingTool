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
    public readonly uint Id;
    public readonly string Name;
    public Matrix4x4 BindPoseMatrix;
    public Controller? Parent;
    public readonly List<Controller> Children = new();

    public Controller(uint id, string name, Matrix4x4 bindPoseMatrix, Controller? parent = null) {
        Id = id;
        // Id = Crc32.CryE.Get(name);
        // if (Id != id)
        //     Debugger.Break();
        Name = name;
        BindPoseMatrix = bindPoseMatrix;
        Parent = parent;
        Parent?.Children.Add(this);
    }

    public Matrix4x4 InverseBindPoseMatrix =>
        Matrix4x4.Invert(BindPoseMatrix, out var res) ? res : throw new InvalidOperationException();

    public Controller CloneInto(Controller parent, IList<Controller> controllerList) {
        var res = new Controller(Id, Name, BindPoseMatrix, parent);
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
                    compiledBone.LocalTransformMatrix.Transformation,
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
                    LocalTransformMatrix = Matrix3x4.CreateFromMatrix4x4(controller.BindPoseMatrix),
                    Mass = 0,
                    Name = controller.Name,
                    ParentOffset = controller.Parent is null ? bones.Count : parentIndex - bones.Count,
                    PhysicsLive = new(),
                    PhysicsDead = default,
                    WorldTransformMatrix = Matrix3x4.CreateFromMatrix4x4(controller.InverseBindPoseMatrix),
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
