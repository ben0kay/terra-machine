# Environment

## Atmosphere trial — implemented, awaiting local validation

`environment.tscn` remains the reusable Sky3D setup with explicit SunLight, MoonLight, SkyDome and TimeOfDay children. Time is fixed at **16:00**, with both editor/game progression disabled, for a lower afternoon sun and longer tree shadows. SkyDome uses a restrained warm daylight tint. Vendor files are unchanged.

The scene-local native Environment resource now provides two independently switchable effects:

- **Distant horizon haze:** depth fog begins at 100 m, builds to 32% intensity at 450 m, with a curved transition and cool sky contribution. Near ground remains clear of this depth-fog layer.
- **Volumetric fog / sun shafts:** light density 0.0015, 96 m computed range, directional scattering 0.65, low ambient contribution, no GI injection, temporal reprojection at 0.85. The sun affects the fog and casts shadows; the moon's fog contribution is disabled. This is real shadowed scattering, not decorative ray planes. Visibility of shafts depends on camera/sun alignment, canopy gaps and shadow resolution.

Both start enabled. **F1 → ENVIRONMENT** toggles them independently without editing source resources. These independent atmosphere choices persist when closing F1 and are held across all five terrain/tree comparison cases, recorded in the copied report. Baseline/Full/Restore full scene affect terrain/tree switches only. Disable both fog switches for the previous clear-air comparison (the afternoon sun remains).

Sky3D's own `fog_enabled` and SkyDome's `fog_visible` remain false to avoid a second fog layer. Native volumetric fog requires Forward+; this project's existing renderer is Forward+. `project.godot` explicitly uses a modest 64 × 64 volume size/depth setting; increasing resolution costs more GPU work. Density alone is not a quality/performance control.

## Tuning and lifetime

Tune the Environment subresource in `environment.tscn` for density, colour, depth/range and scattering; tune SkyDome/TimeOfDay there for sun colour and time. No extra C# sky manager, fog timer, per-tree script, weather or day/night gameplay was added. Debug writes occur only on checkbox input. Turning volumetric fog off disables the native effect rather than making an active volume transparent. Sky3D retains its existing shader/cloud work; enabled volumetric fog has an additional GPU cost.

Global haze also appears inside future caves/buildings: no interior exclusion is part of this pass. There are no visible ray guarantees from every angle, no measured FPS claim and no forest expansion.

## Validation

Source checks: reviewed Sky3D ownership/initialization, resource locality, fog properties, scene paths, both fixed-time values, sun/moon fog contribution and event-only toggle/report wiring. **C# compilation, Godot scene loading, visual appearance and performance are not verified here** (Godot/.NET executables unavailable).

Local: build `terra-machine.csproj`, run `WORLD/Scenes/test.tscn`, wait for terrain/vegetation to settle. Look toward the afternoon sun through trees with tree shadows enabled, then across the distant hills. Inspect for washed-out near grass, blocky shafts, temporal trails while turning and a visible 96 m fog boundary. F1: compare haze only, volumetrics only, both and neither from the same camera. Use the frame-time graph while walking; briefly disable the FPS cap to assess headroom, then restore it. Target remains stable 60 FPS with dense grass before expanding the benchmark.

Reference: [Godot volumetric fog](https://docs.godotengine.org/en/stable/tutorials/3d/volumetric_fog.html), [Environment properties](https://docs.godotengine.org/en/stable/classes/class_environment.html).
