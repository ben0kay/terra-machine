// Test-world composition tuning; terrain generation is owned by TerrainMaster.
using Godot;

[GlobalClass]
public partial class WorldSettings : Resource
{
    [ExportGroup("Spawn")]
    [Export] public Vector2 SpawnXZ { get; set; } = Vector2.Zero;
    [Export] public float SpawnProbeTop { get; set; } = 90f;
    [Export] public float SpawnProbeBottom { get; set; } = -100f;
    [Export] public float LoadingTimeoutSeconds { get; set; } = 60f;
    [ExportGroup("TestSafety")]
    [Export] public float PlayableHalfExtent { get; set; } = 440f;
    [Export] public float KillHeight { get; set; } = -100f;
}
