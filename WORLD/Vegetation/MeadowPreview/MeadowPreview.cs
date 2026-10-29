// Bounded meadow-grass visual trial around the authored test trees.
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
    private readonly Vector2[] _lobes = new Vector2[3];
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
        if (_started || surface == null || offsets == null || offsets.Count == 0 || Settings.Material == null) return;
        foreach (PackedScene variant in Settings.Variants)
        {
            if (variant == null) continue;
            Node3D source = variant.Instantiate<Node3D>();
            Mesh? mesh = FindFirstMesh(source)?.Mesh;
            if (mesh != null) { _meshes.Add(mesh); _transforms.Add(new List<Transform3D>()); }
            source.Free();
        }
        if (_meshes.Count == 0)
        {
            GD.PushWarning("Meadow preview: no imported LOD1 meshes available.");
            return;
        }
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
        float radius = Mathf.Max(Settings.PatchRadius, Settings.TrunkClearance + 2f);
        float clearance = Mathf.Max(0.3f, Settings.TrunkClearance);
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
                    float distance = _random.RandfRange(clearance + 1f, radius * 0.7f);
                    _lobes[i] = center + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * distance;
                }
            }
            if (_sample >= Mathf.Clamp(Settings.SamplesPerTree, 1, 256)) { _active = -1; continue; }
            int lobe = _sample % _lobes.Length;
            _sample++; budget--;
            float angleLocal = _random.Randf() * Mathf.Tau;
            float distanceLocal = Mathf.Sqrt(_random.Randf()) * radius * 0.47f;
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
            basis = basis.Scaled(Vector3.One * scale);
            _transforms[variant].Add(new Transform3D(basis, ground - Vector3.Up * Settings.GroundSink));
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
        for (int i = 0; i < _meshes.Count; i++)
        {
            List<Transform3D> transforms = _transforms[i];
            if (transforms.Count == 0) continue;
            var multi = new MultiMesh {
                TransformFormat = MultiMesh.TransformFormatEnum.Transform3D,
                Mesh = _meshes[i],
                InstanceCount = transforms.Count
            };
            for (int j = 0; j < transforms.Count; j++) multi.SetInstanceTransform(j, transforms[j]);
            AddChild(new MultiMeshInstance3D {
                Name = $"Meadow_{i + 1:00}",
                Multimesh = multi,
                MaterialOverride = Settings.Material,
                CastShadow = GeometryInstance3D.ShadowCastingSetting.Off,
                VisibilityRangeEnd = Mathf.Max(60f, Settings.DrawDistance)
            });
            transforms.Clear();
        }
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
