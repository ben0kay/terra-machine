// Owns movement actions and mouse capture; reusable without a world scene.
using Godot;

public partial class PlayerInput : Node
{
    private bool _jumpQueued;
    public event System.Action? CaptureReleased;
    public bool Blocked { get; set; }
    public bool Captured => !Blocked && Input.MouseMode == Input.MouseModeEnum.Captured;
    public Vector2 Movement => Captured ? Input.GetVector("move_left", "move_right", "move_forward", "move_back") : Vector2.Zero;
    public bool Sprinting => Captured && Input.IsActionPressed("sprint");

    // =========================================================
    // Capture on entry; registered project InputMap actions remain editable.
    public override void _Ready() => Input.MouseMode = Input.MouseModeEnum.Captured;

    // =========================================================
    // Release the mouse on focus loss rather than allowing background movement.
    public override void _Notification(int what)
    {
        if (what == NotificationApplicationFocusOut) ReleaseMouse();
    }

    // =========================================================
    // Queue a single jump per press and support Escape/click mouse capture.
    public override void _UnhandledInput(InputEvent @event)
    {
        if (Blocked) return;
        if (@event is InputEventKey key && key.Pressed && !key.Echo && key.Keycode == Key.Escape)
        {
            ReleaseMouse();
            GetViewport().SetInputAsHandled();
        }
        else if (@event is InputEventMouseButton mouse && mouse.Pressed)
        {
            if (!Captured && mouse.ButtonIndex == MouseButton.Left)
            {
                Input.MouseMode = Input.MouseModeEnum.Captured;
                GetViewport().SetInputAsHandled();
            }
            else if (Captured && mouse.IsActionPressed("jump")) _jumpQueued = true;
        }
    }

    // =========================================================
    // Consume edge-triggered input once, never auto-jump on landing.
    public bool ConsumeJump()
    {
        bool jump = _jumpQueued && Captured;
        _jumpQueued = false;
        return jump;
    }

    // =========================================================
    // Clear pending actions whenever control is released.
    private void ReleaseMouse()
    {
        _jumpQueued = false;
        Input.MouseMode = Input.MouseModeEnum.Visible;
        CaptureReleased?.Invoke();
    }

    // =========================================================
    // Restore normal cursor when removing this player.
    public override void _ExitTree() => Input.MouseMode = Input.MouseModeEnum.Visible;
}
