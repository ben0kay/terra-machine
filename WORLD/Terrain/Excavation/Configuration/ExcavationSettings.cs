// Hard work limits for the bounded DG1 proof, independent of individual tool settings.
using Godot;

[GlobalClass]
public partial class ExcavationSettings : Resource
{
    [Export] public float MaximumReach { get; set; } = 4f;
    [Export] public float MaximumRadius { get; set; } = 1.5f;
}
