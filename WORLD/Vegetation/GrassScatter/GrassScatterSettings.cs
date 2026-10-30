// Budgets and visual ranges for the player-centred grass trial.
using Godot;
[GlobalClass]
public partial class GrassScatterSettings : Resource
{
    [ExportGroup("Assets")]
    [Export] public PackedScene? MidVariants { get; set; }
    [Export] public PackedScene? FarVariants { get; set; }
    [Export] public float MidLodDistance { get; set; } = 32f;
    [Export] public PackedScene? Variants { get; set; }
    [Export] public ShaderMaterial? Material { get; set; }
    [Export] public string[] MeshNames { get; set; } = { "grass_bermuda_01_medium_a", "grass_bermuda_01_medium_c", "grass_bermuda_01_small_c" };
    [Export] public Godot.Collections.Array<PackedScene> NearScenes { get; set; } = new();
    [Export] public Godot.Collections.Array<PackedScene> FarScenes { get; set; } = new();
    [Export(PropertyHint.Range, "5,70,1")] public float LodSwitchDistance { get; set; } = 36f;
    [ExportGroup("Coverage")]
    [Export] public float TileSize { get; set; } = 16f;
    [Export] public float TuftsPerSquareMeter { get; set; } = 12f;
    [Export] public float DenseDistance { get; set; } = 20f;
    [Export] public float SparseDistance { get; set; } = 55f;
    [Export] public float EndDistance { get; set; } = 80f;
    [Export] public float MaximumSlopeDegrees { get; set; } = 35f;
    [Export] public float MinimumScale { get; set; } = 3.0f;
    [Export] public float MaximumScale { get; set; } = 4.5f;
    [Export(PropertyHint.Range, "0,1,0.01")] public float TallTuftChance { get; set; } = 0f;
    [Export] public float TallMinimumScale { get; set; } = 1.3f;
    [Export] public float TallMaximumScale { get; set; } = 1.65f;
    [Export] public float MinimumTuftSpacing { get; set; } = 0f;
    [Export] public float MinimumPatchCoverage { get; set; } = 0.75f;
    [Export] public float DistantDensityFraction { get; set; } = 0.15f;
    [ExportGroup("Clumped patches")]
    [Export] public float PatchSpacing { get; set; } = 0f;
    [Export] public float PatchRadius { get; set; } = 2.5f;
    [ExportGroup("Work budget")]
    [Export] public int Seed { get; set; } = 727;
    [Export] public int QueriesPerPhysicsTick { get; set; } = 64;
    [Export] public float TileCheckSeconds { get; set; } = 0.25f;
    [Export] public bool HideOutsideArea { get; set; } = false;
}
