// Settings for a small deliberately positioned visual benchmark, not a forest generator.
using Godot;

[GlobalClass]
public partial class TreePreviewSettings : Resource
{
    [ExportGroup("Assets")]
    [Export] public Godot.Collections.Array<PackedScene> Variants { get; set; } = new();
    [ExportGroup("Composition")]
    [Export] public Godot.Collections.Array<Vector2> Offsets { get; set; } = new();
    [Export] public float MinimumScale { get; set; } = 0.9f;
    [Export] public float MaximumScale { get; set; } = 1.12f;
    [Export] public float GroundSink { get; set; } = 0.08f;
    [ExportGroup("Foliage")]
    [Export(PropertyHint.Range, "0.05,0.95")] public float AlphaCutoff { get; set; } = 0.35f;
    [ExportGroup("Loading")]
    [Export] public float RetryInterval { get; set; } = 0.2f;
    [Export] public float LoadingTimeout { get; set; } = 40f;
    [Export] public int QueriesPerTick { get; set; } = 4;
}
