// Streams a bounded ring of deterministic grass batches, using ordinary surface queries.
// One timer watches tile crossings/draw density; physics sleeps when placement is complete.
using Godot;
using System;
using System.Collections.Generic;

public partial class GrassScatter : Node3D
{
    [Export] public GrassScatterSettings Settings { get; set; } = new();
    private sealed class Tile
    {
        public Vector2I Key;
        public int Cursor, Attempts;
        public bool CenterChecked, Done;
        public ulong RetryAt;
        public readonly List<(int Variant, Transform3D Transform)> Transforms = new();
        public Node3D? Batch;
        public readonly List<MultiMesh> MeshBatches = new();
        public RandomNumberGenerator Random = new();
    }
    private readonly Dictionary<Vector2I, Tile> _tiles = new();
    private readonly Queue<Tile> _jobs = new();
    private readonly List<Mesh> _meshes = new();
    private readonly List<Mesh> _farMeshes = new();
    private readonly List<Vector3> _pivots = new();
    private ITerrainSurfaceService? _surface;
    private Node3D? _player;
    private ShaderMaterial _material = null!;
    private Timer _timer = null!;
    private Vector2I _center = new(int.MaxValue, int.MaxValue);
    private float _size, _range, _minimumUp, _dense, _sparse;
    private int _candidates, _gridSide;
    private int[] _cellOrder = Array.Empty<int>();
    private bool _initialized, _areaEnabled;
    private Vector2 _areaCenter, _areaRadii;
    public bool IsPlacing
    {
        get
        {
            if (!IsVisibleInTree()) return false;
            foreach (Tile tile in _tiles.Values) if (!tile.Done) return true;
            return false;
        }
    }

    // =========================================================
    // Initialize only after a grounded spawn, with no scene-tree searches during steady play.
    public override void _Ready()
    {
        SetPhysicsProcess(false);
        _timer = new Timer(); AddChild(_timer);
        _timer.Timeout += RefreshTiles;
        VisibilityChanged += UpdateActivity;
    }

