# All-tree performance diagnostics — current

F1 → FOLIAGE now controls both original TreePreview and ForestPreview, including forest fir saplings. The redundant independent ForestPreview visibility checkbox is removed. Visibility remains render-only; trunk collision stays active. New placements inherit current shadow, material and LOD choices. The five-case comparison now includes the forest, rejects runs while it is still placing, restores pre-run visibility/shadow/unshaded state and records tree count, LOD mode/split and sun shadow distance.

- ALL trees visible: hides/shows both owners, including original firs, original pines, forest pines and forest firs.
- Tree shadow casting: Off disables every registered tree mesh's shadow casting. On restores each mesh's original policy, so LOD2 and firs originally configured without shadows stay without shadows.
- Pine LOD1 → LOD2 distance: 5–140 m, initially 70 m. Native visibility ranges still perform distance switching.
- Pine LOD mode: Auto, force detailed LOD1, or force simplified LOD2. Forced modes retain the asset's outer cull distance; firs do not have an authored pair and remain unchanged.
- Sun shadow distance: 10–300 m, starts at scene value. This is the directional light's global shadow distance and affects all scenery, not just trees.

These changes are session-only. At the same camera, disable the FPS cap, wait for loading to settle, then compare tree shadows off and forced LOD2 separately. Preserve the same LOD/distance settings across comparison runs. Slider changes do not add frame polling; cached mesh/material/range settings update on UI input or placement. Materials are shared per source within each owner; geometry, physics and grass are unchanged.

Validation: resource/node paths and C# loop/state wiring reviewed. Godot/.NET unavailable here; compilation, native LOD transitions, all-tree visibility, shadow isolation and measured performance require local validation. Check toggling off/on, travel to place new trees, cancel a comparison and F4 respawn.

# Rendering isolation overlay

Source implemented; local UI/benchmark acceptance pending. Inspired by War Machine's scene-instanced panel, explicit ready-reference binding and event-only changes. No duplicate world manager or per-tree debug loop.

## Use

Run WORLD/Scenes/test.tscn, wait for the tree group/chunks to settle, frame a useful view, then press **F1**. The player and camera lock while the panel is open. Mouse is released; clicks outside the panel cannot recapture it. F1 or Escape closes, cancels a running comparison and restores the previous player movement/input/cursor state. F4 respawn is blocked while the panel is open. Changes are temporary to this running scene and persist when merely closing the panel; Restore full scene resets the render switches.

Controls are grouped under **ENVIRONMENT** (time of day, horizon haze, volumetric fog and terrain), **FOLIAGE** (trees and grass), **OTHER MODELS** (only when additional roots exist), **PERFORMANCE** (FPS cap and frame graph), and **COMPARISON** (presets/results). Headings and separators are built once inside the existing scroll area; no polling/rebuild work is added. Grass has a dedicated visibility toggle, and is skipped in ExtraVisualRoots to prevent a duplicate checkbox. Local layout/compilation checks for this grouping are pending.

Controls: terrain visibility, textured/simple ground shader, ground normal maps, tree visibility, tree shadow casting and tree unshaded diagnostic. Hiding visuals does not stop terrain generation/streaming/collision. No toggles change world generation, view distance or physics. Terrain controls use the game-owned ITerrainRenderDebug interface; only the adapter knows the native material property. Runtime materials are private cached copies, so saved settings/resources are unchanged. Cached tree meshes/material clones support visibility/shadow/unshaded changes without hierarchy scans on frames.

The optional `ExtraVisualRoots` array on the overlay accepts NodePaths relative to it (e.g. ../Props). Each explicitly assigned Node3D gets a visibility checkbox. Use parent roots to group models; no full-world mesh discovery. Add specialized future section controls through explicit owners/interfaces rather than reaching into addon internals.

## Time-of-day slider

F1 → ENVIRONMENT now includes an HH:MM readout and 00:00–23:55 slider (five-minute steps). Input writes Sky3D's public `current_time`, updating its celestial lighting immediately while automatic time progression stays disabled. Reset time to scene default restores the startup value (currently 16:00). Closing the panel retains the selected time; restarting restores scene defaults. The existing Restore full scene button still affects terrain/tree render choices only.

Slider work is event-only, with no additional poll/timer/process; input stepping bounds sky updates. The five-case comparison records time and holds it unchanged. Wait for Sky3D ambient transitions and fog temporal history to settle after large jumps. Cloud speed now defaults to 0.25 in the environment scene, versus the addon default 1.0.

Source/property/range review completed; Godot/.NET compilation, UI layout, sun/sky response and slow-cloud visual acceptance remain local checks.

## Atmosphere controls

F1 → ENVIRONMENT includes separate **Distant horizon haze** and **Volumetric fog / sun shafts** toggles, bound once to the scene's native Environment. Changes happen on input only. Disabling volumetrics turns off the effect. No fog timer or per-frame property writes were added. Both start enabled by the environment scene. The five-case comparison and Baseline/Full buttons preserve atmosphere choices, recording them in the copied report; use the fog toggles manually to isolate atmosphere cost. See ../../WORLD/Environment/README.md for settings and local acceptance. The afternoon sun remains when fog is off. Compilation/runtime validation of this addition is pending.

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

Closed panel: no update/physics callbacks or panel timer. The separately controlled FrameTimeGraph remains active while visible/focused; F2 hides it and stops its sampler/timer. Open: a 4Hz readout and event-driven UI; no debug per-frame callback. Only the bounded comparison enables Process to count frames, disabling it on finish, cancellation and scene removal. No native terrain calls outside the adapter, no per-frame material writes, no per-tree scripts/scans. Normal-map mode swaps between cached material resources so native chunk material instances refresh consistently. Terrain shader LOD transitions are shared through Integration/transvoxel.gdshaderinc in both simple and textured shaders.

