# Ground and fir test — authorized visual slice

Status: user reported the basic first-person visual/functionality test passed. Reported FPS fell roughly from 300 to 125 (3.3 to 8ms/frame); cause unmeasured. A separately authorized F1 debug comparison is implemented in source; local overlay/benchmark validation is pending. Extends the existing test.tscn; this is a subset of ENV0 and an early small tree composition preview, not completion of the entire visual-first roadmap.

Scope: a shared three-layer ground material (forest floor, grass patches and slope rock) with color/normal/roughness, triplanar mapping, editable repeat/patch/normal controls and existing Transvoxel LOD transitions. Twelve A/B/C fir instances ahead of spawn, native collision-grounded placement, corrected Blender offsets, shared cutout material clones and zero placement processing after initialization. HUD reports placed fir count. Existing player, voxel generation, collision/streaming, sky/time and controls are preserved.

No dense grass, full forest generator/streaming, tree collision, terrain reshaping, digging/persistence, weather or gameplay systems. No claims of a finished beautiful landscape or measured performance gains.

Validation: inspected all GLB JSONs (one LOD1 node each, embedded images; B/C offsets and BLEND foliage). Resource paths/load_steps and focused C# loop/syntax review. Godot 4.4.1 isolated headless shader/material import/load check; this caught unsupported EXR compression and prompted the six PNG conversions. .NET compilation and full test-scene execution are not verified here. Shader check is not GPU visual validation.

Local acceptance:
1. Pull and let texture/model/C# import finish. Build `dotnet build terra-machine.csproj` using the matching Godot .NET SDK; F6 WORLD/Scenes/test.tscn.
2. Look down: detailed forest floor/grass patches instead of flat color. Walk up slopes: exposed rock, no stretched vertical UVs or obvious chunk material seams. Change tile size/normal strength in GroundMaterial.tres and restart to tune.
3. Walk forward along -Z: twelve firs appear after nearby collision loads, roots contact ground, no B/C display offsets. Check needles at close/distant views and shadows. HUD should reach Firs: 12; inspect any timeout warning. Trees deliberately have no collision.
4. F4 must not duplicate trees. Compare FPS/frame time with TreePreview removed; placement physics callbacks should be disabled after placement. Test scene stop/reload for resource errors. Review a close ground view, a tree-root view and a view across the group before extending scope.

Render-cost isolation: see ../DEBUG/Overlay/README.md. Do not attribute the reported FPS drop to tree node count or terrain shader alone without same-view results.