    // =========================================================
    // Extract selected mesh resources once; ignore Blender's display-layout node translations.
    public void Initialize(ITerrainSurfaceService? surface, Node3D player,
        Vector2? areaCenter = null, Vector2? areaRadii = null)
    {
        if (_initialized || surface == null || Settings.Material == null) return;
        if (Settings.NearScenes.Count > 0)
        {
            // Separate glTF source scenes are useful for authored meadow LOD pairs.
            for (int i = 0; i < Settings.NearScenes.Count; i++)
            {
                Mesh? near = LoadFirstMesh(Settings.NearScenes[i]);
                if (near == null) continue;
                _meshes.Add(near);
                Aabb bounds = near.GetAabb();
                _pivots.Add(new Vector3(bounds.GetCenter().X, bounds.Position.Y, bounds.GetCenter().Z));
                Mesh? far = i < Settings.FarScenes.Count ? LoadFirstMesh(Settings.FarScenes[i]) : null;
                _farMeshes.Add(far ?? near);
            }
            if (Settings.FarScenes.Count != Settings.NearScenes.Count)
                GD.PushWarning("Grass scatter: authored LOD scene counts differ; using near meshes as fallback.");
        }
        else if (Settings.Variants != null)
        {
            Node3D source = Settings.Variants.Instantiate<Node3D>();
            foreach (string name in Settings.MeshNames)
            {
                var meshNode = source.FindChild(name, true, false) as MeshInstance3D;
                if (meshNode?.Mesh == null) continue;
                Mesh mesh = meshNode.Mesh;
                Aabb bounds = mesh.GetAabb();
                _meshes.Add(mesh);
                _pivots.Add(new Vector3(bounds.GetCenter().X, bounds.Position.Y, bounds.GetCenter().Z));
            }
            source.Free();
        }
        if (_meshes.Count == 0) { GD.PushWarning("Grass scatter found no configured variant meshes."); return; }
        _areaEnabled = Settings.HideOutsideArea && areaCenter.HasValue && areaRadii.HasValue;
        if (_areaEnabled)
        {
            _areaCenter = areaCenter!.Value;
            _areaRadii = new Vector2(Mathf.Max(1f, areaRadii!.Value.X), Mathf.Max(1f, areaRadii.Value.Y));
        }
        _surface = surface; _player = player; _initialized = true;
        _size = Mathf.Clamp(Settings.TileSize, 8f, 32f);
        _range = Mathf.Clamp(Settings.EndDistance, 15f, 80f);
        _minimumUp = Mathf.Cos(Mathf.DegToRad(Mathf.Clamp(Settings.MaximumSlopeDegrees, 0f, 45f)));
        _candidates = Mathf.Clamp((int)(_size * _size * Settings.TuftsPerSquareMeter), 1, 8192);
        _gridSide = Mathf.CeilToInt(Mathf.Sqrt(_candidates));
        _cellOrder = new int[_gridSide * _gridSide];
        var cellRandom = new RandomNumberGenerator { Seed = unchecked((uint)Settings.Seed) };
        for (int i = 0; i < _cellOrder.Length; i++) _cellOrder[i] = i;
        for (int i = _cellOrder.Length - 1; i > 0; i--)
        {
            int other = cellRandom.RandiRange(0, i);
            (_cellOrder[i], _cellOrder[other]) = (_cellOrder[other], _cellOrder[i]);
        }
        _material = (ShaderMaterial)Settings.Material.Duplicate();
        _dense = Mathf.Clamp(Settings.DenseDistance, 1f, _range - 2f);
        _sparse = Mathf.Clamp(Settings.SparseDistance, _dense + 1f, _range - 1f);
        _material.SetShaderParameter("sparse_distance", _sparse);
        _material.SetShaderParameter("end_distance", _range);
        _timer.WaitTime = Mathf.Max(Settings.TileCheckSeconds, 0.1f);
        UpdateActivity();
    }

    // =========================================================
    // Load the one authored mesh from each distinct meadow glTF only during initialization.
    private static Mesh? LoadFirstMesh(PackedScene sourceScene)
    {
        if (sourceScene == null) return null;
        Node3D source = sourceScene.Instantiate<Node3D>();
        Mesh? mesh = FindFirstMesh(source)?.Mesh;
        source.Free();
        return mesh;
    }

    // =========================================================
    // Locate a scene's visual mesh without hard-coding its imported node name.
    private static MeshInstance3D? FindFirstMesh(Node node)
    {
        if (node is MeshInstance3D mesh && mesh.Mesh != null) return mesh;
        foreach (Node child in node.GetChildren())
        {
            MeshInstance3D? found = FindFirstMesh(child);
            if (found != null) return found;
        }
        return null;
    }

    // =========================================================
    // Visibility toggles suspend both placement work and the crossing timer, then resume on demand.
    private void UpdateActivity()
    {
        if (!_initialized) return;
        bool active = IsVisibleInTree();
        SetPhysicsProcess(active && _jobs.Count > 0);
        if (active) { _timer.Start(); RefreshTiles(); } else _timer.Stop();
    }

