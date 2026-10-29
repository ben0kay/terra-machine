// Budgets and visual ranges for the player-centred grass trial.
using Godot;
[GlobalClass]
public partial class GrassScatterSettings : Resource
{
    [ExportGroup("Assets")]
    [Export] public PackedScene? Variants { get; set; }
    [Export] public ShaderMaterial? Material { get; set; }
    [Export] public string[] MeshNames { get; set; } = { "grass_bermuda_01_medium_a", "grass_bermuda_01_medium_c", "grass_bermuda_01_small_c" };
    [ExportGroup("Coverage")]
    [Export] public float TileSize { get; set; } = 16f;
    [Export] public float TuftsPerSquareMeter { get; set; } = 3f;
    [Export] public float DenseDistance { get; set; } = 20f;
    [Export] public float SparseDistance { get; set; } = 45f;
    [Export] public float EndDistance { get; set; } = 60f;
    [Export] public float MaximumSlopeDegrees { get; set; } = 35f;
    [Export] public float MinimumScale { get; set; } = 2.2f;
    [Export] public float MaximumScale { get; set; } = 3.5f;
    [ExportGroup("Work budget")]
    [Export] public int Seed { get; set; } = 727;
    [Export] public int QueriesPerPhysicsTick { get; set; } = 64;
    [Export] public float TileCheckSeconds { get; set; } = 0.25f;
}
