# Current Bermuda patch layout

Bermuda now uses 4 candidate tufts/m² (previously 8), a world-space patch mask with 7.5 m spacing and 2.6 m nominal radius, and 0.35 minimum density acceptance. Patch centres jitter within cells and radii vary from 75–120%, leaving irregular open gaps rather than uniform floor coverage. Existing per-tuft cell jitter, random yaw/scale and mixed meshes remain. The mask spans tile boundaries; existing distance thinning, excavation removal and placement budgets remain. Bermuda alone no longer includes or evaluates vegetation wind: its diffuse, alpha, normal and roughness textures and distance fade remain active. Meadow grass, ferns and other plants retain wind. These current settings supersede the historical density passes below.

Source/resource checks only; visual patch spacing and FPS require a local Godot run.

# Current forest grass update

The forest now uses the substantially lighter GrassMeadow002 A/B/C LOD1/LOD2 scenes in broad clumps, slightly enlarged with per-instance variation, with matching grassy ground and leaf litter in gaps. The old anchored MeadowPreview instance is absent from the test scene, while the separate SpawnMeadow002 layer remains enabled near the original firs. See Configuration/FOREST_MEADOW_README.md for current forest defaults/validation. Bermuda settings below remain independent.

# Player-centred grass trial

Implemented, awaiting local visual/runtime/performance acceptance. `GrassScatter.tscn` is composed into `WORLD/Scenes/test.tscn`; TestWorld activates it after the player finds real terrain collision. No grass nodes or addon calls belong in the player. Remove the scene to disable it, or use **F1 → FOLIAGE → Grass visible**.

## Configuration and appearance

`Configuration/GrassMaster.tres` selects the imported Bermuda asset, shared cutout material and three named small/medium meshes. Only those mesh resources are used; Blender's display translations are ignored. Per-mesh bounding-box base/centre corrections put each tuft at the queried ground point. The small source plants are scaled 3–4.5x (roughly 17–44 cm tall across these variants). Jittered, stratified placement across each tile, irregular density patches, random yaw/scale and mixed variants within every tile avoid square species groups. Grass aligns to the hit normal, sinks 1.5 cm and rejects slopes above 35 degrees.

Defaults: 16 m tiles, 12 candidate tufts/m² before slope rejection. Patch acceptance is 75–98% (about 10.4 tufts/m² on suitable flat ground, versus about 1.5 previously). `MinimumPatchCoverage` preserves a meadow base instead of broad bare bands; size remains 3–4.5x. Candidate count is capped at 8192 per tile. This is a denser visual benchmark, not a measured optimum or a claim of matching Far Cry.

Distance thinning now reduces `MultiMesh.VisibleInstanceCount` at the existing 4 Hz timer, from full density within 20 m to `DistantDensityFraction` (15%) by 55 m. Distance is conservatively measured to the nearest tile edge, keeping all nearby grass full but retaining extra density across tile boundaries. Per-variant instance order is spatially random, so prefixes keep distributed coverage. Density changes happen per tile and can produce visible changes while walking; inspect that locally. Unlike shader-only thinning, omitted instances are not submitted for vertex processing. Buffers still store full density. Shader shrinking/discard handles the final 55–80 m fade from the current camera position, with no per-frame CPU uniform writes or alpha-blended sorting. Shadows start disabled, collision is absent and no wind or gameplay interaction is added.

## Work and lifetime

A 0.25-second timer checks the player's tile coordinate and adjusts batch visible counts (writes only when changed). Crossing a tile boundary queues nearby tiles nearest-first and removes distant batches. A conservative extra ring keeps coverage to 80 m as the player moves within a tile. Each tile has at most three MultiMesh draw batches sharing meshes/material; no per-tuft nodes/scripts. CPU queue rebuild/allocation occurs on crossings or bounded retries, not every frame.

Physics placement runs only while work is queued: at most 64 surface rays and 512 candidate visits per physics tick. The optional game-owned `ITerrainSurfaceService` provides ground position/normal through one physics ray; native-addon details remain in the adapter. Tile centres with unloaded collision get up to four checks, two seconds apart, then remain empty until unloaded/revisited. Individual failed samples inside a loaded tile are skipped; this first pass does not repair every partial collision gap. F4/teleport is handled at the next timer check and does not duplicate tiles.

