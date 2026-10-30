// Sparse, seeded mossy boulders grounded on the flat-side forest terrain.
// One small pending-placement timer and a bounded ray budget; idle after completion.
using Godot;
using System.Collections.Generic;

public partial class BoulderPreview : Node3D
{
    [Export] public BoulderPreviewSettings Settings { get; set; } = new();
    private sealed class RockSpot
    {
        public Vector2 Position;
        public float Scale, Yaw;
        public ulong RetryAt;
        public int Misses;
        public bool Complete;
    }
    private readonly List<RockSpot> _spots = new();
    private ITerrainSurfaceService? _terrain;
    private Node3D? _player;
    private Timer _timer = null!;
    private ConvexPolygonShape3D? _collision;
    private int _cursor, _remaining;
    private float _top, _bottom;
    private bool _started;
    public int PlacedCount { get; private set; }
    public int PlannedCount => _spots.Count;
    public bool IsPlacing => _started && _remaining > 0;

    #region Startup
    // =========================================================
    // Nothing runs before grounded test-world initialization.
    public override void _Ready()
    {
        SetPhysicsProcess(false);
        _timer = new Timer { WaitTime = 0.6f };
        AddChild(_timer);
        _timer.Timeout += RefreshActivity;
        VisibilityChanged += RefreshActivity;
    }

    // =========================================================
    // Prepare a single shared hull and finite forest-linked rock positions.
    public void Initialize(ITerrainSurfaceService? terrain, Node3D player, ForestPreview? forest,
        Vector2 spawn, float probeTop, float probeBottom)
    {
        if (_started || terrain == null || forest == null || Settings.Model == null) return;
        _terrain = terrain; _player = player;
        _top = Mathf.Max(probeTop, Settings.ProbeTop); _bottom = probeBottom;
        Node3D source = Settings.Model.Instantiate<Node3D>();
        Mesh? mesh = FindFirstMesh(source)?.Mesh;
        if (mesh != null && Settings.EnableCollision) _collision = mesh.CreateConvexShape();
        source.Free();
        if (mesh == null) { GD.PushWarning("Boulder preview: imported mesh not found."); return; }
        Generate(forest, spawn);
        _remaining = _spots.Count;
        _started = true;
        RefreshActivity();
    }

    // =========================================================
    // Scatter around the configured forest, avoiding the winding gap and trees.
    private void Generate(ForestPreview forest, Vector2 spawn)
    {
        var random = new RandomNumberGenerator { Seed = unchecked((uint)Settings.Seed) };
        ForestPreviewSettings forestConfig = forest.Settings;
        Vector2 centre = spawn + forestConfig.CenterOffset;
        Vector2 radii = new(Mathf.Max(12f, forestConfig.RadiusX - 9f),
            Mathf.Max(12f, forestConfig.RadiusZ - 9f));
        int target = Mathf.Clamp(Settings.RockCount, 0, 40);
        float spacing = Mathf.Max(6f, Settings.MinimumSpacing);
        float clearance = Mathf.Max(3f, Settings.TreeClearance);
        for (int attempt = 0; attempt < target * 130 && _spots.Count < target; attempt++)
        {
            float distance = Mathf.Sqrt(random.Randf()) * 0.95f;
            float angle = random.Randf() * Mathf.Tau;
            Vector2 pos = centre + new Vector2(Mathf.Cos(angle) * radii.X * distance,
                Mathf.Sin(angle) * radii.Y * distance);
            if (pos.DistanceSquaredTo(spawn) < 30f * 30f ||
                forest.IsReservedForBoulder(pos, spawn, clearance)) continue;
            bool tooClose = false;
            foreach (RockSpot rock in _spots)
                if (rock.Position.DistanceSquaredTo(pos) < spacing * spacing) { tooClose = true; break; }
            if (tooClose) continue;
            _spots.Add(new RockSpot {
                Position = pos,
                Scale = random.RandfRange(Mathf.Max(0.2f, Settings.MinimumScale),
                    Mathf.Max(Settings.MinimumScale, Settings.MaximumScale)),
                Yaw = random.Randf() * Mathf.Tau
            });
        }
        if (_spots.Count < target)
            GD.PushWarning($"Boulder preview planned {_spots.Count}/{target} rocks due to spacing limits.");
    }
    #endregion

