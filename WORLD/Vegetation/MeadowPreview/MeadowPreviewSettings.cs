// Small authored tree-adjacent meadow preview settings; not a world vegetation streamer.
using Godot;

[GlobalClass]
public partial class MeadowPreviewSettings : Resource
{
    [ExportGroup("Assets")]
    [Export] public Godot.Collections.Array<PackedScene> Variants { get; set; } = new();
    [Export] public ShaderMaterial? Material { get; set; }
    [Export] public StaticCollisionProfile? Collision { get; set; }
    [Export] public bool CollectAllMeshes { get; set; }
    [Export] public bool GroundAtMeshBottom { get; set; }
    [Export] public int TreeSiteLimit { get; set; } = 0;
    [ExportGroup("Area sites")]
    [Export] public Godot.Collections.Array<Vector2> PreviewOffsets { get; set; } = new();
    [Export] public int SiteCount { get; set; } = 10;
    [Export] public float SiteSpacing { get; set; } = 8f;
    [ExportGroup("Composition")]
    [Export] public int Seed { get; set; } = 906;
    [Export] public int SamplesPerTree { get; set; } = 72;
    [Export] public float PatchRadius { get; set; } = 5.5f;
    [Export] public int ClumpsPerSite { get; set; } = 3;
    [Export] public float ClumpRadius { get; set; } = 0.9f;
    [Export] public float TrunkClearance { get; set; } = 1.4f;
    [Export] public float MinimumScale { get; set; } = 0.8f;
    [Export] public float MaximumScale { get; set; } = 1.25f;
    [Export] public float MaximumSlopeDegrees { get; set; } = 32f;
    [Export] public float GroundSink { get; set; } = 0.02f;
    [Export] public float DrawDistance { get; set; } = 140f;
    [ExportGroup("Loading")]
    [Export] public int QueriesPerPhysicsTick { get; set; } = 40;
    [Export] public float RetrySeconds { get; set; } = 0.6f;
    [Export] public float LoadingTimeoutSeconds { get; set; } = 12f;
}