When settled, PhysicsProcess is disabled; only the 4 Hz position/retry/batch-density timer remains. There is no per-tuft CPU update. Hiding the grass stops that timer and placement, and showing resumes them. Existing hidden batch data remain cached. Visible grass still has GPU vertex/material cost. On scene removal, child timers/batches are freed with their owner. Tiles outside the ring are removed; no expanding world cache is retained. Terrain edits, persistence, biome-exact matching to the ground shader, collision-triggered regrounding and a general vegetation streamer are deferred.

## Wider coverage pass — awaiting local validation

Grass now reaches 80 m (previously 60 m), thinning from 20–55 m and fading over 55–80 m. Candidate positions use jittered cells rather than independent random points to reduce accidental clusters and holes. A shared shuffled cell order keeps the retained MultiMesh prefix distributed across the tile when distance thinning is active. Rejection for slope, patch variation and unloaded collision still applies; this does not guarantee a continuous carpet.

Near density/scale and the fixed placement budget are unchanged. Wider coverage means more retained tile buffers, more total placement rays and potentially more rendering cost. Near tiles still fill first. No per-tuft callbacks were added; settled work remains the existing 4 Hz tile/density timer, stopped by hiding grass.

Checks: shuffled-cell uniqueness and spatial distribution, settings/ranges, loop syntax review and single grass-toggle wiring. Compilation, visual continuity, panel layout and frame-time checks remain local because Godot/.NET are unavailable here. Inspect both stationary and walking views, especially tile/fade boundaries, then compare F1 grass on/off after placement settles.

## Dense meadow pass — awaiting local validation

The fixed 64-ray/physics-tick budget is unchanged. More tufts mean longer fill time and more buffer memory, even when fewer distant instances are drawn. Let grass settle before comparing steady-state costs; test travel separately for placement stalls. Startup still fills whole tiles nearest-first, so batches appear progressively. No shadows or wind added.

Current pass checks: reviewed settings/resource links, candidate cap and C#/shader interface; numerically checked density falloff and expected patch acceptance. Compilation and Godot shader/runtime checks are **not verified** in this environment (no .NET/Godot executables). Earlier checks below apply to the previous pass only.

Acceptance target: dense overlapping near grass with stable 60 FPS (16.7 ms/frame) while stationary and walking. F1 grass on/off from an identical camera separates its cost. Briefly disable the FPS cap to assess headroom, then restore it; use F2 frame-time spikes to check travel. Inspect tile density transitions, slope rejection, roots, fade, and hide/re-enable. If coverage remains insufficient, adjust asset mix/coverage after this test rather than assuming more instances always solve it. This milestone must be accepted before expanding the vegetation area.

## Debug and validation

F1 visibility is independent of tree/terrain controls. The five-case comparison waits for visible grass placement/retries to settle, holds its visibility unchanged across all cases and records its visibility in the report. To measure grass cost, compare the same camera with only the grass toggle changed; the existing five rows still isolate terrain/trees, not grass.

Previous pass checks (not rerun for the dense meadow changes): all C# sources compiled with Roslyn against available .NET 8/GodotSharp 4.4.1 references; not the configured Godot.NET.Sdk 4.7.2/source-generator build. Godot 4.7.2 headless loaded the updated shader/material (8 uniforms), found all three named meshes (one surface each), and created sample MultiMesh resources. Headless rendering does not verify GPU appearance or live C# streaming.

Local acceptance: build/run test.tscn, allow nearby grass to settle, inspect alpha edges/root placement and slopes, walk >100 m and return, check F4, hide/re-enable grass through F1, and compare FPS at a fixed view. Check the profiler while stationary: GrassScatter physics should stop after placement; the 4 Hz tile timer is intentional. Review far fade and density before increasing the budget. First-time shader compilation/terrain loading may cause temporary stalls; no measured FPS claim is made here.

## Forest meadow companion and slightly looser Bermuda — awaiting visual validation

The regular Bermuda grass has fewer candidates (12 → 8 tufts/m²), looser minimum patch coverage (0.75 → 0.62), and 12% distant density instead of 15%. Its existing 20–55 m CPU MultiMesh instance thinning and 55–80 m vertex shrink/fade remain; Godot's import-generated mesh LODs are still enabled. A second GrassScatter instance in NatureRoot/Grass, `ForestMeadowScatter`, uses separate LOD1/LOD2 scene arrays, the existing shared meadow wind material, and an ellipse passed from ForestPreview settings. No separate C# manager or per-tuft scripts were added. Native visibility ranges select meadow LOD1 nearby and authored LOD2 farther away; both share a tile AABB. The shared 4 Hz tile timer updates visible instance counts. All patches reuse the terrain-only surface query contract, and trees/rocks are unchanged.
