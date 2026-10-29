// Small rolling frame-time graph. Sampling is intentionally continuous only while visible/focused.
// Uses elapsed wall time rather than scaled/clamped game delta; this is not GPU timing.
using Godot;

public partial class FrameTimeGraph : Control
{
    [Export] public DebugSettings Settings { get; set; } = new();
    private readonly float[] _milliseconds = new float[180];
    private int _cursor, _count;
    private ulong _previous;
    private bool _ready, _focused = true;
    private Timer _timer = null!;
    private Label _label = null!;

    // =========================================================
    // Fixed-size buffer and throttled redraws keep the diagnostic's own work small.
    public override void _Ready()
    {
        MouseFilter = MouseFilterEnum.Ignore;
        _label = new Label { Position = new Vector2(8, 5), MouseFilter = MouseFilterEnum.Ignore };
        _label.AddThemeFontSizeOverride("font_size", 13);
        AddChild(_label);
        _timer = new Timer { WaitTime = 0.2 };
        AddChild(_timer); _timer.Timeout += RefreshGraph;
        VisibilityChanged += UpdateActivity;
        _ready = true;
        Visible = Settings.Enabled && Settings.ShowLagometer;
        SetProcessInput(Settings.Enabled);
        UpdateActivity();
    }

    // =========================================================
    // F2 remains available while hidden; a hidden graph has no sample/update timer callbacks.
    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventKey key && key.Pressed && !key.Echo && key.Keycode == Key.F2)
        {
            Visible = !Visible;
            GetViewport().SetInputAsHandled();
        }
    }

    // =========================================================
    // Pausing on focus loss prevents alt-tab from filling the history with background timings.
    public override void _Notification(int what)
    {
        if (what == NotificationApplicationFocusOut) { _focused = false; UpdateActivity(); }
        if (what == NotificationApplicationFocusIn) { _focused = true; UpdateActivity(); }
    }

    // =========================================================
    // Reset the timestamp on resume so a hidden interval is never counted as one giant frame.
    private void UpdateActivity()
    {
        if (!_ready) return;
        bool active = Settings.Enabled && IsVisibleInTree() && _focused;
        _previous = 0;
        SetProcess(active);
        if (active) _timer.Start(); else _timer.Stop();
    }

    // =========================================================
    // One timestamp and one ring-buffer write per frame; no strings, scans or graph redraw here.
    public override void _Process(double delta)
    {
        ulong now = Time.GetTicksUsec();
        if (_previous != 0)
        {
            _milliseconds[_cursor] = (now - _previous) / 1000f;
            _cursor = (_cursor + 1) % _milliseconds.Length;
            _count = Mathf.Min(_count + 1, _milliseconds.Length);
        }
        _previous = now;
    }

    // =========================================================
    // Five UI updates per second show mean frame time and the worst retained spike.
    private void RefreshGraph()
    {
        float sum = 0f, peak = 0f;
        for (int i = 0; i < _count; i++) { sum += _milliseconds[i]; peak = Mathf.Max(peak, _milliseconds[i]); }
        float mean = _count > 0 ? sum / _count : 0f;
        _label.Text = mean > 0f ? $"{1000f / mean:0} FPS  |  {mean:0.0} ms  |  peak {peak:0.0} ms\nFrame time · 16.7 ms line · F2 hides" : "Frame time · collecting... · F2 hides";
        QueueRedraw();
    }

    // =========================================================
    // Higher bars mean slower frames; graph clips at 50 ms but the peak readout remains exact.
    public override void _Draw()
    {
        DrawRect(new Rect2(Vector2.Zero, Size), new Color(0.02f, 0.03f, 0.04f, 0.82f));
        float left = 8f, bottom = Size.Y - 8f, height = Size.Y - 49f, width = Size.X - 16f;
        float targetY = bottom - height * (16.6667f / 50f);
        DrawLine(new Vector2(left, targetY), new Vector2(left + width, targetY), new Color(0.8f, 0.8f, 0.8f, 0.45f));
        for (int i = 0; i < _count; i++)
        {
            float ms = _milliseconds[(_cursor - _count + i + _milliseconds.Length) % _milliseconds.Length];
            float x = left + (i + _milliseconds.Length - _count) * width / _milliseconds.Length;
            Color color = ms <= 20f ? new Color(0.3f, 0.85f, 0.4f) : ms <= 33.34f ? new Color(1f, 0.75f, 0.2f) : new Color(1f, 0.3f, 0.25f);
            DrawLine(new Vector2(x, bottom), new Vector2(x, bottom - height * Mathf.Clamp(ms / 50f, 0f, 1f)), color);
        }
    }
}
