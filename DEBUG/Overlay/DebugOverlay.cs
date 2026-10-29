// Optional event-driven render isolation panel and bounded same-camera comparison.
// Closed panel has no frame/physics/timer work; the optional frame graph owns its own sampling.
using Godot;
using System;
using System.Text;

public partial class DebugOverlay : CanvasLayer
{
    [Export] public DebugSettings Settings { get; set; } = new();
    // Optional additional visual roots, paths relative to this overlay (e.g. ../../Props).
    [Export] public Godot.Collections.Array<NodePath> ExtraVisualRoots { get; set; } = new();
    private ITerrainRenderDebug? _terrain;
    private TreePreview? _trees;
    private GrassScatter? _grass;
    private PlayerController _player = null!;
    private PanelContainer _panel = null!;
    private Label _metrics = null!, _results = null!, _progress = null!;
    private Timer _timer = null!;
    private CheckButton _ground = null!, _textures = null!, _normals = null!, _models = null!, _shadows = null!, _unshaded = null!;
    private int _previousFpsCap;
    private bool _initialized, _open, _restoreMovement, _restoreInputBlocked;
    private Input.MouseModeEnum _restoreMouse;
    private bool _groundOn = true, _texturesOn = true, _normalsOn = true, _treesOn = true, _shadowsOn = true, _unshadedOn;
    private readonly StringBuilder _report = new();
    private readonly StringBuilder _resultRows = new();
    private int _stage = -1, _frames;
    private bool _sampling;
    private double _warmup;
    private ulong _sampleStart;
    private bool[] _saved = Array.Empty<bool>();
    private static readonly string[] Cases = { "Baseline: simple ground, no trees", "Textured ground, no trees", "Trees + simple ground", "Full scene", "Full scene, tree shadows off" };

    #region SetupAndInput
    // =========================================================
    // The world initializes this after successful grounded spawn; unavailable before then.
    public override void _Ready()
    {
        _previousFpsCap = Engine.MaxFps;
        if (Settings.Enabled) Engine.MaxFps = Mathf.Clamp(Settings.FpsCap, 30, 240);
        _panel = GetNode<PanelContainer>("Panel");
        _progress = GetNode<Label>("Progress");
        _timer = GetNode<Timer>("ReadoutTimer");
        _timer.Timeout += UpdateReadout;
        SetProcess(false); SetPhysicsProcess(false);
        SetProcessInput(Settings.Enabled);
    }

    // =========================================================
    // Explicit ready references mirror War Machine's composition without copying its systems.
    public void Initialize(ITerrainRenderDebug? terrain, TreePreview? trees, PlayerController player)
    {
        if (_initialized || !Settings.Enabled) return;
        _terrain = terrain; _trees = trees; _player = player;
        _grass = GetNodeOrNull<GrassScatter>("../GrassScatter");
        _timer.WaitTime = Mathf.Max(0.1f, Settings.ReadoutInterval);
        BuildControls(GetNode<VBoxContainer>("Panel/Rows"));
        _initialized = true;
    }

    // =========================================================
    // F1 opens controls; Escape/F1 cancels a run and closes. UI clicks never reach jump/input.
    public override void _Input(InputEvent @event)
    {
        if (!_initialized || @event is not InputEventKey key || !key.Pressed || key.Echo) return;
        if (_open && key.Keycode == Key.F4) { GetViewport().SetInputAsHandled(); return; }
        if (key.Keycode == Key.F1 || (_open && key.Keycode == Key.Escape))
        {
            SetOpen(!_open);
            GetViewport().SetInputAsHandled();
        }
    }

    // =========================================================
    // Background throttling would invalidate a run, so cancel if the application loses focus.
    public override void _Notification(int what)
    {
        if (what == NotificationApplicationFocusOut && _stage >= 0)
        {
            CancelComparison();
            _panel.Show();
        }
    }

    // =========================================================
    // Freeze the player, not the world: camera/view remain identical for comparisons.
    private void SetOpen(bool open)
    {
        if (_open == open) return;
        if (open)
        {
            _restoreMovement = _player.MovementEnabled;
            _restoreInputBlocked = _player.InputBlocked;
            _player.InputBlocked = true;
            _restoreMouse = Input.MouseMode;
            _player.SetMovementEnabled(false);
            Input.MouseMode = Input.MouseModeEnum.Visible;
            _timer.Start();
        }
        else
        {
            CancelComparison();
            _timer.Stop();
            _player.SetMovementEnabled(_restoreMovement);
            _player.InputBlocked = _restoreInputBlocked;
            Input.MouseMode = _restoreMouse;
        }
        _open = open;
        _panel.Visible = open;
        if (open) UpdateReadout();
    }
    #endregion

