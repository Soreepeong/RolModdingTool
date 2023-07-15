using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using SynergyLib.FileFormat.CryEngine.CryDefinitions.Structs;
using SynergyLib.Util;

namespace SynergyLib.FileFormat.CryEngine.CryModelElements;

public class Controller {
    public readonly uint Id;
    public readonly string Name;
    public Matrix4x4 BindPoseMatrix;
    public Controller? Parent;
    public readonly List<Controller> Children = new();

    public Controller(string name, Matrix4x4 bindPoseMatrix, Controller? parent = null) {
        Id = Crc32.CryE.Get(name);
        Name = name;
        BindPoseMatrix = bindPoseMatrix;
        Parent = parent;
        Parent?.Children.Add(this);
    }

    public static List<Controller> ListFromCompiledBones(IReadOnlyList<CompiledBone> bones) {
        var controllers = new List<Controller>(bones.Count);
        foreach (var compiledBone in bones)
            controllers.Add(
                new(
                    compiledBone.Name,
                    compiledBone.LocalTransformMatrix.Transformation,
                    compiledBone.ParentOffset == 0
                        ? null
                        : controllers[controllers.Count + compiledBone.ParentOffset]));
        return controllers;
    }

    public override string ToString() => Children.Any()
        ? $"{nameof(CryModel)}: {Name} #{Id:X08}"
        : $"{nameof(CryModel)}: {Name} #{Id:X08} (Leaf)";
}