    // =========================================================
    // Rebuild a small nearest-first job queue only on tile crossings or bounded collision retries.
    private void RefreshTiles()
    {
        if (_player == null || !IsVisibleInTree()) return;
        Vector3 p = _player.GlobalPosition;
        foreach (Tile tile in _tiles.Values) UpdateDensity(tile, p);
        var center = new Vector2I(Mathf.FloorToInt(p.X / _size), Mathf.FloorToInt(p.Z / _size));
        ulong now = Time.GetTicksMsec();
        bool changed = center != _center;
        bool retry = false;
        foreach (Tile tile in _tiles.Values)
            if (!tile.Done && tile.RetryAt > 0 && tile.RetryAt <= now) { retry = true; break; }
        if (!changed && !retry) return;
        _center = center;
        int radius = Mathf.CeilToInt(_range / _size) + 1;
        var wanted = new HashSet<Vector2I>();
        var ordered = new List<Vector2I>();
        Vector2 anchor = (new Vector2(center.X, center.Y) + Vector2.One * 0.5f) * _size;
        for (int z = -radius; z <= radius; z++)
        for (int x = -radius; x <= radius; x++)
        {
            var key = center + new Vector2I(x, z);
            Vector2 tileCenter = (new Vector2(key.X, key.Y) + Vector2.One * 0.5f) * _size;
            // Conservative ring includes tile corners and player motion within the current tile.
            if (tileCenter.DistanceTo(anchor) > _range + _size * 1.42f) continue;
            if (_areaEnabled && !InsideArea(tileCenter, _size)) continue;
            wanted.Add(key); ordered.Add(key);
        }
        var remove = new List<Vector2I>();
        foreach (var entry in _tiles)
            if (!wanted.Contains(entry.Key)) { entry.Value.Batch?.QueueFree(); remove.Add(entry.Key); }
        foreach (Vector2I key in remove) _tiles.Remove(key);
        ordered.Sort((a, b) => (a - center).LengthSquared().CompareTo((b - center).LengthSquared()));
        _jobs.Clear();
        foreach (Vector2I key in ordered)
        {
            if (!_tiles.TryGetValue(key, out Tile? tile))
            {
                uint seed = unchecked((uint)(key.X * 73856093 ^ key.Y * 19349663 ^ Settings.Seed));
                tile = new Tile { Key = key };
                tile.Random.Seed = seed; _tiles.Add(key, tile);
            }
            if (!tile.Done && tile.RetryAt <= now) { tile.RetryAt = 0; _jobs.Enqueue(tile); }
        }
        SetPhysicsProcess(_jobs.Count > 0);
    }

    // =========================================================
    // Bound startup/travel ray work. Placement never invokes native voxel calls directly.
    public override void _PhysicsProcess(double delta)
    {
        if (_surface == null || _player == null) return;
        int rays = Mathf.Clamp(Settings.QueriesPerPhysicsTick, 1, 256), visits = 0;
        while (_jobs.Count > 0 && rays > 0 && visits++ < 512)
        {
            Tile tile = _jobs.Peek();
            Vector2 origin = new(tile.Key.X * _size, tile.Key.Y * _size);
            float top = _player.GlobalPosition.Y + 96f, bottom = _player.GlobalPosition.Y - 192f;
            if (!tile.CenterChecked)
            {
                rays--;
                if (!_surface.TryFindSurface(origin + Vector2.One * (_size * 0.5f), top, bottom, out _, out _))
                {
                    tile.Attempts++; tile.Done = tile.Attempts >= 4;
                    tile.RetryAt = Time.GetTicksMsec() + 2000;
                    _jobs.Dequeue(); continue;
                }
                tile.CenterChecked = true;
                if (rays == 0) break;
            }
            // Stratify candidates across the whole tile; jitter preserves a natural layout.
            // Shared shuffled cell order keeps distance-thinned prefixes distributed.
            int cell = _cellOrder[tile.Cursor];
            float x = origin.X + (cell % _gridSide + tile.Random.Randf()) * (_size / _gridSide);
            float z = origin.Y + (cell / _gridSide + tile.Random.Randf()) * (_size / _gridSide);
            tile.Cursor++;
            if ((_areaEnabled && !InsideArea(new Vector2(x, z))) || !InsidePatch(new Vector2(x, z)))
            {
                // Even a rejected final sample must retire its tile before the next tick.
                if (tile.Cursor >= _candidates) { CompleteTile(tile, origin); _jobs.Dequeue(); }
                continue;
            }
            // Keep a continuous meadow base; broad variation changes fullness without bare bands.
            float patch = 0.5f + 0.3f * Mathf.Sin(x * 0.19f + Mathf.Sin(z * 0.11f)) + 0.2f * Mathf.Sin(z * 0.27f + x * 0.07f);
            if (tile.Random.Randf() < Mathf.Lerp(Mathf.Clamp(Settings.MinimumPatchCoverage, 0.1f, 0.98f), 0.98f, Mathf.Clamp(patch, 0f, 1f)))
            {
                rays--;
                if (_surface.TryFindSurface(new Vector2(x, z), top, bottom, out Vector3 ground, out Vector3 normal) && normal.Y >= _minimumUp)
                {
                    int variant = tile.Random.RandiRange(0, _meshes.Count - 1);
                    float scale = tile.Random.RandfRange(Mathf.Max(0.1f, Settings.MinimumScale), Mathf.Max(Settings.MinimumScale, Settings.MaximumScale));
                    Basis basis = new Basis(new Quaternion(Vector3.Up, normal.Normalized())) * new Basis(Vector3.Up, tile.Random.Randf() * Mathf.Tau);
                    basis = basis.Scaled(Vector3.One * scale);
                    Vector3 position = ground - Vector3.Up * 0.015f - basis * _pivots[variant];
                    position -= new Vector3(origin.X, 0f, origin.Y);
                    tile.Transforms.Add((variant, new Transform3D(basis, position)));
                }
            }
            if (tile.Cursor < _candidates) continue;
            CompleteTile(tile, origin); _jobs.Dequeue();
        }
        if (_jobs.Count == 0) SetPhysicsProcess(false);
    }

