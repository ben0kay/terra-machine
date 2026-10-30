// Placeholder shovel's reach, bounded scoop size and discrete-click cooldown.
using Godot;

[GlobalClass]
public partial class ShovelSettings : Resource
{
    [Export] public float Reach { get; set; } = 3.5f;
    [Export] public float Radius { get; set; } = 1.0f;
    [Export] public float SurfaceInset { get; set; } = 0.15f;
    [Export] public float CooldownSeconds { get; set; } = 0.65f;
}
