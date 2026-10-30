// Small rock scatter settings for the flat-side forest visual benchmark.
using Godot;

[GlobalClass]
public partial class BoulderPreviewSettings : Resource
{
    [ExportGroup("Asset")]
    [Export] public PackedScene? Model { get; set; }
    [ExportGroup("Scatter")]
    [Export] public int Seed { get; set; } = 414;
    [Export] public int RockCount { get; set; } = 14;
    [Export] public float MinimumSpacing { get; set; } = 20f;
    [Export] public float TreeClearance { get; set; } = 5.5f;
    [Export] public float MinimumScale { get; set; } = 0.55f;
    [Export] public float MaximumScale { get; set; } = 1.0f;
    [Export] public float GroundSink { get; set; } = 0.27f;
    [Export] public float MaximumSlopeDegrees { get; set; } = 22f;
    [ExportGroup("Rendering and collision")]
    [Export] public float DrawDistance { get; set; } = 190f;
    [Export] public bool CastShadows { get; set; } = true;
    [Export] public bool EnableCollision { get; set; } = true;
    [ExportGroup("Loading")]
    [Export] public float ProbeTop { get; set; } = 192f;
    [Export] public float PlacementRange { get; set; } = 140f;
    [Export] public int QueriesPerPhysicsTick { get; set; } = 3;
    [Export] public float RetrySeconds { get; set; } = 2f;
    [Export] public int MaxGroundRetries { get; set; } = 18;
}
