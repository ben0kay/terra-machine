// One-time native static-body construction shared by rocks, trunks, props and other fixed assets.
// Call Prepare once per source/profile, then Attach for each instance; no manager/timer/frame work.
using Godot;

public static class StaticCollisionBuilder
{
    // =========================================================
    // Return an authored primitive/hull, or cook one shared convex hull from a dedicated mesh.
    public static Shape3D? Prepare(StaticCollisionProfile? profile, Mesh? sourceMesh = null)
    {
        if (profile == null || profile.Mode == StaticCollisionMode.None) return null;
        if (profile.Mode == StaticCollisionMode.AuthoredShape)
        {
            if (profile.Shape == null) GD.PushWarning("Static collision profile has no authored Shape3D.");
            return profile.Shape;
        }
        if (profile.Mode == StaticCollisionMode.ConvexMesh)
        {
            if (sourceMesh != null) return sourceMesh.CreateConvexShape(true, profile.SimplifyConvexHull);
            GD.PushWarning("Static convex collision requires a source mesh.");
        }
        return null;
    }

    // =========================================================
    // Keep one body/shape per fixed object; preserve the source mesh's local alignment.
    // Parent transforms carry uniform scale/yaw. Lifetime and optional streaming belong to caller.
    public static StaticBody3D? Attach(Node3D parent, StaticCollisionProfile? profile,
        Shape3D? preparedShape, Transform3D sourceTransform)
    {
        if (profile == null || profile.Mode == StaticCollisionMode.None || preparedShape == null) return null;
        var body = new StaticBody3D { Name = "Collision", Transform = sourceTransform,
            CollisionLayer = profile.CollisionLayer, CollisionMask = profile.CollisionMask };
        body.AddChild(new CollisionShape3D { Shape = preparedShape, Transform = profile.ShapeTransform });
        parent.AddChild(body);
        return body;
    }
}
