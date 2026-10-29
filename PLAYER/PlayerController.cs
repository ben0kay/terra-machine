// First-person capsule movement. World composition can suspend it while loading.
using Godot;

public partial class PlayerController : CharacterBody3D
{
    [Export] public PlayerSettings Settings { get; set; } = new();
    private PlayerInput _input = null!;
    private Camera3D _camera = null!;
    private bool _movementEnabled = true;
    private float _pitch;
    public bool MovementEnabled => _movementEnabled;
    public bool InputBlocked { get => _input.Blocked; set { _input.Blocked = value; _input.ConsumeJump(); } }

    #region SetupAndControl
    // =========================================================
    // Cache stable child references once.
    public override void _Ready()
    {
        _input = GetNode<PlayerInput>("PlayerInput");
        _camera = GetNode<Camera3D>("Head/Camera3D");
        _camera.Fov = Settings.FieldOfView;
    }

    // =========================================================
    // Suspend gravity/movement while terrain is loading, retaining mouse look.
    public void SetMovementEnabled(bool enabled)
    {
        _movementEnabled = enabled;
        Velocity = Vector3.Zero;
        _input.ConsumeJump();
        SetPhysicsProcess(enabled);
    }

    // =========================================================
    // Apply event-driven mouse look without an idle render loop.
    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event is not InputEventMouseMotion mouse || !_input.Captured) return;
        RotateY(-mouse.Relative.X * Settings.MouseSensitivity);
        float direction = Settings.InvertMouseY ? 1f : -1f;
        _pitch = Mathf.Clamp(_pitch + mouse.Relative.Y * Settings.MouseSensitivity * direction,
            Mathf.DegToRad(-85f), Mathf.DegToRad(85f));
        _camera.Rotation = new Vector3(_pitch, 0f, 0f);
    }
    #endregion

    #region Movement
    // =========================================================
    // One native capsule sweep per physics tick while enabled.
    public override void _PhysicsProcess(double delta)
    {
        if (!_movementEnabled) return;
        float dt = (float)delta;
        Vector2 input = _input.Movement;
        Vector3 wish = GlobalBasis * new Vector3(input.X, 0f, input.Y);
        float speed = Settings.WalkSpeed * (_input.Sprinting ? Settings.SprintMultiplier : 1f);
        Vector3 velocity = Velocity;
        velocity.X = Mathf.MoveToward(velocity.X, wish.X * speed, Settings.Acceleration * dt);
        velocity.Z = Mathf.MoveToward(velocity.Z, wish.Z * speed, Settings.Acceleration * dt);
        velocity.Y = IsOnFloor() ? -0.1f : velocity.Y - Settings.Gravity * dt;
        if (_input.ConsumeJump() && IsOnFloor()) velocity.Y = Settings.JumpVelocity;
        Velocity = velocity;
        MoveAndSlide();
    }
    #endregion
}
