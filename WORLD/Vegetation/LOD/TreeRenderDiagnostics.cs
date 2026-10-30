// Event-driven tree render diagnostics. Register once at placement; no frame callbacks.
using Godot;
using System.Collections.Generic;

public sealed class TreeRenderDiagnostics
{
    private sealed class Entry
    {
        public MeshInstance3D Mesh = null!;
        public GeometryInstance3D.ShadowCastingSetting Shadow;
        public bool Authored, Far, Visible;
        public float Begin, End, BeginMargin, EndMargin, Cull;
    }
    private readonly List<Entry> _meshes = new();
    private readonly Dictionary<StandardMaterial3D, StandardMaterial3D> _materials = new();
    private bool _shadows = true, _unshaded;
    private float _split = 70f;
    private int _lod;

    // =========================================================
    // Called after owner-specific material/shadow setup and imported model readiness.
    public void Register(Node root)
    {
        Collect(root, false, false, 280f);
        Apply();
    }

    // =========================================================
    // Cache original settings so Auto and shadows-on restore authored policy, including firs.
    private void Collect(Node node, bool authored, bool far, float cull)
    {
        if (node is AuthoredTreeLod pair) { authored = true; cull = Mathf.Max(pair.SwitchDistance + 1f, pair.CullDistance); }
        far |= authored && node.Name == "LOD2";
        if (node is MeshInstance3D mesh && mesh.Mesh != null)
        {
            _meshes.Add(new Entry { Mesh = mesh, Shadow = mesh.CastShadow,
                Authored = authored, Far = far, Visible = mesh.Visible,
                Cull = cull, Begin = mesh.VisibilityRangeBegin, End = mesh.VisibilityRangeEnd,
                BeginMargin = mesh.VisibilityRangeBeginMargin, EndMargin = mesh.VisibilityRangeEndMargin });
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
        foreach (Node child in node.GetChildren()) Collect(child, authored, far, cull);
    }

    // =========================================================
    // Session settings also apply to trees registered later while travelling.
    public void Set(bool shadows, bool unshaded, float split, int lod)
    {
        _shadows = shadows; _unshaded = unshaded;
        _split = Mathf.Clamp(split, 5f, 140f); _lod = Mathf.Clamp(lod, 0, 2);
        Apply();
    }

    // =========================================================
    // Native renderer still owns range checks. Only explicit UI inputs write these values.
    private void Apply()
    {
        foreach (Entry e in _meshes)
        {
            MeshInstance3D mesh = e.Mesh;
            if (!GodotObject.IsInstanceValid(mesh)) continue;
            mesh.CastShadow = _shadows ? e.Shadow : GeometryInstance3D.ShadowCastingSetting.Off;
            if (!e.Authored) continue; // Fir saplings have no authored LOD pair.
            float split = Mathf.Min(_split, e.Cull - 1f);
            mesh.Visible = e.Visible && (_lod == 0 || (_lod == 2) == e.Far);
            mesh.VisibilityRangeBegin = _lod == 0 && e.Far ? split : 0f;
            mesh.VisibilityRangeEnd = _lod == 0 && !e.Far ? split : e.Cull;
            mesh.VisibilityRangeBeginMargin = _lod == 0 ? e.BeginMargin : 0f;
            mesh.VisibilityRangeEndMargin = _lod == 0 ? e.EndMargin : 0f;
        }
        foreach (var entry in _materials)
            entry.Value.ShadingMode = _unshaded ? BaseMaterial3D.ShadingModeEnum.Unshaded : entry.Key.ShadingMode;
    }
}
