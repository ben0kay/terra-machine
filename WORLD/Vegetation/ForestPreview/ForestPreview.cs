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
        public bool Complete, UseTall, IsSapling;
        public int SaplingVariant;
    }
    private readonly List<TreeSpot> _spots = new();
    private ITerrainSurfaceService? _terrain;
    private Node3D? _player;
    private Timer _timer = null!;
    private int _cursor, _remaining;
    private float _top, _bottom;
    private bool _started;
    private readonly TreeRenderDiagnostics _diagnostics = new();
    public int PlacedCount { get; private set; }
    public int PlannedCount => _spots.Count;
    public int SaplingsPlaced { get; private set; }
    public int SaplingsPlanned { get; private set; }
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
            // The walkable gap follows the configured forest direction (positive or negative Z).
            float direction = Settings.CenterOffset.Y >= 0f ? 1f : -1f;
            float forwardDistance = (point.Y - spawn.Y) * direction;
            float progress = Mathf.Clamp((forwardDistance - 25f) / 150f, 0f, 1f);
            float trailX = spawn.X + 9f + 14f * progress + 5f * Mathf.Sin(forwardDistance * 0.047f);
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
        // Mix tall variants into the nearest planned sites without changing layout or RNG state.
        if (Settings.TallPineTree != null && Settings.TallTreeCount > 0)
        {
            var nearest = new List<TreeSpot>(_spots);
            nearest.Sort((a, b) => a.Position.DistanceSquaredTo(spawn).CompareTo(b.Position.DistanceSquaredTo(spawn)));
            int tallCount = Mathf.Clamp(Settings.TallTreeCount, 0, nearest.Count);
            for (int i = 0; i < tallCount; i++) nearest[i].UseTall = true;
        }
        if (_spots.Count < target)
            GD.PushWarning($"Forest preview planned {_spots.Count}/{target} mature trees; spacing or radius limits further placement.");
        GenerateSaplings(randomSeed: unchecked((uint)Settings.Seed ^ 0x5F3759DFu), center, radius, spawn);
    }

    // =========================================================
    // Seed a smaller understory between mature trunks, biased toward the forest interior.
    // A separate RNG leaves the pine layout and existing tall-tree selection independent.
    private void GenerateSaplings(uint randomSeed, Vector2 center, Vector2 radius, Vector2 spawn)
    {
        if (Settings.SaplingVariants.Count == 0 || Settings.SaplingCount <= 0) return;
        var random = new RandomNumberGenerator { Seed = randomSeed };
        int target = Mathf.Clamp(Settings.SaplingCount, 0, 100);
        float innerRadius = Mathf.Clamp(Settings.SaplingCoreRadiusFraction, 0.2f, 0.95f);
        float pineClearanceSq = Mathf.Pow(Mathf.Max(1.5f, Settings.SaplingToPineClearance), 2f);
        float saplingSpacingSq = Mathf.Pow(Mathf.Max(2.5f, Settings.SaplingSpacing), 2f);
        float nearbyPineSq = Mathf.Pow(Mathf.Max(Settings.SaplingToPineClearance + 1f,
            Settings.SaplingNearPineRange), 2f);
        for (int attempt = 0; attempt < target * 120 && SaplingsPlanned < target; attempt++)
        {
            float r = Mathf.Sqrt(random.Randf()) * innerRadius;
            float angle = random.Randf() * Mathf.Tau;
            Vector2 point = center + new Vector2(Mathf.Cos(angle) * radius.X * r,
                Mathf.Sin(angle) * radius.Y * r);
            if (IsPathReserved(point, spawn, 1.4f)) continue;
            bool hasPineNearby = false, blocked = false;
            foreach (TreeSpot existing in _spots)
            {
                float d = existing.Position.DistanceSquaredTo(point);
                if (existing.IsSapling)
                {
                    if (d < saplingSpacingSq) { blocked = true; break; }
                }
                else
                {
                    if (d < pineClearanceSq) { blocked = true; break; }
                    if (d < nearbyPineSq) hasPineNearby = true;
                }
            }
            if (blocked || !hasPineNearby) continue;
            _spots.Add(new TreeSpot { Position = point, IsSapling = true,
                SaplingVariant = random.RandiRange(0, Settings.SaplingVariants.Count - 1),
                Scale = random.RandfRange(Mathf.Max(0.25f, Settings.SaplingMinimumScale),
                    Mathf.Max(Settings.SaplingMinimumScale, Settings.SaplingMaximumScale)),
                Yaw = random.Randf() * Mathf.Tau });
            SaplingsPlanned++;
        }
        if (SaplingsPlanned < target)
            GD.PushWarning($"Forest understory planned {SaplingsPlanned}/{target} fir saplings after spacing exclusions.");
    }

    // =========================================================
    // Compact smoothstep, used only during initial layout generation.
    private static float Smooth(float a, float b, float x)
    {
        float t = Mathf.Clamp((x - a) / Mathf.Max(b - a, 0.001f), 0f, 1f);
        return t * t * (3f - 2f * t);
    }
    // =========================================================
    // Reserve existing tree sites and the winding corridor for static forest props.
    // Called by one-time content generation, never from a frame callback.
    public bool IsReservedForBoulder(Vector2 position, Vector2 spawn, float clearance)
    {
        float clearanceSq = clearance * clearance;
        foreach (TreeSpot spot in _spots)
            if (spot.Position.DistanceSquaredTo(position) < clearanceSq) return true;
        return IsPathReserved(position, spawn, clearance);
    }

    // =========================================================
    // Keep paths clear for both boulder placement and new understory saplings.
    private bool IsPathReserved(Vector2 position, Vector2 spawn, float padding)
    {
        float direction = Settings.CenterOffset.Y >= 0f ? 1f : -1f;
        float forward = (position.Y - spawn.Y) * direction;
        float progress = Mathf.Clamp((forward - 25f) / 150f, 0f, 1f);
        float trailX = spawn.X + 9f + 14f * progress + 5f * Mathf.Sin(forward * 0.047f);
        return Mathf.Abs(position.X - trailX) < Settings.TrailHalfWidth + padding;
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
                // Missing distant terrain is usually streaming, not an invalid tree location.
                // Only abandon a stubborn query after repeated misses very close to the player.
                if (spot.Misses >= Mathf.Max(1, Settings.MaxGroundRetries))
                {
                    if (WithinRange(spot.Position, 35f)) Complete(spot);
                    else spot.Misses = 0;
                }
                if (!spot.Complete)
                    spot.RetryAt = now + (ulong)(Mathf.Max(0.5f, Settings.RetrySeconds) * 1000f);
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
    private bool WithinRange(Vector2 position, float rangeOverride = 0f)
    {
        if (_player == null) return false;
        Vector3 player = _player.GlobalPosition;
        float dx = position.X - player.X, dz = position.Y - player.Z;
        float range = Mathf.Max(30f, rangeOverride > 0f ? rangeOverride : Settings.PlacementRange);
        return dx * dx + dz * dz <= range * range;
    }
    #endregion

    // =========================================================
    // The global tree diagnostic includes this grove and newly placed saplings.
    public void SetDebugRendering(bool visible, bool shadows, bool unshaded, float split = 70f, int lod = 0, float farSplit = 280f)
    {
        Visible = visible;
        _diagnostics.Set(shadows, unshaded, split, lod, farSplit);
    }

    #region Visuals
    // =========================================================
    // Reuse PineTree002's existing authored near/far LOD scene; no per-tree distance script.
    private void AddTree(TreeSpot spot, Vector3 ground)
    {
        Node3D wrapper = new() {
            Name = spot.IsSapling ? $"ForestFir_{SaplingsPlaced + 1:000}" : $"ForestPine_{PlacedCount - SaplingsPlaced + 1:000}"
        };
        AddChild(wrapper);
        PackedScene scene = spot.IsSapling ? Settings.SaplingVariants[spot.SaplingVariant] :
            spot.UseTall && Settings.TallPineTree != null ? Settings.TallPineTree : Settings.PineTree!;
        Node3D model = scene.Instantiate<Node3D>();
        wrapper.AddChild(model);
        // Fir A/B/C prefabs already cancel their Blender display offsets and align trunk collision.
        // Keep their prefab root intact; the existing mature pine pivot correction stays unchanged.
        MeshInstance3D? first = FindFirstMesh(model);
        if (!spot.IsSapling && first != null) model.Position -= wrapper.ToLocal(first.GlobalPosition);
        ApplyShadows(model, false, spot.IsSapling);
        _diagnostics.Register(model);
        wrapper.Scale = Vector3.One * spot.Scale;
        wrapper.Rotation = new Vector3(0f, spot.Yaw, 0f);
        wrapper.GlobalPosition = ground - Vector3.Up * Settings.GroundSink;
        PlacedCount++;
        if (spot.IsSapling) SaplingsPlaced++;
    }

    // =========================================================
    // Keep canopy shadows near the player; skip distant LOD2 shadow drawing.
    private void ApplyShadows(Node node, bool far, bool sapling)
    {
        if (node.Name == "LOD2" || node.Name == "LOD3") far = true;
        if (node is MeshInstance3D mesh)
        {
            mesh.CastShadow = far || !Settings.NearShadows || (sapling && !Settings.SaplingShadows)
                ? GeometryInstance3D.ShadowCastingSetting.Off
                : GeometryInstance3D.ShadowCastingSetting.On;
            if (sapling) mesh.VisibilityRangeEnd = Mathf.Max(25f, Settings.SaplingCullDistance);
        }
        foreach (Node child in node.GetChildren()) ApplyShadows(child, far, sapling);
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
