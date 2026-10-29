# DEBUG

Optional debug tools live here, separate from gameplay. Instance `Overlay/DebugOverlay.tscn` in the test scene; TestWorld binds ready references after grounded spawn. `Configuration/DebugMaster.tres` controls enabled state, benchmark warmup/sample duration and readout frequency. No War Machine controllers or managers are imported.

F1 toggles the overlay. It freezes player movement/look, releases the cursor and exposes render-only terrain/tree switches, isolation presets and a bounded same-camera comparison. Closed overlay has no Process/PhysicsProcess callback or running timer. Input events still detect F1. See Overlay/README.md for controls, methodology and validation.
