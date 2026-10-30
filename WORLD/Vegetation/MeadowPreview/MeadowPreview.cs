// Bounded clumped vegetation preview: tree-adjacent meadow or finite forest-floor sites.
// Grounds shared MultiMesh tufts with a fixed physics-ray budget, then sleeps.
using Godot;
using System.Collections.Generic;

public partial class MeadowPreview : Node3D
{
    [Export] public MeadowPreviewSettings Settings { get; set; } = new();
    private ITerrainSurfaceService? _surface;
    private Godot.Collections.Array<Vector2>? _offsets;
    private readonly List<Mesh> _meshes = new();
    private readonly List<List<Transform3D>> _transforms = new();
    private readonly Queue<int> _work = new(), _retries = new();
    private Vector2[] _lobes = new Vector2[3];
    private readonly List<Basis> _sourceBases = new();
    private readonly List<Shape3D?> _shapes = new();
    private readonly Dictionary<Material, Material> _materials = new();
    private readonly RandomNumberGenerator _random = new();
    private Timer _retryTimer = null!;
    private Vector2 _origin;
    private float _top, _bottom;
    private ulong _startMs;
    private int _active = -1, _sample;
    private bool _started, _finished;

    public int PlacedCount { get; private set; }
    public bool IsPlacing => _started && !_finished;

    #region Lifecycle
    // =========================================================
    // No idle physics; wake only for initial placement and bounded retries.
    public override void _Ready()
    {
        SetPhysicsProcess(false);
        _retryTimer = new Timer { OneShot = true };
        AddChild(_retryTimer);
        _retryTimer.Timeout += RetryPending;
        VisibilityChanged += UpdateActivity;
    }

    // =========================================================
    // Reuse TreePreview's authored positions; do not create a second position list.
    public void Initialize(ITerrainSurfaceService? surface, Godot.Collections.Array<Vector2>? offsets,
        Vector2 origin, float top, float bottom)
    {
        if (_started || surface == null || offsets == null || offsets.Count == 0) return;
        foreach (PackedScene variant in Settings.Variants)
        {
            if (variant == null) continue;
            Node3D source = variant.Instantiate<Node3D>();
            if (Settings.CollectAllMeshes) CollectMeshes(source, Basis.Identity);
            else
            {
                MeshInstance3D? first = FindFirstMesh(source);
                if (first?.Mesh != null) AddMesh(first.Mesh, first.Basis);
            }
            source.Free();
        }
        if (_meshes.Count == 0)
        {
            GD.PushWarning("Meadow preview: no imported LOD1 meshes available.");
            return;
        }
        _lobes = new Vector2[Mathf.Clamp(Settings.ClumpsPerSite, 1, 8)];
        _started = true;
        _surface = surface; _offsets = offsets; _origin = origin; _top = top; _bottom = bottom;
        _startMs = Time.GetTicksMsec();
        for (int i = 0; i < offsets.Count; i++) _work.Enqueue(i);
        UpdateActivity();
    }

    // =========================================================
    // F1 visibility disables placement/retry work; turning it back on resumes.
    private void UpdateActivity()
    {
        if (!_started || _finished) return;
        if (!IsVisibleInTree()) { SetPhysicsProcess(false); _retryTimer.Stop(); return; }
        if (_active < 0 && _work.Count == 0)
            while (_retries.Count > 0) _work.Enqueue(_retries.Dequeue());
        SetPhysicsProcess(_active >= 0 || _work.Count > 0);
    }

    // =========================================================
    // Retry unavailable terrain later, rather than querying unloaded collision each frame.
    private void RetryPending()
    {
        if (_finished || !IsVisibleInTree()) return;
        while (_retries.Count > 0) _work.Enqueue(_retries.Dequeue());
        SetPhysicsProcess(_work.Count > 0);
    }
    #endregion

    // =========================================================
    // Finite, spaced sites across a forest ellipse; centres are shared with the same batching path.
    public void InitializeArea(ITerrainSurfaceService? surface, Vector2 origin, Vector2 centerOffset,
        Vector2 radii, float top, float bottom)
    {
        if (_started) return;
        var sites = new Godot.Collections.Array<Vector2>();
        var random = new RandomNumberGenerator { Seed = unchecked((uint)Settings.Seed) };
        int target = Mathf.Clamp(Settings.SiteCount, 1, 64);
        foreach (Vector2 offset in Settings.PreviewOffsets)
            if (sites.Count < target) sites.Add(offset);
        for (int attempt = 0; attempt < target * 100 && sites.Count < target; attempt++)
        {
            float angle = random.Randf() * Mathf.Tau, r = Mathf.Sqrt(random.Randf());
            Vector2 site = centerOffset + new Vector2(Mathf.Cos(angle) * radii.X, Mathf.Sin(angle) * radii.Y) * r;
            bool blocked = false;
            foreach (Vector2 other in sites)
                if (other.DistanceSquaredTo(site) < Settings.SiteSpacing * Settings.SiteSpacing) { blocked = true; break; }
            if (!blocked) sites.Add(site);
        }
        Initialize(surface, sites, origin, top, bottom);
    }