    #region Grounding
    // =========================================================
    // One light timer wakes short ray bursts as terrain streams near the player.
    private void RefreshActivity()
    {
        if (!_started || _remaining == 0 || !IsVisibleInTree())
        { SetPhysicsProcess(false); _timer.Stop(); return; }
        if (_timer.IsStopped()) _timer.Start();
        ulong now = Time.GetTicksMsec();
        bool hasReady = false;
        foreach (RockSpot rock in _spots)
            if (!rock.Complete && rock.RetryAt <= now && WithinRange(rock.Position)) { hasReady = true; break; }
        SetPhysicsProcess(hasReady);
    }

    // =========================================================
    // Retry unavailable collision with backoff; abandon only repeated very-near misses.
    public override void _PhysicsProcess(double delta)
    {
        if (_terrain == null || _spots.Count == 0) { SetPhysicsProcess(false); return; }
        int budget = Mathf.Clamp(Settings.QueriesPerPhysicsTick, 1, 12), visited = 0;
        ulong now = Time.GetTicksMsec();
        float minUp = Mathf.Cos(Mathf.DegToRad(Mathf.Clamp(Settings.MaximumSlopeDegrees, 0f, 50f)));
        while (budget > 0 && visited++ < _spots.Count)
        {
            RockSpot rock = _spots[_cursor];
            _cursor = (_cursor + 1) % _spots.Count;
            if (rock.Complete || rock.RetryAt > now || !WithinRange(rock.Position)) continue;
            budget--;
            if (!_terrain.TryFindSurface(rock.Position, _top, _bottom, out Vector3 ground, out Vector3 normal))
            {
                rock.Misses++;
                if (rock.Misses >= Mathf.Max(1, Settings.MaxGroundRetries))
                {
                    if (WithinRange(rock.Position, 35f)) Complete(rock);
                    else rock.Misses = 0;
                }
                if (!rock.Complete)
                    rock.RetryAt = now + (ulong)(Mathf.Max(0.5f, Settings.RetrySeconds) * 1000f);
                continue;
            }
            if (normal.Y >= minUp) AddBoulder(rock, ground);
            Complete(rock);
        }
        RefreshActivity();
    }

    // =========================================================
    // Mark placed/skipped sites; stop both callbacks once all positions are resolved.
    private void Complete(RockSpot rock)
    {
        rock.Complete = true;
        _remaining--;
        if (_remaining == 0) { SetPhysicsProcess(false); _timer.Stop(); }
    }

    // =========================================================
    // Never probe beyond the available nearby voxel terrain collision.
    private bool WithinRange(Vector2 position, float requestedRange = 0f)
    {
        if (_player == null) return false;
        Vector3 player = _player.GlobalPosition;
        float x = position.X - player.X, z = position.Y - player.Z;
        float range = Mathf.Max(30f, requestedRange > 0f ? requestedRange : Settings.PlacementRange);
        return x * x + z * z <= range * range;
    }
    #endregion

    #region Visuals
    // =========================================================
    // Instantiate a shared opaque glTF with a native renderer cutoff and reusable convex shape.
    private void AddBoulder(RockSpot rock, Vector3 ground)
    {
        var wrapper = new Node3D { Name = $"Boulder_{PlacedCount + 1:00}" };
        AddChild(wrapper);
        var model = Settings.Model!.Instantiate<Node3D>();
        wrapper.AddChild(model);
        MeshInstance3D? mesh = FindFirstMesh(model);
        if (mesh != null)
        {
            mesh.VisibilityRangeEnd = Mathf.Max(50f, Settings.DrawDistance);
            mesh.CastShadow = Settings.CastShadows
                ? GeometryInstance3D.ShadowCastingSetting.On
                : GeometryInstance3D.ShadowCastingSetting.Off;
            if (_collision != null)
            {
                var body = new StaticBody3D { Name = "Collision", CollisionLayer = 4, CollisionMask = 0 };
                wrapper.AddChild(body);
                body.GlobalTransform = mesh.GlobalTransform;
                body.AddChild(new CollisionShape3D { Shape = _collision });
            }
        }
        wrapper.Scale = Vector3.One * rock.Scale;
        wrapper.Rotation = new Vector3(0f, rock.Yaw, 0f);
        wrapper.GlobalPosition = ground - Vector3.Up * Settings.GroundSink;
        PlacedCount++;
    }

    // =========================================================
    // Find the lone imported mesh without relying on a fixed importer root node name.
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