## Validation

All 12 project C# scripts compile through Roslyn against .NET 8 reference assemblies and GodotSharp 4.4.1 (zero errors/warnings after correction). This is a focused API/syntax check, **not** the configured Godot.NET.Sdk/4.7.2 source-generator/project build. A direct dotnet CLI build could not run in this environment because process-stat access caused its CLI startup to abort. Godot 4.4.1 isolated headless checks parse both shaders (18 and 1 uniforms) and load the textured material. Paths, load_steps and new loop syntax were checked. No full game/UI run, GPU benchmark, measured performance gain or exact-version runtime compatibility claim.

Local: build terra-machine.csproj in the installed Godot .NET editor. Run test.tscn. Open F1 after Trees:16, test every toggle and the restore button; ensure terrain collision remains when hidden. Click outside the panel and verify the view stays locked; close and verify movement/look/RMB jump return. Run comparison, Copy results, then test F1/Escape cancellation and alt-tab cancellation. Close/reopen and stop/reload the scene. In the profiler, debug Process/timer must stop after close. Send the five copied rows to identify where the frame cost lies before optimizing or increasing tree density.

Grass trial: test.tscn assigns GrassScatter through ExtraVisualRoots. Its visibility toggle also suspends/resumes placement and the tile timer. Visible grass must finish placement/bounded retries before comparison. Grass visibility is held constant through the five cases and recorded in their header; the Baseline/Full buttons do not change this independent toggle. Compare grass on/off manually at the same camera for its cost.

Frame pacing: the test defaults to a 60 FPS cap from DebugMaster.tres, adjustable through F1. The bottom-right FrameTimeGraph can be toggled with F2/F1. See ../FrameTime/README.md. Disable the cap (and consider the existing VSync state) before interpreting raw throughput benchmarks; capped cases may all report 60 FPS despite different rendering costs. Graph visibility is recorded in the comparison header.

Forest trial: `ForestPreview` appears as a separate F1 → OTHER MODELS visibility toggle and is intentionally independent of the original TreePreview controls/comparison. The five-case test holds forest visibility fixed; toggle ForestPreview off/on from the same camera to measure its cost. Pending forest placement can continue if visible during the comparison, so wait for nearby trees to settle before benchmarking.


The natural scene nodes are nested beneath `NatureRoot/Trees`, `NatureRoot/Grass` and `NatureRoot/Rocks`; F1 resolves the new paths. The original tree controls, separate Bermuda toggle, meadow toggle, forest toggle and new boulder toggle remain independent. Hiding boulder visuals doesn't disable their static colliders. Benchmarking should hold those extra toggles fixed except the content being measured.


F1 → OTHER MODELS includes `ForestMeadowScatter` as its own visibility toggle; hiding it also stops its 4 Hz tile timer and placement. Original Bermuda grass retains its dedicated FOLIAGE toggle and 20–80 m thinning/fade, while forest meadow uses separate 13–40–65 m density/fade and native authored near/far mesh LODs. For a fair foliage comparison keep the same camera view and let tile placement settle before measuring.

## Night fill and glow controls — awaiting local validation

F1 → ENVIRONMENT now includes **Night ambient fill**, **Sun bloom / glow**, **Glow strength** and **Glow highlight threshold**. Night fill talks to the environment's event-driven AtmosphereLighting component; the glow controls write native scene-local Environment properties. Night fill blends by sun elevation and preserves daytime lighting; use the time slider to inspect it. The sliders apply on value changes only, have wheel scrolling disabled so the panel can scroll normally, and introduce no sampling/timer.

Glow strength controls GlowIntensity; the native spread/strength setting remains authored at 0.8. Threshold 1.5 targets HDR highlights; whole-image bloom stays zero. The five-case comparison preserves these independent settings and records them in Copy results. Baseline/Full/Restore full scene still affect terrain/tree choices only. Close/reopen preserves adjustments for the run; restart resets them.

Source/property/signal/reference checks only. Build/runtime/UI/night appearance/sun halo and frame-time validation remain local; see WORLD/Environment/README.md. Glow is an added GPU post effect, not a claim of zero FPS cost or a lens-flare implementation.

## EXCAVATION debug controls

F1 includes an EXCAVATION category when the test scene has an ExcavationService. Dig amount defaults to 1.0 with 0.25–6 range; it scales the next scoop's cap depth, with existing radius/reach/cooldown bounds. 1.0 is the new substantially smaller default, not the old large scoop or a volume in m³. Controls update the service on ValueChanged only and do not mutate resources, poll input, or run extra timers. Restart resets the session multiplier. Current intended behaviour/checks: ../../WORLD/Terrain/Excavation/DG1_VALIDATION.md.


## Three-level pine LOD control — 2026-10-30

The F1 FOLIAGE panel now provides two **session-only** split sliders: LOD1→LOD2 (initially 70 m) and new **LOD2→LOD3** (initially 280 m). The pine LOD dropdown adds **Force ultra-distant LOD3**. PineTree002 uses LOD1 0–70, LOD2 70–280 and LOD3 280–600 m. PineTree003 remains two levels at 0–70/70–280 and falls back to its LOD2 under Force LOD3; fir saplings remain unaffected. TreeRenderDiagnostics caches level availability, native range margins and shadow state when each tree is placed; no frame callbacks are introduced. Return to Auto to restore native distance switching. The five-case comparison logs both split distances and override mode. Validate transitions, any canopy alpha popping and scene FPS in Godot; these UI changes were checked at source level only.