    // =========================================================
    // Treat imported siblings as individual variants; their translations are Blender display layout.
    private void CollectMeshes(Node node, Basis parentBasis)
    {
        Basis basis = node is Node3D spatial ? parentBasis * spatial.Basis : parentBasis;
        if (node is MeshInstance3D visual && visual.Mesh != null) AddMesh(visual.Mesh, basis);
        foreach (Node child in node.GetChildren()) CollectMeshes(child, basis);
    }

    // =========================================================
    // Share configured mesh/materials and cook at most one convex shape per imported variant.
    private void AddMesh(Mesh source, Basis basis)
    {
        Mesh mesh = source;
        if (Settings.Material == null)
        {
            mesh = (Mesh)source.Duplicate();
            for (int i = 0; i < mesh.GetSurfaceCount(); i++)
            {
                Material original = source.SurfaceGetMaterial(i);
                if (original is not StandardMaterial3D standard) continue;
                if (!_materials.TryGetValue(original, out Material? material))
                {
                    var configured = (StandardMaterial3D)standard.Duplicate();
                    configured.TextureFilter = BaseMaterial3D.TextureFilterEnum.LinearWithMipmapsAnisotropic;
                    if (configured.Transparency == BaseMaterial3D.TransparencyEnum.Alpha)
                    {
                        configured.Transparency = BaseMaterial3D.TransparencyEnum.AlphaScissor;
                        configured.AlphaScissorThreshold = 0.4f;
                    }
                    material = configured; _materials.Add(original, material);
                }
                mesh.SurfaceSetMaterial(i, material);
            }
        }
        _meshes.Add(mesh); _sourceBases.Add(basis);
        _shapes.Add(StaticCollisionBuilder.Prepare(Settings.Collision, mesh));
        _transforms.Add(new List<Transform3D>());
    }

    #region GroundedPlacement
    // =========================================================
    // Spread irregular clumps near trees, rejecting trunks, steep ground and missing collision.
    public override void _PhysicsProcess(double delta)
    {
        if (_surface == null || _offsets == null) { Finish(); return; }
        if (TimedOut())
        {
            if (_work.Count > 0 || _retries.Count > 0 || _active >= 0)
                GD.PushWarning("Meadow preview: some tree patches were skipped while terrain collision loaded.");
            Finish(); return;
        }
        int budget = Mathf.Clamp(Settings.QueriesPerPhysicsTick, 1, 128);
        float clearance = Mathf.Max(0f, Settings.TrunkClearance);
        float radius = Mathf.Max(Settings.PatchRadius, clearance + Settings.ClumpRadius);
        float minUp = Mathf.Cos(Mathf.DegToRad(Mathf.Clamp(Settings.MaximumSlopeDegrees, 0f, 60f)));
        while (budget > 0)
        {
            if (_active < 0)
            {
                if (_work.Count == 0)
                {
                    SetPhysicsProcess(false);
                    if (_retries.Count == 0) Finish();
                    else _retryTimer.Start(Mathf.Clamp(Settings.RetrySeconds, 0.2f, 2f));
                    return;
                }
                int next = _work.Dequeue();
                budget--;
                Vector2 center = _origin + _offsets[next];
                if (!_surface.TryFindSurface(center, _top, _bottom, out _, out _))
                {
                    _retries.Enqueue(next);
                    continue;
                }
                _active = next; _sample = 0;
                _random.Seed = unchecked((uint)(Settings.Seed + next * 16127));
                float startAngle = _random.Randf() * Mathf.Tau;
                for (int i = 0; i < _lobes.Length; i++)
                {
                    float angle = startAngle + i * Mathf.Tau / _lobes.Length + _random.RandfRange(-0.45f, 0.45f);
                    float distance = _random.RandfRange(clearance, Mathf.Max(clearance, radius - Settings.ClumpRadius));
                    _lobes[i] = center + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * distance;
                }
            }
            if (budget <= 0) break;
            if (_sample >= Mathf.Clamp(Settings.SamplesPerTree, 1, 256))
            {
                if (Settings.CollectAllMeshes) FlushBatches();
                _active = -1; continue;
            }
            int lobe = _sample % _lobes.Length;
            _sample++; budget--;
            float angleLocal = _random.Randf() * Mathf.Tau;
            float distanceLocal = Mathf.Sqrt(_random.Randf()) * Mathf.Max(0.01f, Settings.ClumpRadius);
            Vector2 position = _lobes[lobe] + new Vector2(Mathf.Cos(angleLocal), Mathf.Sin(angleLocal)) * distanceLocal;
            Vector2 centerXZ = _origin + _offsets[_active];
            float distanceSquared = position.DistanceSquaredTo(centerXZ);
            if (distanceSquared < clearance * clearance || distanceSquared > radius * radius) continue;
            if (!_surface.TryFindSurface(position, _top, _bottom, out Vector3 ground, out Vector3 normal)
                || normal.Y < minUp) continue;
            int variant = _random.RandiRange(0, _meshes.Count - 1);
            float scale = _random.RandfRange(Mathf.Max(0.1f, Settings.MinimumScale),
                Mathf.Max(Settings.MinimumScale, Settings.MaximumScale));
            Basis basis = new Basis(new Quaternion(Vector3.Up, normal.Normalized()))
                * new Basis(Vector3.Up, _random.Randf() * Mathf.Tau);
            basis = basis.Scaled(Vector3.One * scale) * _sourceBases[variant];
            Vector3 position3D = ground - normal * Settings.GroundSink;
            if (Settings.GroundAtMeshBottom) position3D -= normal * (_meshes[variant].GetAabb().Position.Y * scale);
            Transform3D transform = new(basis, ToLocal(position3D));
            _transforms[variant].Add(transform);
            if (_shapes[variant] != null)
            {
                var owner = new Node3D { Name = $"Branch_{PlacedCount + 1:00}", Transform = transform };
                AddChild(owner);
                StaticCollisionBuilder.Attach(owner, Settings.Collision, _shapes[variant], Transform3D.Identity);
            }
            PlacedCount++;
        }
    }

