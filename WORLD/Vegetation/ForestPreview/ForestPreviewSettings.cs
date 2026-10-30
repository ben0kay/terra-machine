// Inspector-tunable density-gradient forest benchmark; no general biome or world streamer.
using Godot;

[GlobalClass]
public partial class ForestPreviewSettings : Resource
{
    [ExportGroup("Tree asset")]
    [Export] public PackedScene? PineTree { get; set; }
    [Export] public PackedScene? TallPineTree { get; set; }
    [Export] public int TallTreeCount { get; set; } = 10;
    [ExportGroup("Fir sapling understory")]
    [Export] public Godot.Collections.Array<PackedScene> SaplingVariants { get; set; } = new();
    [Export] public int SaplingCount { get; set; } = 41;
    [Export] public float SaplingCoreRadiusFraction { get; set; } = 0.76f;
    [Export] public float SaplingToPineClearance { get; set; } = 2.9f;
    [Export] public float SaplingSpacing { get; set; } = 5.5f;
    [Export] public float SaplingNearPineRange { get; set; } = 12f;
    [Export] public float SaplingMinimumScale { get; set; } = 0.85f;
    [Export] public float SaplingMaximumScale { get; set; } = 1.14f;
    [Export] public bool SaplingShadows { get; set; } = false;
    [Export] public float SaplingCullDistance { get; set; } = 125f;
    [ExportGroup("Footprint")]
    [Export] public int Seed { get; set; } = 20261029;
    [Export] public Vector2 CenterOffset { get; set; } = new(18f, 137.3f);
    [Export] public float RadiusX { get; set; } = 100.4f;
    [Export] public float RadiusZ { get; set; } = 105.3f;
    [Export] public int TreeCount { get; set; } = 156;
    [ExportGroup("Forest composition")]
    [Export] public float CenterSpacing { get; set; } = 6.4f;
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
