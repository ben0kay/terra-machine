// Bounded, seeded pine forest test: sparse edges, tighter interior, and a winding walkable gap.
// Uses terrain surface queries only while placing; native per-tree LOD handles draw distance.
using Godot;
using System.Collections.Generic;

public partial class ForestPreview : Node3D
{
    [Export] public ForestPreviewSettings Settings { get; set; } = new();
    private sealed class TreeSpot
    {
        public Vector2 Position;
        public float Spacing, Scale, Yaw;
        public ulong RetryAt;
        public int Misses;
        public bool Complete;
    }
    private readonly List<TreeSpot> _spots = new();
    private ITerrainSurfaceService? _terrain;
    private Node3D? _player;
    private Timer _timer = null!;
    private int _cursor, _remaining;
    private float _top, _bottom;
    private bool _started;
    public int PlacedCount { get; private set; }
    public int PlannedCount => _spots.Count;
    public bool IsPlacing => _started && _remaining > 0;

    #region Startup
    // =========================================================
    // Remain dormant until test-world composition supplies the terrain and player.
    public override void _Ready()
    {
        SetPhysicsProcess(false);
        _timer = new Timer { WaitTime = 0.6f };
        AddChild(_timer);
        _timer.Timeout += RefreshActivity;
        VisibilityChanged += RefreshActivity;
    }

    // =========================================================
    // Build a finite layout once; avoid overlapping the existing sixteen authored trees.
    public void Initialize(ITerrainSurfaceService? terrain, Node3D player, Vector2 spawn,
        Godot.Collections.Array<Vector2>? originalTreeOffsets, float probeTop, float probeBottom)
    {
        if (_started || terrain == null || Settings.PineTree == null) return;
        _terrain = terrain; _player = player;
        _top = Mathf.Max(probeTop, Settings.ProbeTop); _bottom = probeBottom;
        Generate(spawn, originalTreeOffsets);
        _started = true; _remaining = _spots.Count;
        RefreshActivity();
    }

    // =========================================================
    // A single, seeded spatial rejection pass: tighter spacing and higher acceptance inside.
    private void Generate(Vector2 spawn, Godot.Collections.Array<Vector2>? originalTreeOffsets)
    {
        var random = new RandomNumberGenerator { Seed = unchecked((uint)Settings.Seed) };
        Vector2 center = spawn + Settings.CenterOffset;
        Vector2 radius = new(Mathf.Max(15f, Settings.RadiusX), Mathf.Max(15f, Settings.RadiusZ));
        int target = Mathf.Clamp(Settings.TreeCount, 1, 250);
        for (int attempt = 0; attempt < target * 120 && _spots.Count < target; attempt++)
        {
            float r = Mathf.Sqrt(random.Randf());
            float angle = random.Randf() * Mathf.Tau;
            Vector2 point = center + new Vector2(Mathf.Cos(angle) * radius.X * r, Mathf.Sin(angle) * radius.Y * r);
            float edge = Smooth(0.37f, 1f, r);
            if (random.Randf() > Mathf.Lerp(0.98f, 0.52f, edge)) continue;
            // A narrow curving gap leads inward from the original tree preview.
            float progress = Mathf.Clamp((-point.Y - 45f) / 150f, 0f, 1f);
            float trailX = spawn.X + 9f + 14f * progress + 5f * Mathf.Sin(point.Y * 0.047f);
            if (Mathf.Abs(point.X - trailX) < Mathf.Max(1f, Settings.TrailHalfWidth)) continue;
            float spacing = Mathf.Lerp(Mathf.Max(4f, Settings.CenterSpacing),
                Mathf.Max(Settings.CenterSpacing, Settings.EdgeSpacing), edge);
            bool blocked = false;
            if (originalTreeOffsets != null)
                foreach (Vector2 offset in originalTreeOffsets)
                    if (point.DistanceSquaredTo(spawn + offset) < Settings.OriginalTreeClearance * Settings.OriginalTreeClearance)
                    { blocked = true; break; }
            if (blocked) continue;
            foreach (TreeSpot existing in _spots)
                if (point.DistanceSquaredTo(existing.Position) < Mathf.Pow(Mathf.Max(spacing, existing.Spacing), 2f))
                { blocked = true; break; }
            if (blocked) continue;
            _spots.Add(new TreeSpot { Position = point, Spacing = spacing,
                Scale = random.RandfRange(Mathf.Max(0.4f, Settings.MinimumScale),
                    Mathf.Max(Settings.MinimumScale, Settings.MaximumScale)), Yaw = random.Randf() * Mathf.Tau });
        }
        if (_spots.Count < target)
            GD.PushWarning($"Forest preview planned {_spots.Count}/{target} trees; spacing or radius limits further placement.");
    }

