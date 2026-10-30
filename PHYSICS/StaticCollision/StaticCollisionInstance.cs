// Opt-in prefab component for a shared authored static shape; no tick or distance polling.
using Godot;

public partial class StaticCollisionInstance : Node3D
{
    [Export] public StaticCollisionProfile? Profile { get; set; }

    // =========================================================
    // Attach once in the asset's local frame; parent transforms/lifetime carry the collider.
    public override void _Ready()
    {
        SetProcess(false); SetPhysicsProcess(false);
        if (Profile == null || Profile.Mode == StaticCollisionMode.None) return;
        if (Profile.Mode != StaticCollisionMode.AuthoredShape)
        {
            GD.PushWarning("Prefab collision requires an authored shared Shape3D. Prepare mesh hulls once in the placement owner.");
            return;
        }
        StaticCollisionBuilder.Attach(this, Profile, StaticCollisionBuilder.Prepare(Profile), Transform3D.Identity);
    }
}
