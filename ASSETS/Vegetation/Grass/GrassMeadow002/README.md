# Grass Meadow 002

Three authored grass-clump variants A/B/C. `Models/` contains individual LOD1 and LOD2 glTF exports for each, with shared 1K color/alpha texture in `Textures/` and GPU wind in `Materials/`. LOD1 triangles per tuft: **60 / 66 / 54**; LOD2 triangles: **24 / 30 / 24**. Native heights are roughly 0.75–0.9 m. Do not confuse A/B/C appearance variations with LOD levels.

Two independent uses in the test world:

- `WORLD/Vegetation/MeadowPreview/Configuration/SpawnMeadow002Master.tres`: original modest bunches near 12 starting firs, scale 0.88–1.13, original `Materials/MeadowGrass002Wind.tres`, no authored far-LOD switching.
- `WORLD/Vegetation/GrassScatter/Configuration/ForestMeadowMaster.tres`: flat-side forest clumps, scale 0.75–0.95 for 85% of tufts with 15% tall accents at 1.30–1.65, density 0.65 candidates/m² and 1.9 m minimum root clearance within each tile, paired LOD1/2 with a switch around 24 m. Uses dedicated `Materials/ForestMeadow002Wind.tres` with opt-in far-distance fade; original spawn material is unaffected.

Both use the same shader source, with distance fade disabled by default so other materials opt in explicitly. Neither requires grass collision or per-tuft C# updates. The existing GrassMedium001 exports are retained as inactive art sources for optional future use. In-game import/shader compile, appearance and FPS await local verification.

## Foliage shading pass — awaiting local visual acceptance

Both existing materials now use the same updated lit cutout shader. Atlas path/UVs, mesh/buffer files, alpha cutoff, placement density/size and native LOD settings are unchanged, so an atlas replacement with the same layout/alpha alignment and filename remains a drop-in update. Existing local wind and distance fade are preserved.

Per-clump colour comes from a deterministic centimetre-quantised world XZ root hash, with restrained fresh/olive-dry tints and ±6% brightness. No INSTANCE_ID/custom-data dependency, texture duplication or C# material updates. Native LOD batches with identical transforms therefore share the seed; ground-foliage compaction does not change surviving colours. This adds instance variation to the existing authored vertex colours/texture.

The shader uses local unanimated blade height (normalised by plant_height=0.85) to apply gentle root darkening (0.10 over the bottom 30%) and a small ambient-only root occlusion term. Preserve the existing authored vertex colours. New texture art may already contain darker roots, so root_darkening can be reduced to 0 to disable both additional root terms. There is no new normal map or invented texture-dependent blade detail.

Two-sided lighting orients the interpolated card normal toward the viewed side, without blindly flipping an already-facing normal. It does not author new mesh normals or force all blades upward. Built-in BACKLIGHT provides restrained direct-light transmission (0.18, lower at roots); it does not use emission, unshaded rendering, screen-space scattering or custom light loops. Roughness stays 0.9, metallic 0 and specular 0.25. Scene lighting/night ambient is untouched.

Tune under each Materials/*.tres → Shader Parameters → Foliage: color_variation (0 removes tint variation), brightness_variation (0 removes brightness variation), fresh_tint/dry_tint, root_darkening, root_shade_height, two_sided_lighting, transmission_strength (0 disables backlighting), surface_roughness and plant_height. Root/height controls use source-mesh units, independent of placement scale. Spawn/forest resources can be tuned separately; restart to apply to the forest's runtime material copy.

Performance: one existing atlas sample, additional per-vertex hash/height work and ordinary built-in foliage lighting; no additional draw calls, maps, triangles, per-instance scripts/timers, normal-map fetches or screen-space SSS. More visible lighting cost is possible; no measured FPS claim. Actual density, overdraw, trees and night-fill problems remain separate concerns.

Checked source/resource paths, parameter declarations, shader delimiters and preservation of existing fade/wind/atlas wiring against the current repository. Godot is unavailable here: shader compilation, appearance, shadow/backlight behaviour, LOD/compaction consistency and FPS remain local checks. A replacement atlas was pushed concurrently and is preserved in this commit's base; its pixel content was not inspected here. Final colour calibration must use that current atlas.

Local: pull/reopen, run test.tscn, compare fixed camera views at noon, dusk and night using F1 time-of-day; view both sides and put sun in front/behind the grass. Confirm rooted bases, no self-lit grass without direct light, no colour pop at LOD changes, and stable colours after digging removes neighbouring tufts. A/B variation can be checked by setting color_variation/brightness_variation to 0; reduce root_darkening if the new atlas already shades roots. Do not judge grass alongside trees with tree-only Unshaded enabled. Compare F2 frame time before raising transmission or density.

Reference for BACKLIGHT and lit shader outputs: https://docs.godotengine.org/en/4.7/tutorials/shaders/shader_reference/spatial_shader.html
