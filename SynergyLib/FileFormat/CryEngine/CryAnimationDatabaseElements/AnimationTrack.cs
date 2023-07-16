using SynergyLib.FileFormat.CryEngine.CryDefinitions.Structs;

namespace SynergyLib.FileFormat.CryEngine.CryAnimationDatabaseElements;

public class AnimationTrack {
    public ControllerKeyPosition? Position;
    public ControllerKeyRotation? Rotation;
    public ControllerKeyTime? PositionTime;
    public ControllerKeyTime? RotationTime;
}