    // =========================================================
    // World-anchored, jittered patch centres stay continuous across streamed tile edges.
    // Disabled by default so the Bermuda base retains its existing coverage.
    private bool InsidePatch(Vector2 point)
    {
        if (Settings.PatchSpacing <= 0f) return true;
        float spacing = Mathf.Max(2f, Settings.PatchSpacing);
        int cx = Mathf.FloorToInt(point.X / spacing), cz = Mathf.FloorToInt(point.Y / spacing);
        for (int z = cz - 1; z <= cz + 1; z++)
        for (int x = cx - 1; x <= cx + 1; x++)
        {
            uint hash = unchecked((uint)(x * 73856093 ^ z * 19349663 ^ Settings.Seed));
            float jx = PatchRandom(hash), jz = PatchRandom(hash + 1u);
            Vector2 center = new((x + 0.15f + jx * 0.7f) * spacing, (z + 0.15f + jz * 0.7f) * spacing);
            float radius = Mathf.Clamp(Settings.PatchRadius, 0.2f, spacing * 0.45f) * Mathf.Lerp(0.75f, 1.2f, PatchRandom(hash + 2u));
            if (point.DistanceSquaredTo(center) <= radius * radius) return true;
        }
        return false;
    }

    // =========================================================
    // Stateless integer mixing avoids resource allocations or RNG reseeding per sample.
    private static float PatchRandom(uint value)
    {
        unchecked { value ^= value >> 16; value *= 0x7feb352du; value ^= value >> 15; value *= 0x846ca68bu; value ^= value >> 16; }
        return (value & 0x00ffffffu) / 16777216f;
    }

    // =========================================================
    // Bounds check for optional forest ellipse, with padding only for tile discovery.
    private bool InsideArea(Vector2 position, float padding = 0f)
    {
        float dx = (position.X - _areaCenter.X) / (_areaRadii.X + padding);
        float dz = (position.Y - _areaCenter.Y) / (_areaRadii.Y + padding);
        return dx * dx + dz * dz <= 1f;
    }

