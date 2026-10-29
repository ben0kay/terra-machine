// Composition only: connects ordinary player transforms to the terrain contract and gates spawn.
using Godot;

public partial class TestWorld : Node3D
{
    [Export] public WorldSettings Settings { get; set; } = new();
    [Export] public NodePath PlayerPath { get; set; } = "Player";
    [Export] public NodePath TerrainPath { get; set; } = "Terrain";
    private PlayerController _player = null!;
    private ITerrainService _terrain = null!;
    private TreePreview? _trees;
    private Label _readout = null!;
    private bool _waiting;
    private bool _failed;
    private float _elapsed;
    private float _probeInterval;

    // =========================================================
    // Connect reusable components once; no addon-specific classes here.
    public override void _Ready()
    {
        _player = GetNode<PlayerController>(PlayerPath);
        _terrain = (ITerrainService)GetNode(TerrainPath);
        _readout = GetNode<Label>("TestHUD/Readout");
        _trees = GetNodeOrNull<TreePreview>("TreePreview");
        BeginSpawn();
        var timer = GetNode<Timer>("ReadoutTimer");
        timer.Timeout += RefreshReadout;
        RefreshReadout();
    }

    // =========================================================
    // Suspend the player until a real terrain collider exists below the spawn.
    private void BeginSpawn()
    {
        _failed = false;
        _waiting = true;
        _elapsed = 0f;
        _probeInterval = 0f;
        _player.SetMovementEnabled(false);
        _player.GlobalPosition = new Vector3(Settings.SpawnXZ.X, Settings.SpawnProbeTop, Settings.SpawnXZ.Y);
        _terrain.BindViewer(_player);
    }

    // =========================================================
    // Bounded startup collision polling; steady play only needs a cheap safety bound check.
    public override void _PhysicsProcess(double delta)
    {
        if (!_terrain.IsAvailable || _failed) return;
        if (!_waiting)
        {
            Vector3 p = _player.GlobalPosition;
            if (p.Y < Settings.KillHeight || Mathf.Abs(p.X) > Settings.PlayableHalfExtent || Mathf.Abs(p.Z) > Settings.PlayableHalfExtent)
                BeginSpawn();
            return;
        }
        _elapsed += (float)delta;
        _probeInterval -= (float)delta;
        if (_elapsed > Settings.LoadingTimeoutSeconds)
        {
            _failed = true;
            GD.PushError("Terrain spawn timed out. Inspect Voxel Tools errors; press F4 to retry.");
            RefreshReadout();
            return;
        }
        if (_probeInterval > 0f) return;
        _probeInterval = 0.1f;
        if (!_terrain.TryFindGround(Settings.SpawnXZ, Settings.SpawnProbeTop, Settings.SpawnProbeBottom, out Vector3 ground)) return;
        // A small clearance lets the native body settle onto the ground without initial overlap.
        _player.GlobalPosition = ground + Vector3.Up * 0.15f;
        _player.SetMovementEnabled(true);
        _waiting = false;
        _trees?.Initialize(_terrain, Settings.SpawnXZ, Settings.SpawnProbeTop, Settings.SpawnProbeBottom);
        RefreshReadout();
    }

    // =========================================================
    // On-demand recovery without binding terrain controls into the player.
    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event is InputEventKey key && key.Pressed && !key.Echo && key.Keycode == Key.F4) BeginSpawn();
    }

    // =========================================================
    // Small diagnostic HUD updates four times per second, not every rendered frame.
    private void RefreshReadout()
    {
        string status = !_terrain.IsAvailable ? _terrain.Status : _failed ? "Terrain load timed out — F4 retries" :
            _waiting ? "Loading terrain collision..." : "Terrain ready";
        string trees = _trees == null ? "" : $" | Firs: {_trees.PlacedCount}";
        _readout.Text = $"TERRA MACHINE — terrain test\n{status} | {Engine.GetFramesPerSecond()} FPS{trees}\n" +
            "Arrows: move   Shift: sprint   RMB: jump\nMouse: look   Esc: release   LMB: capture   F4: respawn";
    }
}
