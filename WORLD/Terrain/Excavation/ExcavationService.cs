// Small request validator. Native edit/target logic belongs to the optional terrain backend.
using Godot;

public partial class ExcavationService : Node
{
    [Export] public ExcavationSettings Settings { get; set; } = new();
    private ITerrainExcavation? _terrain;

    // =========================================================
    // Composition supplies the optional backend once; this service has no frame callbacks.
    public void Initialize(ITerrainExcavation? terrain) => _terrain = terrain;

    // =========================================================
    // Reject invalid/oversized requests rather than silently expanding a tool's edit.
    public bool TryDig(Vector3 origin, Vector3 direction, float reach, float radius, float inset, out string feedback)
    {
        feedback = "Excavation unavailable in this scene.";
        if (_terrain == null) return false;
        if (!origin.IsFinite() || !direction.IsFinite() || direction.LengthSquared() < 0.001f
            || !float.IsFinite(reach) || !float.IsFinite(radius) || !float.IsFinite(inset)
            || reach <= 0f || reach > Mathf.Clamp(Settings.MaximumReach, 0.5f, 5f)
            || radius < 0.5f || radius > Mathf.Clamp(Settings.MaximumRadius, 0.5f, 1.5f)
            || inset < 0f || inset > radius * 0.5f)
        { feedback = "Dig request exceeds the excavation limits."; return false; }
        return _terrain.TryExcavate(origin, direction.Normalized(), reach, radius, inset, out feedback);
    }
}
