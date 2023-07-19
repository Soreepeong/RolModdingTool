using System;
using System.Collections.Generic;
using System.Linq;
using SynergyLib.FileFormat.CryEngine.CryDefinitions.Chunks;
using SynergyLib.FileFormat.CryEngine.CryDefinitions.Enums;

namespace SynergyLib.FileFormat.CryEngine.CryModelElements;

public class PseudoMaterial {
    public readonly string Name;
    public readonly List<PseudoMaterial> Children = new();

    public MtlNameFlags Flags;
    public MtlNamePhysicsType PhysicsType;
    public float ShOpacity;

    public PseudoMaterial(string name) {
        Name = name;
    }

    public PseudoMaterial(MtlNameChunk chunk) {
        Name = chunk.Name;
        Flags = chunk.Flags;
        PhysicsType = chunk.PhysicsType;
        ShOpacity = chunk.ShOpacity;
    }

    public IEnumerable<Tuple<PseudoMaterial, PseudoMaterial?>> EnumerateHierarchy() {
        yield return new(this, null);
        foreach (var e in Children.SelectMany(c => c.EnumerateHierarchy()))
            yield return new(e.Item1, this);
    }
}