    #region Controls
    // =========================================================
    // Build controls once. No hierarchy searches or UI reconstruction while sampling.
    private void BuildControls(VBoxContainer rows)
    {
        rows.AddChild(new Label { Text = "RENDER DEBUG — F1 / Esc closes" });
        rows.AddChild(new Label { Text = "Temporary changes · camera frozen while open" });
        _metrics = new Label(); rows.AddChild(_metrics);
        var scroll = new ScrollContainer { HorizontalScrollMode = ScrollContainer.ScrollMode.Disabled, CustomMinimumSize = new Vector2(400, Mathf.Clamp(GetViewport().GetVisibleRect().Size.Y - 350, 160, 380)) };
        rows.AddChild(scroll);
        var controls = new VBoxContainer { SizeFlagsHorizontal = Control.SizeFlags.ExpandFill };
        scroll.AddChild(controls);
        AddSection(controls, "ENVIRONMENT");
        _ground = AddToggle(controls, "Terrain visible (collision stays active)", value => _groundOn = value);
        _textures = AddToggle(controls, "Textured terrain / simple baseline", value => _texturesOn = value);
        _normals = AddToggle(controls, "Ground normal maps", value => _normalsOn = value);
        AddSection(controls, "FOLIAGE");
        _models = AddToggle(controls, "Tree models visible", value => _treesOn = value);
        _shadows = AddToggle(controls, "Tree shadow casting", value => _shadowsOn = value);
        _unshaded = AddToggle(controls, "Trees unshaded (lighting diagnostic)", value => _unshadedOn = value, false);
        if (_grass != null)
        {
            var grass = new CheckButton { Text = "Grass visible", ButtonPressed = _grass.Visible, FocusMode = Control.FocusModeEnum.None };
            controls.AddChild(grass);
            grass.Toggled += value => _grass.Visible = value;
        }
        bool extraHeading = false;
        foreach (NodePath path in ExtraVisualRoots)
        {
            Node3D? root = GetNodeOrNull<Node3D>(path);
            if (root == null || root == _grass) continue;
            if (!extraHeading) { AddSection(controls, "OTHER MODELS"); extraHeading = true; }
            var toggle = new CheckButton { Text = $"Visible: {root.Name}", ButtonPressed = root.Visible, FocusMode = Control.FocusModeEnum.None };
            controls.AddChild(toggle);
            toggle.Toggled += value => root.Visible = value;
        }
        AddSection(controls, "PERFORMANCE");
        var cap = new CheckButton { Text = $"Cap at {Mathf.Clamp(Settings.FpsCap, 30, 240)} FPS", ButtonPressed = Engine.MaxFps > 0, FocusMode = Control.FocusModeEnum.None };
        cap.Toggled += enabled => Engine.MaxFps = enabled ? Mathf.Clamp(Settings.FpsCap, 30, 240) : 0;
        controls.AddChild(cap);
        var graph = GetNodeOrNull<FrameTimeGraph>("FrameTimeGraph");
        if (graph != null)
        {
            var showGraph = new CheckButton { Text = "Frame-time graph (F2)", ButtonPressed = graph.Visible, FocusMode = Control.FocusModeEnum.None };
            showGraph.Toggled += enabled => graph.Visible = enabled;
            graph.VisibilityChanged += () => showGraph.SetPressedNoSignal(graph.Visible);
            controls.AddChild(showGraph);
        }
        AddSection(controls, "COMPARISON");
        var presets = new HBoxContainer(); controls.AddChild(presets);
        AddButton(presets, "Baseline", () => ApplyCase(0));
        AddButton(presets, "Ground only", () => ApplyCase(1));
        AddButton(presets, "Trees only*", () => ApplyCase(2));
        AddButton(presets, "Full", () => ApplyCase(3));
        controls.AddChild(new Label { Text = "*Trees only keeps simple terrain for a fair view." });
        AddButton(controls, "Run 5-case comparison (~25 seconds)", StartComparison);
        controls.AddChild(new Label { Text = "Wait until trees/chunks settle. VSync/FPS caps can mask costs." });
        _results = new Label { Text = "No results yet. Compare from your current view.", AutowrapMode = TextServer.AutowrapMode.WordSmart, CustomMinimumSize = new Vector2(390, 0) }; controls.AddChild(_results);
        AddButton(controls, "Copy results", () => DisplayServer.ClipboardSet(_report.Length > 0 ? _report.ToString() : "No render comparison has been run."));
        AddButton(rows, "Restore full scene", () => ApplyCase(3));
        AddButton(rows, "Close / resume", () => SetOpen(false));
        if (_terrain == null) { _ground.Disabled = true; _textures.Disabled = true; _normals.Disabled = true; }
        if (_trees == null) { _models.Disabled = true; _shadows.Disabled = true; _unshaded.Disabled = true; }
    }