    // =========================================================
    // One MultiMesh per variant/tile/LOD; regular Bermuda retains its original single batch.
    private void CompleteTile(Tile tile, Vector2 origin)
    {
        tile.Done = true;
        if (tile.Transforms.Count == 0) return;
        tile.Batch = new Node3D { Name = $"Grass_{tile.Key.X}_{tile.Key.Y}" };
        AddChild(tile.Batch); tile.Batch.GlobalPosition = new Vector3(origin.X, 0f, origin.Y);
        for (int variant = 0; variant < _meshes.Count; variant++)
        {
            int count = 0;
            foreach (var item in tile.Transforms) if (item.Variant == variant) count++;
            if (count == 0) continue;
            var multi = new MultiMesh { TransformFormat = MultiMesh.TransformFormatEnum.Transform3D, Mesh = _meshes[variant], InstanceCount = count };
            MultiMesh? farMulti = _farMeshes.Count > 0
                ? new MultiMesh { TransformFormat = MultiMesh.TransformFormatEnum.Transform3D, Mesh = _farMeshes[variant], InstanceCount = count }
                : null;
            int index = 0;
            float minY = float.MaxValue, maxY = float.MinValue;
            foreach (var item in tile.Transforms)
            {
                if (item.Variant != variant) continue;
                multi.SetInstanceTransform(index, item.Transform);
                farMulti?.SetInstanceTransform(index, item.Transform);
                minY = Mathf.Min(minY, item.Transform.Origin.Y);
                maxY = Mathf.Max(maxY, item.Transform.Origin.Y);
                index++;
            }
            // Identical tile bounds make near/far native visibility ranges switch together.
            Aabb bounds = new(new Vector3(-2f, minY - 2f, -2f),
                new Vector3(_size + 4f, maxY - minY + 8f, _size + 4f));
            float switchDistance = Mathf.Clamp(Settings.LodSwitchDistance, 5f, _range - 6f);
            float margin = 1.5f;
            var batch = new MultiMeshInstance3D { Multimesh = multi, MaterialOverride = _material,
                CastShadow = GeometryInstance3D.ShadowCastingSetting.Off,
                VisibilityRangeEnd = farMulti == null ? _range + _size * 1.5f : switchDistance };
            if (farMulti != null)
            {
                batch.CustomAabb = bounds;
                batch.VisibilityRangeEndMargin = margin;
            }
            tile.Batch.AddChild(batch);
            tile.MeshBatches.Add(multi);
            if (farMulti != null)
            {
                var farBatch = new MultiMeshInstance3D { Multimesh = farMulti, MaterialOverride = _material,
                    CustomAabb = bounds, CastShadow = GeometryInstance3D.ShadowCastingSetting.Off,
                    VisibilityRangeBegin = switchDistance, VisibilityRangeEnd = _range + _size * 1.5f,
                    VisibilityRangeBeginMargin = margin };
                tile.Batch.AddChild(farBatch);
                tile.MeshBatches.Add(farMulti);
            }
        }
        tile.Transforms.Clear();
        if (_player != null) UpdateDensity(tile, _player.GlobalPosition);
    }
    // =========================================================
    // Reduce submitted instances, not only shader visibility. Random placement order keeps
    // each retained prefix spatially mixed. Update at 4 Hz, with no per-tuft CPU work.
    private void UpdateDensity(Tile tile, Vector3 player)
    {
        if (tile.MeshBatches.Count == 0) return;
        float x = tile.Key.X * _size, z = tile.Key.Y * _size;
        float dx = player.X - Mathf.Clamp(player.X, x, x + _size);
        float dz = player.Z - Mathf.Clamp(player.Z, z, z + _size);
        float distance = Mathf.Sqrt(dx * dx + dz * dz);
        float blend = Mathf.Clamp((distance - _dense) / (_sparse - _dense), 0f, 1f);
        blend = blend * blend * (3f - 2f * blend);
        float fraction = Mathf.Lerp(1f, Mathf.Clamp(Settings.DistantDensityFraction, 0.02f, 1f), blend);
        if (distance >= _range) fraction = 0f;
        foreach (MultiMesh multi in tile.MeshBatches)
        {
            int visible = Mathf.Clamp(Mathf.CeilToInt(multi.InstanceCount * fraction), 0, multi.InstanceCount);
            if (multi.VisibleInstanceCount != visible) multi.VisibleInstanceCount = visible;
        }
    }
}
