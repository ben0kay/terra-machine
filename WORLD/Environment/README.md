# Environment

## Atmosphere trial — implemented, awaiting local validation

`environment.tscn` remains the reusable Sky3D setup with explicit SunLight, MoonLight, SkyDome and TimeOfDay children. Time starts at **16:00**, with both editor/game progression disabled, for a lower afternoon sun and longer tree shadows. SkyDome uses a restrained warm daylight tint. Vendor files are unchanged.

The scene-local native Environment resource now provides two independently switchable effects:

- **Distant horizon haze:** depth fog begins at 65 m, builds to 65% intensity at 300 m, with a curved transition and cool sky contribution. Near ground remains clear of this depth-fog layer.
- **Volumetric fog / sun shafts:** light density 0.0015, 96 m computed range, directional scattering 0.65, low ambient contribution, no GI injection, temporal reprojection at 0.85. The sun affects the fog and casts shadows; the moon's fog contribution is disabled. This is real shadowed scattering, not decorative ray planes. Visibility of shafts depends on camera/sun alignment, canopy gaps and shadow resolution.

Both start enabled. **F1 → ENVIRONMENT** toggles them independently without editing source resources. These independent atmosphere choices persist when closing F1 and are held across all five terrain/tree comparison cases, recorded in the copied report. Baseline/Full/Restore full scene affect terrain/tree switches only. Disable both fog switches for the previous clear-air comparison (the afternoon sun remains).

Sky3D's own `fog_enabled` and SkyDome's `fog_visible` remain false to avoid a second fog layer. Native volumetric fog requires Forward+; this project's existing renderer is Forward+. `project.godot` explicitly uses a modest 64 × 64 volume size/depth setting; increasing resolution costs more GPU work. Density alone is not a quality/performance control.

## Time-of-day preview and slower clouds

**F1 → ENVIRONMENT → Time of Day** provides a 00:00–23:55 slider in five-minute steps and an HH:MM readout. It writes Sky3D's public `current_time` property, whose TimeOfDay setter immediately refreshes celestial coordinates: the sun, moon and sky stay coordinated. It does not rotate the sun independently or enable automatic time progression. Closing F1 keeps the chosen time for this run; **Reset time to scene default** restores the startup time (currently 16:00). Restarting the scene restores authored settings. Baseline/Full controls leave the time unchanged. Copied benchmark reports record the selected hour, held across all five cases.

Sky3D and SkyDome both explicitly use `wind_speed = 0.25`, down from the addon default 1.0: roughly one-quarter cloud translation speed. Cirrus retains its existing additional speed reduction. No vendor code, grass wind, weather or time-cycle logic was changed.

The slider has no timer/process callback and only updates on value changes; five-minute steps limit celestial recomputation while dragging. Sky3D's existing cloud animation and short ambient-light transitions remain intentional. Large jumps between day/night may need a moment for the ambient transition and volumetric history to settle before judging/benchmarking.

Current checks: inspected Sky3D's forwarding property, TimeOfDay's immediate coordinate refresh and SkyDome's wind multiplier; reviewed slider range, reset, label/report wiring and both scene wind values. **Compilation/UI/runtime checks remain local**. Build/run, drag through midnight/noon/sunset, close/reopen F1, reset time, confirm camera input resumes and cloud movement slows. Allow lighting/fog to settle before comparison. This doesn't change the existing moon shadow settings or fog colours into a complete night-art pass.

## Tuning and lifetime

Tune the Environment subresource in `environment.tscn` for density, colour, depth/range and scattering; tune SkyDome/TimeOfDay there for sun colour and time. No extra C# sky manager, fog timer, per-tree script, weather or day/night gameplay was added. Debug writes occur only on checkbox/slider input. Turning volumetric fog off disables the native effect rather than making an active volume transparent. Sky3D retains its existing shader/cloud work; enabled volumetric fog has an additional GPU cost.

Global haze also appears inside future caves/buildings: no interior exclusion is part of this pass. There are no visible ray guarantees from every angle, no measured FPS claim and no forest expansion.

## Validation

Source checks: reviewed Sky3D ownership/initialization, resource locality, fog properties, scene paths, both fixed-time values, sun/moon fog contribution and event-only toggle/report wiring. **C# compilation, Godot scene loading, visual appearance and performance are not verified here** (Godot/.NET executables unavailable).

Local: build `terra-machine.csproj`, run `WORLD/Scenes/test.tscn`, wait for terrain/vegetation to settle. Look toward the afternoon sun through trees with tree shadows enabled, then across the distant hills. Inspect for washed-out near grass, blocky shafts, temporal trails while turning and a visible 96 m fog boundary. F1: compare haze only, volumetrics only, both and neither from the same camera. Use the frame-time graph while walking; briefly disable the FPS cap to assess headroom, then restore it. Target remains stable 60 FPS with dense grass before expanding the benchmark.

Reference: [Godot volumetric fog](https://docs.godotengine.org/en/stable/tutorials/3d/volumetric_fog.html), [Environment properties](https://docs.godotengine.org/en/stable/classes/class_environment.html).

## Readable nights and stronger horizon haze — awaiting local validation

Compared War Machine `implement-td0-td1` at 733b1d8: its player-distance overlay is clear to ~24 m and opaque by ~48 m in the top-down profile, with a separate FPS override. Terra keeps its native depth fog and long landscape views: clear to 65 m, reaching 65% at 300 m, depth curve 1.2, sky affect 0.3. The previous 100–450 m / 32% / curve 1.6 profile was very weak at ordinary forest distances. No fullscreen shader, camera far-plane linkage or simulation culling was copied.

Native ambient source is now Color with daytime sky contribution 0.7, energy 1.15 and Sky3D night contribution 0.2. This lets the addon's ambient colour supply more night fill instead of relying mostly on the dark sky. SkyDome night tint is (0.22, 0.28, 0.4), with brighter night clouds and moon energy 0.8. The existing Sky3D celestial updates/tween handle transitions; no second clock or per-frame override was added. Slower cloud speed and 16:00 startup remain.

Use F1 time slider at 00:00/03:00/20:00/23:00 and allow transitions/fog history to settle. Target: readable blue-toned terrain/trunks at night, distinct from daylight, plus a visible distant fade without concealing the near forest. Compare haze on/off across the same valley. Stronger native depth fog doesn't increase volumetric resolution or introduce another rendering pass. Exact appearance, night shadows and frame time still need local acceptance; source/property/scene checks only.
