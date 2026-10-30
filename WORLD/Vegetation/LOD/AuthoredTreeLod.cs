// Configures two or three authored tree models once; native visibility ranges own distance LOD.
// No per-frame distance checks, timers or material duplication.
using Godot;
using System.Collections.Generic;

public partial class AuthoredTreeLod : Node3D
{
    [ExportGroup("Models")]
    [Export] public PackedScene? NearModel { get; set; }
    [Export] public PackedScene? FarModel { get; set; }
    [Export] public PackedScene? DistantModel { get; set; }
    [ExportGroup("Distances (metres)")]
    [Export] public float SwitchDistance { get; set; } = 70f;
    [Export] public float DistantSwitchDistance { get; set; } = 280f;
    [Export] public float CullDistance { get; set; } = 280f;
    [Export] public float HysteresisMargin { get; set; } = 3f;

    // =========================================================
    // Instantiate shared imported resources and configure geometry once, then remain dormant.
    public override void _Ready()
    {
        SetProcess(false); SetPhysicsProcess(false);
        if (NearModel == null || FarModel == null)
        {
            GD.PushWarning("Authored tree LOD requires both near and far models.");
            return;
        }
        Node3D near = NearModel.Instantiate<Node3D>(), far = FarModel.Instantiate<Node3D>();
        near.Name = "LOD1"; far.Name = "LOD2";
        AddChild(near); AddChild(far);
        var nearMeshes = new List<MeshInstance3D>();
        var farMeshes = new List<MeshInstance3D>();
        CollectMeshes(near, nearMeshes); CollectMeshes(far, farMeshes);
        if (nearMeshes.Count == 0 || farMeshes.Count == 0)
        {
            // Leave the detailed source visible if a later export accidentally loses its meshes.
            far.Hide();
            GD.PushWarning("Authored tree LOD has an empty model; keeping the near model.");
            return;
        }
        // Optional LOD3 is a third imported scene; missing/broken assets fall back to LOD2.
        Node3D? distant = null;
        var distantMeshes = new List<MeshInstance3D>();
        if (DistantModel != null)
        {
            distant = DistantModel.Instantiate<Node3D>();
            distant.Name = "LOD3";
            AddChild(distant);
            CollectMeshes(distant, distantMeshes);
            if (distantMeshes.Count == 0)
            {
                distant.Hide();
                GD.PushWarning("Authored tree LOD3 is empty; keeping LOD2 until final cull.");
            }
        }

        Aabb bounds = BoundsInRoot(nearMeshes[0]);
        foreach (MeshInstance3D mesh in nearMeshes) bounds = bounds.Merge(BoundsInRoot(mesh));
        foreach (MeshInstance3D mesh in farMeshes) bounds = bounds.Merge(BoundsInRoot(mesh));
        foreach (MeshInstance3D mesh in distantMeshes) bounds = bounds.Merge(BoundsInRoot(mesh));
        float split = Mathf.Max(1f, SwitchDistance);
        bool hasThird = distantMeshes.Count > 0;
        float farSplit = hasThird ? Mathf.Max(split + 1f, DistantSwitchDistance) : 0f;
        float end = Mathf.Max((hasThird ? farSplit : split) + 1f, CullDistance);
        float gap = hasThird ? Mathf.Min(split, Mathf.Min(farSplit - split, end - farSplit))
            : Mathf.Min(split, end - split);
        float margin = Mathf.Clamp(HysteresisMargin, 0f, gap * 0.25f);
        Configure(nearMeshes, bounds, 0f, split, 0f, margin);
        Configure(farMeshes, bounds, split, hasThird ? farSplit : end, margin, margin);
        if (hasThird)
        {
            foreach (MeshInstance3D mesh in distantMeshes)
                mesh.CastShadow = GeometryInstance3D.ShadowCastingSetting.Off;
            Configure(distantMeshes, bounds, farSplit, end, margin, margin);
        }
    }

    // =========================================================
    // Imported hierarchy traversal happens only during creation, never on camera movement.
    private static void CollectMeshes(Node node, List<MeshInstance3D> meshes)
    {
        if (node is MeshInstance3D mesh && mesh.Mesh != null) meshes.Add(mesh);
        foreach (Node child in node.GetChildren()) CollectMeshes(child, meshes);
    }

    // =========================================================
    // All LODs use one root-space bounding box so their distance reference points agree.
    private Aabb BoundsInRoot(MeshInstance3D mesh)
    {
        Transform3D relative = GlobalTransform.AffineInverse() * mesh.GlobalTransform;
        return relative * mesh.GetAabb();
    }

    // =========================================================
    // Disabled fades retain opaque/cutout rendering; margins provide native hysteresis.
    private void Configure(List<MeshInstance3D> meshes, Aabb bounds, float begin, float end, float beginMargin, float endMargin)
    {
        foreach (MeshInstance3D mesh in meshes)
        {
            Transform3D relative = GlobalTransform.AffineInverse() * mesh.GlobalTransform;
            mesh.CustomAabb = relative.AffineInverse() * bounds;
            mesh.VisibilityRangeBegin = begin; mesh.VisibilityRangeEnd = end;
            mesh.VisibilityRangeBeginMargin = beginMargin; mesh.VisibilityRangeEndMargin = endMargin;
            mesh.VisibilityRangeFadeMode = GeometryInstance3D.VisibilityRangeFadeModeEnum.Disabled;
        }
    }
}