    // =========================================================
    // Compact smoothstep, used only during initial layout generation.
    private static float Smooth(float a, float b, float x)
    {
        float t = Mathf.Clamp((x - a) / Mathf.Max(b - a, 0.001f), 0f, 1f);
        return t * t * (3f - 2f * t);
    }
    #endregion

    #region Grounding
    // =========================================================
    // A single low-frequency timer watches for unplaced trees near the moving player.
    private void RefreshActivity()
    {
        if (!_started || _remaining == 0 || !IsVisibleInTree())
        { SetPhysicsProcess(false); _timer.Stop(); return; }
        if (_timer.IsStopped()) _timer.Start();
        bool ready = false;
        ulong now = Time.GetTicksMsec();
        foreach (TreeSpot spot in _spots)
            if (!spot.Complete && spot.RetryAt <= now && WithinRange(spot.Position)) { ready = true; break; }
        SetPhysicsProcess(ready);
    }

    // =========================================================
    // Only nearby positions query collision, with capped work and retry backoff.
    public override void _PhysicsProcess(double delta)
    {
        if (_terrain == null || _spots.Count == 0) { SetPhysicsProcess(false); return; }
        int budget = Mathf.Clamp(Settings.QueriesPerPhysicsTick, 1, 16), visited = 0;
        ulong now = Time.GetTicksMsec();
        float minUp = Mathf.Cos(Mathf.DegToRad(Mathf.Clamp(Settings.MaximumSlopeDegrees, 0f, 60f)));
        while (budget > 0 && visited++ < _spots.Count)
        {
            TreeSpot spot = _spots[_cursor];
            _cursor = (_cursor + 1) % _spots.Count;
            if (spot.Complete || spot.RetryAt > now || !WithinRange(spot.Position)) continue;
            budget--;
            if (!_terrain.TryFindSurface(spot.Position, _top, _bottom, out Vector3 ground, out Vector3 normal))
            {
                spot.Misses++;
                if (spot.Misses >= Mathf.Max(1, Settings.MaxGroundRetries)) Complete(spot);
                else spot.RetryAt = now + (ulong)(Mathf.Max(0.5f, Settings.RetrySeconds) * 1000f);
                continue;
            }
            if (normal.Y >= minUp) AddTree(spot, ground);
            Complete(spot);
        }
        RefreshActivity();
    }

    // =========================================================
    // Track skipped cliffs and completed placements without permanent processing.
    private void Complete(TreeSpot spot)
    {
        spot.Complete = true; _remaining--;
        if (_remaining == 0) { SetPhysicsProcess(false); _timer.Stop(); }
    }

    // =========================================================
    // The test forest is finite, but far-off points should not probe unloaded chunks.
    private bool WithinRange(Vector2 position)
    {
        if (_player == null) return false;
        Vector3 player = _player.GlobalPosition;
        float dx = position.X - player.X, dz = position.Y - player.Z;
        float range = Mathf.Max(35f, Settings.PlacementRange);
        return dx * dx + dz * dz <= range * range;
    }
    #endregion

    #region Visuals
    // =========================================================
    // Reuse PineTree002's existing authored near/far LOD scene; no per-tree distance script.
    private void AddTree(TreeSpot spot, Vector3 ground)
    {
        Node3D wrapper = new() { Name = $"ForestPine_{PlacedCount + 1:000}" };
        AddChild(wrapper);
        Node3D model = Settings.PineTree!.Instantiate<Node3D>();
        wrapper.AddChild(model);
        MeshInstance3D? first = FindFirstMesh(model);
        if (first != null) model.Position -= wrapper.ToLocal(first.GlobalPosition);
        ApplyShadows(model, false);
        wrapper.Scale = Vector3.One * spot.Scale;
        wrapper.Rotation = new Vector3(0f, spot.Yaw, 0f);
        wrapper.GlobalPosition = ground - Vector3.Up * Settings.GroundSink;
        PlacedCount++;
    }

    // =========================================================
    // Keep canopy shadows near the player; skip distant LOD2 shadow drawing.
    private void ApplyShadows(Node node, bool far)
    {
        if (node.Name == "LOD2") far = true;
        if (node is MeshInstance3D mesh)
            mesh.CastShadow = far || !Settings.NearShadows
                ? GeometryInstance3D.ShadowCastingSetting.Off
                : GeometryInstance3D.ShadowCastingSetting.On;
        foreach (Node child in node.GetChildren()) ApplyShadows(child, far);
    }

    // =========================================================
    // Read imported geometry once per placed tree to preserve PineTree002 pivot alignment.
    private static MeshInstance3D? FindFirstMesh(Node node)
    {
        if (node is MeshInstance3D mesh) return mesh;
        foreach (Node child in node.GetChildren())
        {
            MeshInstance3D? found = FindFirstMesh(child);
            if (found != null) return found;
        }
        return null;
    }
    #endregion
}