    // =========================================================
    // One renderer batch per authored variant. No per-tuft scene nodes or CPU wind updates.
    private void Finish()
    {
        if (_finished) return;
        _finished = true;
        SetPhysicsProcess(false); _retryTimer.Stop();
        FlushBatches();
        GD.Print($"{Name}: placed {PlacedCount} instances across {_offsets?.Count ?? 0} planned sites.");
    }

    // =========================================================
    // Publish each completed floor patch immediately, even if other sites await terrain.
    private void FlushBatches()
    {
        for (int i = 0; i < _meshes.Count; i++)
        {
            List<Transform3D> transforms = _transforms[i];
            if (transforms.Count == 0) continue;
            // Small spatial batches allow nearby patches to render independently of distant ones.
            if (Settings.CollectAllMeshes)
            {
                var tiles = new Dictionary<Vector2I, List<Transform3D>>();
                foreach (Transform3D transform in transforms)
                {
                    var key = new Vector2I(Mathf.FloorToInt(transform.Origin.X / 16f), Mathf.FloorToInt(transform.Origin.Z / 16f));
                    if (!tiles.TryGetValue(key, out List<Transform3D>? tile)) { tile = new(); tiles.Add(key, tile); }
                    tile.Add(transform);
                }
                foreach (var tile in tiles) AddBatch(i, tile.Value);
            }
            else AddBatch(i, transforms);
            transforms.Clear();
        }
    }

    // =========================================================
    // Native renderer culling per spatial group; no per-instance distance checks.
    private void AddBatch(int variant, List<Transform3D> transforms)
    {
        var multi = new MultiMesh {
            TransformFormat = MultiMesh.TransformFormatEnum.Transform3D,
            Mesh = _meshes[variant], InstanceCount = transforms.Count
        };
        for (int j = 0; j < transforms.Count; j++) multi.SetInstanceTransform(j, transforms[j]);
        AddChild(new MultiMeshInstance3D {
            Name = $"Patch_{variant + 1:00}", Multimesh = multi, MaterialOverride = Settings.Material,
            CastShadow = GeometryInstance3D.ShadowCastingSetting.Off,
            VisibilityRangeEnd = Mathf.Max(20f, Settings.DrawDistance)
        });
    }

    // =========================================================
    // Stop unresolved retry attempts after a bounded loading window.
    private bool TimedOut() => Time.GetTicksMsec() - _startMs >=
        (ulong)(Mathf.Max(1f, Settings.LoadingTimeoutSeconds) * 1000f);

    // =========================================================
    // Each imported meadow glTF contains one visual mesh under its scene root.
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
