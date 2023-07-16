using System.Collections.Generic;
using SynergyLib.FileFormat.CryEngine.CryDefinitions.Structs;

namespace SynergyLib.FileFormat.CryEngine.CryAnimationDatabaseElements;

public class Animation {
    public ControllerMotionParams MotionParams;
    public Dictionary<uint, AnimationTrack> Tracks = new();
}