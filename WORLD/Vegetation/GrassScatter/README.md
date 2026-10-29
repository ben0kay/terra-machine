# Player-centred grass trial

Implemented, awaiting local visual/runtime/performance acceptance. `GrassScatter.tscn` is composed into `WORLD/Scenes/test.tscn`; TestWorld activates it after the player finds real terrain collision. No grass nodes or addon calls belong in the player. Remove the scene to disable it, or use **F1 → Visible: GrassScatter**.

## Configuration and appearance

`Configuration/GrassMaster.tres` selects the imported Bermuda asset, shared cutout material and three named small/medium meshes. Only those mesh resources are used; Blender's display translations are ignored. Per-mesh bounding-box base/centre corrections put each tuft at the queried ground point. The small source plants are scaled 3–4.5x (roughly 17–44 cm tall across these variants). Irregular density patches, random yaw/scale and mixed variants within every tile avoid square species groups. Grass aligns to the hit normal, sinks 1.5 cm and rejects slopes above 35 degrees.

Defaults: 16 m tiles, 3 candidate tufts/m² before patch/slope rejection, full density inside 20 m, thinning to ~22% by 45 m, shrinking out over 45–60 m. This is an initial light coverage trial, not a measured optimum or full lawn. Tune density/scale after viewing at eye level. The shader uses the current camera position, so fading follows camera movement without per-frame CPU uniform writes. It shrinks/discards individual tufts with staggered thresholds; no alpha-blended sorting. Shadows start disabled, collision is absent and no wind or gameplay interaction is added.

## Work and lifetime

A 0.25-second timer checks the player's tile coordinate. Crossing a tile boundary queues nearby tiles nearest-first and removes distant batches. A conservative extra ring keeps coverage to 60 m as the player moves within a tile. Each tile has at most three MultiMesh draw batches sharing meshes/material; no per-tuft nodes/scripts. CPU queue rebuild/allocation occurs on crossings or bounded retries, not every frame.

Physics placement runs only while work is queued: at most 64 surface rays and 512 candidate visits per physics tick. The optional game-owned `ITerrainSurfaceService` provides ground position/normal through one physics ray; native-addon details remain in the adapter. Tile centres with unloaded collision get up to four checks, two seconds apart, then remain empty until unloaded/revisited. Individual failed samples inside a loaded tile are skipped; this first pass does not repair every partial collision gap. F4/teleport is handled at the next timer check and does not duplicate tiles.

When settled, PhysicsProcess is disabled; only the 4 Hz position/retry timer remains. Hiding the grass stops that timer and placement, and showing resumes them. Existing hidden batch data remain cached. Visible grass still has GPU vertex/material cost. On scene removal, child timers/batches are freed with their owner. Tiles outside the ring are removed; no expanding world cache is retained. Terrain edits, persistence, biome-exact matching to the ground shader, collision-triggered regrounding and a general vegetation streamer are deferred.

## Debug and validation

F1 visibility is independent of tree/terrain controls. The five-case comparison waits for visible grass placement/retries to settle, holds its visibility unchanged across all cases and records its visibility in the report. To measure grass cost, compare the same camera with only the grass toggle changed; the existing five rows still isolate terrain/trees, not grass.

Checks run: all C# sources compiled with Roslyn against available .NET 8/GodotSharp 4.4.1 references; not the configured Godot.NET.Sdk 4.7.2/source-generator build. Godot 4.7.2 headless loaded the updated shader/material (8 uniforms), found all three named meshes (one surface each), and created sample MultiMesh resources. Headless rendering does not verify GPU appearance or live C# streaming.

Local acceptance: build/run test.tscn, allow nearby grass to settle, inspect alpha edges/root placement and slopes, walk >100 m and return, check F4, hide/re-enable grass through F1, and compare FPS at a fixed view. Check the profiler while stationary: GrassScatter physics should stop after placement; the 4 Hz tile timer is intentional. Review far fade and density before increasing the budget. First-time shader compilation/terrain loading may cause temporary stalls; no measured FPS claim is made here.
