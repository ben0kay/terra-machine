# Rendering isolation overlay

Source implemented; local UI/benchmark acceptance pending. Inspired by War Machine's scene-instanced panel, explicit ready-reference binding and event-only changes. No duplicate world manager or per-tree debug loop.

## Use

Run WORLD/Scenes/test.tscn, wait for the tree group/chunks to settle, frame a useful view, then press **F1**. The player and camera lock while the panel is open. Mouse is released; clicks outside the panel cannot recapture it. F1 or Escape closes, cancels a running comparison and restores the previous player movement/input/cursor state. F4 respawn is blocked while the panel is open. Changes are temporary to this running scene and persist when merely closing the panel; Restore full scene resets the render switches.

Controls: terrain visibility, textured/simple ground shader, ground normal maps, tree visibility, tree shadow casting and tree unshaded diagnostic. Hiding visuals does not stop terrain generation/streaming/collision. No toggles change world generation, view distance or physics. Terrain controls use the game-owned ITerrainRenderDebug interface; only the adapter knows the native material property. Runtime materials are private cached copies, so saved settings/resources are unchanged. Cached tree meshes/material clones support visibility/shadow/unshaded changes without hierarchy scans on frames.

The optional `ExtraVisualRoots` array on the overlay accepts NodePaths relative to it (e.g. ../Props). Each explicitly assigned Node3D gets a visibility checkbox. Use parent roots to group models; no full-world mesh discovery. Add specialized future section controls through explicit owners/interfaces rather than reaching into addon internals.

## Same-camera comparison

Run comparison rejects starting while the tree-placement helper is active. The default five cases are:

1. Baseline: original simple ground, trees hidden.
2. Textured ground, trees hidden.
3. Trees and their shadows, simple ground retained.
4. Full textured ground, trees and shadows.
5. Full scene with tree shadow casting disabled.

Each gets 2 seconds to settle and 3 seconds of counting actual process frames over elapsed wall time. The panel hides during all cases, leaving the same small progress label. No camera motion, changed resolution, new tree placements or respawn during the run. Results are average FPS and elapsed milliseconds/frame, printed to Output, retained in the panel and available through Copy results. They are **not GPU timings**. Header records engine version, viewport size, VSync, FPS cap, camera transform and placed-tree count. Live draw calls/primitives are throttled instantaneous rendering counters, not averages or a CPU/GPU bottleneck diagnosis.

Success or cancellation restores the user's pre-run render choices. Losing application focus cancels to avoid background-throttled measurements. Avoid changing window size, other settings or GPU workloads during comparison. Shader compilation, incomplete chunk settling, VSync/FPS caps and active clouds can affect results; rerun only if needed. The helper cannot establish the bottleneck without measurements from the user's actual renderer/hardware.

Interpret differences in milliseconds, using the same view: case 2 minus 1 estimates the textured-ground cost, case 3 minus 1 tree/shadow cost, case 4 minus 5 tree-shadow contribution. Effects need not add perfectly. Ground normals can also be toggled manually; unshaded trees help diagnose dark lighting, not texture quality. These toggles are tools, not a claim that any renderer optimization has been delivered.

## Performance and lifecycle

Closed: no update/physics callbacks or debug timer. Open: a 4Hz readout and event-driven UI; no debug per-frame callback. Only the bounded comparison enables Process to count frames, disabling it on finish, cancellation and scene removal. No native terrain calls outside the adapter, no per-frame material writes, no per-tree scripts/scans. Normal-map mode swaps between cached material resources so native chunk material instances refresh consistently. Terrain shader LOD transitions are shared through Integration/transvoxel.gdshaderinc in both simple and textured shaders.

## Validation

All 12 project C# scripts compile through Roslyn against .NET 8 reference assemblies and GodotSharp 4.4.1 (zero errors/warnings after correction). This is a focused API/syntax check, **not** the configured Godot.NET.Sdk/4.7.2 source-generator/project build. A direct dotnet CLI build could not run in this environment because process-stat access caused its CLI startup to abort. Godot 4.4.1 isolated headless checks parse both shaders (18 and 1 uniforms) and load the textured material. Paths, load_steps and new loop syntax were checked. No full game/UI run, GPU benchmark, measured performance gain or exact-version runtime compatibility claim.

Local: build terra-machine.csproj in the installed Godot .NET editor. Run test.tscn. Open F1 after Trees:16, test every toggle and the restore button; ensure terrain collision remains when hidden. Click outside the panel and verify the view stays locked; close and verify movement/look/RMB jump return. Run comparison, Copy results, then test F1/Escape cancellation and alt-tab cancellation. Close/reopen and stop/reload the scene. In the profiler, debug Process/timer must stop after close. Send the five copied rows to identify where the frame cost lies before optimizing or increasing tree density.

Grass trial: test.tscn assigns GrassScatter through ExtraVisualRoots. Its visibility toggle also suspends/resumes placement and the tile timer. Visible grass must finish placement/bounded retries before comparison. Grass visibility is held constant through the five cases and recorded in their header; the Baseline/Full buttons do not change this independent toggle. Compare grass on/off manually at the same camera for its cost.
