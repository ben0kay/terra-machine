// Inspector-tunable density-gradient forest benchmark; no general biome or world streamer.
using Godot;

[GlobalClass]
public partial class ForestPreviewSettings : Resource
{
    [ExportGroup("Tree asset")]
    [Export] public PackedScene? PineTree { get; set; }
    [Export] public PackedScene? TallPineTree { get; set; }
    [Export] public int TallTreeCount { get; set; } = 8;
    [ExportGroup("Footprint")]
    [Export] public int Seed { get; set; } = 20261029;
    [Export] public Vector2 CenterOffset { get; set; } = new(18f, 118f);
    [Export] public float RadiusX { get; set; } = 82f;
    [Export] public float RadiusZ { get; set; } = 86f;
    [Export] public int TreeCount { get; set; } = 115;
    [ExportGroup("Forest composition")]
    [Export] public float CenterSpacing { get; set; } = 7.5f;
    [Export] public float EdgeSpacing { get; set; } = 14f;
    [Export] public float OriginalTreeClearance { get; set; } = 8f;
    [Export] public float TrailHalfWidth { get; set; } = 3f;
    [Export] public float MinimumScale { get; set; } = 0.82f;
    [Export] public float MaximumScale { get; set; } = 1.33f;
    [Export] public float GroundSink { get; set; } = 0.08f;
    [Export] public float MaximumSlopeDegrees { get; set; } = 30f;
    [Export] public bool NearShadows { get; set; } = true;
    [ExportGroup("Loading budget")]
    [Export] public float ProbeTop { get; set; } = 192f;
    [Export] public float PlacementRange { get; set; } = 140f;
    [Export] public int QueriesPerPhysicsTick { get; set; } = 4;
    [Export] public float RetrySeconds { get; set; } = 2f;
    [Export] public int MaxGroundRetries { get; set; } = 18;
}
