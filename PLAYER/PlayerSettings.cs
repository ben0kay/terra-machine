// Shared player tuning. No terrain or third-party addon dependencies.
using Godot;

[GlobalClass]
public partial class PlayerSettings : Resource
{
    [ExportGroup("Movement")]
    [Export] public float WalkSpeed { get; set; } = 5f;
    [Export] public float SprintMultiplier { get; set; } = 1.6f;
    [Export] public float Acceleration { get; set; } = 24f;
    [Export] public float Gravity { get; set; } = 20f;
    [Export] public float JumpVelocity { get; set; } = 6f;
    [ExportGroup("Camera")]
    [Export] public float MouseSensitivity { get; set; } = 0.002f;
    [Export] public bool InvertMouseY { get; set; } = false;
    [Export] public float FieldOfView { get; set; } = 75f;
}