    // =========================================================
    // Named sections are built once and remain inside the existing scrollable panel.
    private static void AddSection(VBoxContainer rows, string title)
    {
        rows.AddChild(new HSeparator());
        var heading = new Label { Text = title };
        heading.AddThemeColorOverride("font_color", new Color(0.55f, 0.8f, 0.9f));
        heading.AddThemeFontSizeOverride("font_size", 15);
        rows.AddChild(heading);
    }

    // =========================================================
    // Shared toggle builder; applying values happens only after an explicit input event.
    private CheckButton AddToggle(VBoxContainer rows, string text, Action<bool> set, bool initial = true)
    {
        var button = new CheckButton { Text = text, ButtonPressed = initial, FocusMode = Control.FocusModeEnum.None };
        rows.AddChild(button);
        button.Toggled += value => { set(value); ApplyRendering(); };
        return button;
    }

    // =========================================================
    // Buttons retain no keyboard focus, so normal movement keys resume cleanly.
    private static void AddButton(Node parent, string text, Action pressed)
    {
        var button = new Button { Text = text, FocusMode = Control.FocusModeEnum.None };
        parent.AddChild(button);
        button.Pressed += pressed;
    }

    // =========================================================
    // The terrain contract retains all native addon details inside its adapter.
    private void ApplyRendering()
    {
        _terrain?.SetDebugRendering(_groundOn, _texturesOn, _normalsOn);
        _trees?.SetDebugRendering(_treesOn, _shadowsOn, _unshadedOn);
        _ground.SetPressedNoSignal(_groundOn); _textures.SetPressedNoSignal(_texturesOn);
        _normals.SetPressedNoSignal(_normalsOn); _models.SetPressedNoSignal(_treesOn);
        _shadows.SetPressedNoSignal(_shadowsOn); _unshaded.SetPressedNoSignal(_unshadedOn);
    }

    // =========================================================
    // Presets preserve geometry, view distance, lighting, terrain collision and the camera.
    private void ApplyCase(int stage)
    {
        _groundOn = true; _normalsOn = true; _unshadedOn = false;
        _texturesOn = stage == 1 || stage >= 3;
        _treesOn = stage >= 2;
        _shadowsOn = stage != 4;
        ApplyRendering();
    }
    #endregion

    #region Comparison
    // =========================================================
    // A bounded benchmark runs locally on the actual renderer/hardware, not in headless mode.
    private void StartComparison()
    {
        if (_stage >= 0) return;
        if ((_trees != null && _trees.IsPlacing) || (_grass != null && _grass.IsPlacing))
        {
            _results.Text = "Vegetation is still loading; wait, then retry.";
            return;
        }
        _saved = new[] { _groundOn, _texturesOn, _normalsOn, _treesOn, _shadowsOn, _unshadedOn };
        _report.Clear();
        _resultRows.Clear();
        _report.AppendLine("Terra Machine — same-camera rendering comparison");
        _report.AppendLine($"Godot {Engine.GetVersionInfo()["string"]}; viewport {GetViewport().GetVisibleRect().Size}; VSync {DisplayServer.WindowGetVsyncMode()}; MaxFPS {Engine.MaxFps}");
        _report.AppendLine($"Frame graph visible: {GetNodeOrNull<FrameTimeGraph>("FrameTimeGraph")?.Visible ?? false}");
        _report.AppendLine($"Grass visible: {_grass?.Visible ?? false}; grass state is held across all five cases.");
        _report.AppendLine($"Camera: {GetViewport().GetCamera3D()?.GlobalTransform}; trees: {_trees?.PlacedCount ?? 0}");
        _report.AppendLine("Elapsed frame time, not GPU timing. Fixed world/sky/streaming; 2D panel hidden equally in all cases.");
        _stage = 0;
        BeginCase();
        _panel.Hide(); _progress.Show();
        SetProcess(true);
    }

