// Discrete shovel activation: one queued physics-frame request, then sleep again.
using Godot;
using System;

public partial class Shovel : Node
{
    [Export] public ShovelSettings Settings { get; set; } = new();
    private ExcavationService? _service;
    private PlayerController? _player;
    private PlayerInput? _input;
    private Vector3 _origin, _direction;
    private ulong _nextAllowed;
    private bool _queued;
    public bool Equipped { get; private set; }
    public event Action<string>? Feedback;

    // =========================================================
    // The tool remains dormant until one input edge schedules a request.
    public override void _Ready() => SetPhysicsProcess(false);

    // =========================================================
    // Bind ordinary gameplay objects; the shovel knows no native terrain API.
    public void Initialize(ExcavationService? service, PlayerController player, PlayerInput input)
    { _service = service; _player = player; _input = input; }

    // =========================================================
    // Switching tools cancels any unexecuted click without resetting the cooldown.
    public void SetEquipped(bool equipped)
    {
        Equipped = equipped; CancelPending();
        Feedback?.Invoke(equipped ? "Shovel equipped — LMB: dig nearby ground | 1: unequip" : "1: equip test shovel");
    }

    // =========================================================
    // Input-edge only. Holding the button does not repeatedly excavate.
    public void Activate(Vector3 origin, Vector3 direction)
    {
        if (!CanUse() || _queued) return;
        if (Time.GetTicksMsec() < _nextAllowed) return;
        _origin = origin; _direction = direction; _queued = true;
        SetPhysicsProcess(true);
    }

    // =========================================================
    // Physics-space reads occur in the physics callback, never in an input callback.
    public override void _PhysicsProcess(double delta)
    {
        bool queued = _queued;
        CancelPending();
        if (!queued || !CanUse()) return;
        _nextAllowed = Time.GetTicksMsec() + (ulong)(Mathf.Clamp(Settings.CooldownSeconds, 0.2f, 3f) * 1000f);
        if (_service == null) { Feedback?.Invoke("Excavation unavailable in this scene."); return; }
        _service.TryDig(_origin, _direction, Settings.Reach, Settings.Radius, Settings.SurfaceInset, out string feedback);
        Feedback?.Invoke(feedback);
    }

    // =========================================================
    // Loading, UI blocking, Escape and focus loss invalidate a queued action.
    private bool CanUse() => Equipped && _player?.MovementEnabled == true && _input?.Captured == true;

    // =========================================================
    // No retry timer or polling while the shovel is idle or unequipped.
    public void CancelPending() { _queued = false; SetPhysicsProcess(false); }
}
