// Grounds a bounded authored group on loaded terrain, then sleeps completely.
// Uses the game terrain interface; no Voxel Tools types or calls belong here.
using Godot;
using System.Collections.Generic;

public partial class TreePreview : Node3D
{
    [Export] public TreePreviewSettings Settings { get; set; } = new();
    private ITerrainService? _terrain;
    private readonly Dictionary<Material, Material> _materials = new();
    private readonly TreeRenderDiagnostics _diagnostics = new();
    private bool[] _placed = System.Array.Empty<bool>();
    private Vector2 _center;
    private float _top, _bottom, _elapsed, _untilProbe;
    private int _cursor, _remaining;
    public int PlacedCount { get; private set; }
    public bool IsPlacing { get; private set; }

    #region Placement
    // =========================================================
    // Merely adding the scene does no background work until composition activates it.
    public override void _Ready() => SetPhysicsProcess(false);

    // =========================================================
    // Start once after successful player spawn; F4 does not duplicate already placed trees.
    public void Initialize(ITerrainService terrain, Vector2 center, float top, float bottom)
    {
        if (_placed.Length > 0 || Settings.Offsets.Count == 0 || Settings.Variants.Count == 0) return;
        _terrain = terrain;
        _center = center; _top = top; _bottom = bottom;
        _placed = new bool[Settings.Offsets.Count];
        _remaining = _placed.Length;
        // Keep shared offsets and original pine indices stable when hiding authored sites.
        foreach (int index in Settings.DisabledIndices)
        {
            if (index < 0 || index >= _placed.Length || _placed[index]) continue;
            _placed[index] = true; _remaining--;
        }
        IsPlacing = _remaining > 0;
        SetPhysicsProcess(IsPlacing);
    }

    // =========================================================
    // Retry only during initial chunk loading, with an explicit query budget and timeout.
    public override void _PhysicsProcess(double delta)
    {
        _elapsed += (float)delta;
        if (_terrain == null || !_terrain.IsAvailable || _elapsed > Settings.LoadingTimeout)
        {
            if (_remaining > 0) GD.PushWarning($"Tree preview placed {PlacedCount}/{_placed.Length}; remaining terrain collision was unavailable.");
            StopPlacement();
            return;
        }
        _untilProbe -= (float)delta;
        if (_untilProbe > 0f) return;
        _untilProbe = Mathf.Max(0.05f, Settings.RetryInterval);
        int budget = Mathf.Clamp(Settings.QueriesPerTick, 1, 16);
        int visited = 0;
        while (budget > 0 && visited < _placed.Length && _remaining > 0)
        {
            int index = _cursor;
            _cursor = (_cursor + 1) % _placed.Length;
            visited++;
            if (_placed[index]) continue;
            budget--;
            Vector2 position = _center + Settings.Offsets[index];
            if (!_terrain.TryFindGround(position, _top, _bottom, out Vector3 ground)) continue;
            AddTree(index, ground);
            _placed[index] = true;
            _remaining--; PlacedCount++;
        }
        if (_remaining == 0) StopPlacement();
    }

    // =========================================================
    // Stop all callbacks once settled; models are static scene instances without scripts.
    private void StopPlacement()
    {
        IsPlacing = false;
        SetPhysicsProcess(false);
    }
    #endregion

    #region Visuals
    // =========================================================
    // Cancel Blender's A/B/C display offsets without modifying the imported GLB files.
    private void AddTree(int index, Vector3 ground)
    {
        PackedScene scene = Settings.Variants[index % Settings.Variants.Count];
        var wrapper = new Node3D { Name = $"Tree_{index + 1:00}" };
        AddChild(wrapper);
        var model = scene.Instantiate<Node3D>();
        wrapper.AddChild(model);
        MeshInstance3D? firstMesh = FindFirstMesh(model);
        if (firstMesh != null) model.Position -= wrapper.ToLocal(firstMesh.GlobalPosition);
        ApplyMaterials(model);
        _diagnostics.Register(model);
        float amount = ((index * 7) % 13) / 12f;
        float scale = Mathf.Lerp(Settings.MinimumScale, Settings.MaximumScale, amount);
        wrapper.Scale = Vector3.One * scale;
        wrapper.Rotation = new Vector3(0f, Mathf.DegToRad(index * 137.5f), 0f);
        wrapper.GlobalPosition = ground - Vector3.Up * Settings.GroundSink;
    }

    // =========================================================
    // Debug visibility is render-only. Cached handles avoid repeated scene-tree searches.
    public void SetDebugRendering(bool visible, bool shadows, bool unshaded, float split = 70f, int lod = 0)
    {
        Visible = visible;
        _diagnostics.Set(shadows, unshaded, split, lod);
    }

    // =========================================================
    // Find the mesh pivot once per placed model; not part of a frame hot path.
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

    // =========================================================
    // Share one cloned material per source material; avoid alpha-blended needle sorting.
    private void ApplyMaterials(Node node)
    {
        if (node is MeshInstance3D mesh && mesh.Mesh != null)
        {
            for (int i = 0; i < mesh.Mesh.GetSurfaceCount(); i++)
            {
                Material original = mesh.GetActiveMaterial(i);
                if (original is not StandardMaterial3D standard) continue;
                if (!_materials.TryGetValue(original, out Material? configured))
                {
                    var material = (StandardMaterial3D)standard.Duplicate();
                    material.TextureFilter = BaseMaterial3D.TextureFilterEnum.LinearWithMipmapsAnisotropic;
                    if (material.Transparency == BaseMaterial3D.TransparencyEnum.Alpha)
                    {
                        material.Transparency = BaseMaterial3D.TransparencyEnum.AlphaScissor;
                        material.AlphaScissorThreshold = Settings.AlphaCutoff;
                    }
                    configured = material;
                    _materials.Add(original, configured);
                }
                mesh.SetSurfaceOverrideMaterial(i, configured);
            }
        }
        foreach (Node child in node.GetChildren()) ApplyMaterials(child);
    }
    #endregion
}
