// Small request validator. Native edit/target logic belongs to the optional terrain backend.
using Godot;

public partial class ExcavationService : Node
{
    [Export] public ExcavationSettings Settings { get; set; } = new();
    private ITerrainExcavation? _terrain;
    public float DigAmount { get; private set; } = 1f;
    public event System.Action<Vector3, float>? Excavated;

    // =========================================================
    // Composition supplies the optional backend once; this service has no frame callbacks.
    public void Initialize(ITerrainExcavation? terrain)
    {
        if (_terrain != null) _terrain.Excavated -= OnExcavated;
        _terrain = terrain;
        if (_terrain != null) _terrain.Excavated += OnExcavated;
    }

    // =========================================================
    // Debug changes apply to future clicks only; no idle work or mutation of saved resources.
    public void SetDebugDigAmount(float amount) => DigAmount = float.IsFinite(amount) ? Mathf.Clamp(amount, 0.25f, 6f) : 1f;

    // =========================================================
    // Notify composition only after the native edit has been accepted.
    private void OnExcavated(Vector3 center, float radius) => Excavated?.Invoke(center, radius);

    // =========================================================
    // Unsubscribe from the backend when this scene unloads.
    public override void _ExitTree() => Initialize(null);

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
            || inset < -radius || inset > radius * 0.5f)
        { feedback = "Dig request exceeds the excavation limits."; return false; }
        // Keep a voxel-resolvable brush radius; control the shallow cap penetrating the surface.
        float depth = Mathf.Clamp((radius + inset) * DigAmount, 0.02f, radius * 1.5f);
        return _terrain.TryExcavate(origin, direction.Normalized(), reach, radius, depth - radius, out feedback);
    }
}
