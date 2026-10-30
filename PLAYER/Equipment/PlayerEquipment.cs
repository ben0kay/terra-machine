// Minimal DG1 selection/dispatch: one temporary shovel, no inventory/hotbar framework.
using Godot;

public partial class PlayerEquipment : Node
{
    private PlayerController _player = null!;
    private PlayerInput _input = null!;
    private Camera3D _camera = null!;
    private Shovel _shovel = null!;
    private Label _status = null!;

    // =========================================================
    // Cache stable children once and update the HUD only when equip/feedback changes.
    public override void _Ready()
    {
        _player = GetParent<PlayerController>();
        _input = _player.GetNode<PlayerInput>("PlayerInput");
        _camera = _player.GetNode<Camera3D>("Head/Camera3D");
        _shovel = GetNode<Shovel>("Shovel");
        _status = GetNode<Label>("HUD/Status");
        _shovel.Feedback += ShowFeedback;
        _input.CaptureReleased += CancelPending;
        _shovel.Initialize(null, _player, _input);
        _shovel.SetEquipped(false);
    }

    // =========================================================
    // Test-world composition opts into excavation; the player remains reusable elsewhere.
    public void BindExcavation(ExcavationService? service) => _shovel.Initialize(service, _player, _input);

    // =========================================================
    // Respect capture/loading/UI state; handled UI clicks never reach this callback.
    public override void _UnhandledInput(InputEvent @event)
    {
        if (!_input.Captured || !_player.MovementEnabled) return;
        if (@event is InputEventKey key && key.Echo) return;
        if (@event.IsActionPressed("equip_shovel"))
        {
            _shovel.SetEquipped(!_shovel.Equipped);
            GetViewport().SetInputAsHandled();
        }
        else if (@event.IsActionPressed("tool_primary") && _shovel.Equipped)
        {
            _shovel.Activate(_camera.GlobalPosition, -_camera.GlobalBasis.Z);
            GetViewport().SetInputAsHandled();
        }
    }

    // =========================================================
    // Respawn cancels a pending click so an old aim position cannot excavate after teleport.
    public void CancelPending() => _shovel.CancelPending();

    // =========================================================
    // Keep feedback small and event-driven rather than rewriting it every frame.
    private void ShowFeedback(string text) => _status.Text = _shovel.Equipped ? $"[Shovel] {text}" : text;

    // =========================================================
    // Release the managed event subscription when the prefab unloads.
    public override void _ExitTree()
    {
        if (GodotObject.IsInstanceValid(_shovel)) _shovel.Feedback -= ShowFeedback;
        if (GodotObject.IsInstanceValid(_input)) _input.CaptureReleased -= CancelPending;
    }
}
