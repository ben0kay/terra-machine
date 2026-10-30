// Event-driven tree diagnostics; caches authored LOD levels and render settings once per tree.
using Godot;
using System.Collections.Generic;

public sealed class TreeRenderDiagnostics
{
    private sealed class Entry
    {
        public MeshInstance3D Mesh = null!;
        public GeometryInstance3D.ShadowCastingSetting Shadow;
        public bool Authored, Visible;
        public int Level, MaxLevel;
        public float BeginMargin, EndMargin, Cull;
    }
    private readonly List<Entry> _meshes = new();
    private readonly Dictionary<StandardMaterial3D, StandardMaterial3D> _materials = new();
    private bool _shadows = true, _unshaded;
    private float _split = 70f, _farSplit = 280f;
    private int _lod;

    // =========================================================
    // Cache geometry handles after the authored LOD scene is ready, then apply UI state.
    public void Register(Node root)
    {
        Collect(root, false, 0, 0, 280f);
        Apply();
    }

    // =========================================================
    // Identify LOD1/2/3 in the authored hierarchy; ordinary firs remain non-authored.
    private void Collect(Node node, bool authored, int level, int maxLevel, float cull)
    {
        if (node is AuthoredTreeLod pair)
        {
            authored = true;
            maxLevel = pair.DistantModel != null &&
                pair.GetNodeOrNull<Node3D>("LOD3")?.Visible == true ? 3 : 2;
            cull = Mathf.Max(pair.SwitchDistance + 1f, pair.CullDistance);
        }
        if (authored)
        {
            if (node.Name == "LOD1") level = 1;
            else if (node.Name == "LOD2") level = 2;
            else if (node.Name == "LOD3") level = 3;
        }
        if (node is MeshInstance3D mesh && mesh.Mesh != null)
        {
            _meshes.Add(new Entry { Mesh = mesh, Shadow = mesh.CastShadow,
                Authored = authored, Level = level, MaxLevel = maxLevel, Visible = mesh.Visible,
                Cull = cull, BeginMargin = mesh.VisibilityRangeBeginMargin,
                EndMargin = mesh.VisibilityRangeEndMargin });
            for (int i = 0; i < mesh.Mesh.GetSurfaceCount(); i++)
            {
                if (mesh.GetActiveMaterial(i) is not StandardMaterial3D source) continue;
                if (!_materials.TryGetValue(source, out StandardMaterial3D? copy))
                {
                    copy = (StandardMaterial3D)source.Duplicate();
                    _materials.Add(source, copy);
                }
                mesh.SetSurfaceOverrideMaterial(i, copy);
            }
        }
        foreach (Node child in node.GetChildren()) Collect(child, authored, level, maxLevel, cull);
    }

    // =========================================================
    // Session options apply to existing and newly placed trees without frame polling.
    public void Set(bool shadows, bool unshaded, float split, int lod, float farSplit = 280f)
    {
        _shadows = shadows; _unshaded = unshaded;
        _split = Mathf.Clamp(split, 5f, 140f);
        _farSplit = Mathf.Clamp(farSplit, 160f, 550f);
        _lod = Mathf.Clamp(lod, 0, 3);
        Apply();
    }

    // =========================================================
    // Force LOD3 falls back to LOD2 for two-level pines; Auto uses native ranges.
    private void Apply()
    {
        foreach (Entry e in _meshes)
        {
            MeshInstance3D mesh = e.Mesh;
            if (!GodotObject.IsInstanceValid(mesh)) continue;
            mesh.CastShadow = _shadows ? e.Shadow : GeometryInstance3D.ShadowCastingSetting.Off;
            if (!e.Authored) continue;
            if (_lod != 0)
            {
                mesh.Visible = e.Visible && e.Level == Mathf.Min(_lod, e.MaxLevel);
                mesh.VisibilityRangeBegin = 0f; mesh.VisibilityRangeEnd = e.Cull;
                mesh.VisibilityRangeBeginMargin = 0f; mesh.VisibilityRangeEndMargin = 0f;
                continue;
            }
            float first = Mathf.Min(_split, e.Cull - 1f);
            float second = e.MaxLevel == 3 ? Mathf.Clamp(_farSplit, first + 1f, e.Cull - 1f) : e.Cull;
            mesh.Visible = e.Visible;
            mesh.VisibilityRangeBegin = e.Level == 1 ? 0f : e.Level == 2 ? first : second;
            mesh.VisibilityRangeEnd = e.Level == 1 ? first : e.Level == 2 ? second : e.Cull;
            mesh.VisibilityRangeBeginMargin = e.BeginMargin;
            mesh.VisibilityRangeEndMargin = e.EndMargin;
        }
        foreach (var entry in _materials)
            entry.Value.ShadingMode = _unshaded ? BaseMaterial3D.ShadingModeEnum.Unshaded : entry.Key.ShadingMode;
    }
}