    // =========================================================
    // Allow shader compilation/streaming to settle before collecting frames for each case.
    private void BeginCase()
    {
        ApplyCase(_stage);
        _sampling = false;
        _warmup = Math.Max(0.5, Settings.WarmupSeconds);
        _frames = 0;
        UpdateReadout();
    }

    // =========================================================
    // This callback exists only for the comparison, and stops on finish/cancel/unload.
    public override void _Process(double delta)
    {
        if (_stage < 0) return;
        if (!_sampling)
        {
            _warmup -= delta;
            if (_warmup > 0) return;
            _sampleStart = Time.GetTicksUsec();
            _sampling = true;
            return;
        }
        _frames++;
        double seconds = (Time.GetTicksUsec() - _sampleStart) / 1_000_000.0;
        if (seconds < Math.Max(1.0, Settings.SampleSeconds)) return;
        double fps = _frames / seconds;
        string row = $"{_stage + 1}. {Cases[_stage]}: {fps:F1} FPS / {1000.0 / fps:F2} ms";
        _report.AppendLine(row);
        _resultRows.AppendLine(row);
        GD.Print(row);
        _stage++;
        if (_stage < Cases.Length) { BeginCase(); return; }
        _stage = -1;
        SetProcess(false);
        RestoreRendering();
        _progress.Hide(); _panel.Show();
        _results.Text = _resultRows.ToString();
        UpdateReadout();
    }

    // =========================================================
    // Restore the pre-run choices on both success and cancellation.
    private void RestoreRendering()
    {
        if (_saved.Length != 6) return;
        _groundOn = _saved[0]; _texturesOn = _saved[1]; _normalsOn = _saved[2];
        _treesOn = _saved[3]; _shadowsOn = _saved[4]; _unshadedOn = _saved[5];
        _saved = Array.Empty<bool>();
        ApplyRendering();
    }

    // =========================================================
    // F1/Escape aborts promptly, without leaving the scene in a benchmark preset.
    private void CancelComparison()
    {
        if (_stage < 0) return;
        _stage = -1;
        SetProcess(false);
        RestoreRendering();
        _progress.Hide();
        _report.AppendLine("Comparison cancelled; partial results only.");
        _results.Text = "Comparison cancelled. Partial results can be copied.";
    }

    // =========================================================
    // Throttled live numbers only while the panel or bounded comparison is active.
    private void UpdateReadout()
    {
        if (!_initialized) return;
        double fps = Engine.GetFramesPerSecond();
        ulong draws = RenderingServer.GetRenderingInfo(RenderingServer.RenderingInfo.TotalDrawCallsInFrame);
        ulong primitives = RenderingServer.GetRenderingInfo(RenderingServer.RenderingInfo.TotalPrimitivesInFrame);
        _metrics.Text = $"{fps:F0} FPS · {(fps > 0 ? 1000.0 / fps : 0):F2} ms/frame\nDraw calls: {draws} · Primitives: {primitives:N0}";
        if (_stage >= 0) _progress.Text = $"Comparison {_stage + 1}/{Cases.Length}: {Cases[_stage]}\n{(_sampling ? "Measuring" : "Settling")} · F1/Esc cancels";
    }

    // =========================================================
    // Release any temporary player lock if this optional scene is removed while open.
    public override void _ExitTree()
    {
        Engine.MaxFps = _previousFpsCap;
        CancelComparison();
        SetProcess(false);
        if (_timer != null && GodotObject.IsInstanceValid(_timer)) _timer.Stop();
        if (_open && GodotObject.IsInstanceValid(_player))
        {
            _player.SetMovementEnabled(_restoreMovement);
            _player.InputBlocked = _restoreInputBlocked;
            Input.MouseMode = _restoreMouse;
        }
    }
    #endregion
}
