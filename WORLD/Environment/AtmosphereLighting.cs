// Applies night-only ambient fill after Sky3D updates celestial lighting.
// Event-driven: no frame/physics callback or polling, and vendor scripts remain untouched.
using Godot;

public partial class AtmosphereLighting : Node
{
    [ExportGroup("Night fill")]
    [Export] public float NightAmbientEnergy { get; set; } = 2f;
    [Export] public Color NightAmbientColor { get; set; } = new(0.48f, 0.6f, 0.8f);
    private Godot.Environment? _environment;
    private Node? _clock;
    private DirectionalLight3D? _sun;
    private Callable _timeChanged;
    private Color _skyAmbientColor;
    private float _dayEnergy;
    private bool _pending, _bound;

    // =========================================================
    // Child sky setup is complete before this sibling initializes; capture the daytime baseline.
    public override void _Ready()
    {
        SetProcess(false); SetPhysicsProcess(false);
        _environment = GetNodeOrNull<WorldEnvironment>("../Sky3D")?.Environment;
        _clock = GetNodeOrNull<Node>("../Sky3D/TimeOfDay");
        _sun = GetNodeOrNull<DirectionalLight3D>("../Sky3D/SunLight");
        if (_environment == null || _clock == null || _sun == null) return;
        _dayEnergy = _environment.AmbientLightEnergy;
        _skyAmbientColor = _environment.AmbientLightColor;
        _timeChanged = Callable.From<double>(OnTimeChanged);
        _clock.Connect("time_changed", _timeChanged);
        _bound = true;
        OnTimeChanged(0);
    }

    // =========================================================
    // Sky3D emits before finishing its colour update; defer and coalesce multiple input changes.
    private void OnTimeChanged(double hour)
    {
        if (_pending) return;
        _pending = true;
        CallDeferred(nameof(RefreshFromSky));
    }

    // =========================================================
    // Cache the addon's unmodified colour once per time change, then apply the night contribution.
    public void RefreshFromSky()
    {
        _pending = false;
        if (_environment == null || !IsInsideTree()) return;
        _skyAmbientColor = _environment.AmbientLightColor;
        ApplyNightFill();
    }

    // =========================================================
    // The debug slider changes only this run's settings, without recapturing our own output colour.
    public void SetNightAmbientEnergy(float energy)
    {
        NightAmbientEnergy = Mathf.Clamp(energy, 0f, 4f);
        ApplyNightFill();
    }

    // =========================================================
    // Sun +Z points toward the source. Blend from +6 to -9 degrees around the horizon.
    private void ApplyNightFill()
    {
        if (_environment == null || _sun == null) return;
        float weight = Mathf.Clamp((0.1f - _sun.GlobalBasis.Z.Y) / 0.25f, 0f, 1f);
        weight = weight * weight * (3f - 2f * weight);
        _environment.AmbientLightColor = _skyAmbientColor.Lerp(NightAmbientColor, weight);
        _environment.AmbientLightEnergy = Mathf.Lerp(_dayEnergy, Mathf.Max(0f, NightAmbientEnergy), weight);
    }

    // =========================================================
    // Detach signal ownership and restore the environment if this optional component is removed.
    public override void _ExitTree()
    {
        if (!_bound) return;
        if (_clock != null && GodotObject.IsInstanceValid(_clock) && _clock.IsConnected("time_changed", _timeChanged))
            _clock.Disconnect("time_changed", _timeChanged);
        if (_environment != null && GodotObject.IsInstanceValid(_environment))
        {
            _environment.AmbientLightColor = _skyAmbientColor;
            _environment.AmbientLightEnergy = _dayEnergy;
        }
    }
}
