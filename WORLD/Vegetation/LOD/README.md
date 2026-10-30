# Authored tree distance LOD

`AuthoredTreeLod.cs` is a one-time setup node for **two or optionally three** authored imported tree scenes. PineTree002 now uses three levels; PineTree003 retains two.

## Active visual ranges

| Asset | LOD1 | LOD2 | LOD3 | Final cull |
|---|---|---|---|---|
| PineTree002 | 0–70 m | 70–280 m | **280–600 m** | 600 m |
| PineTree003 | 0–70 m | 70–280 m | None | 280 m |

PineTree002 LOD3 is the new **120-triangle** coarse foliage-card model, with its own 1K macro atlas; see `ASSETS/Vegetation/Trees/PineTree002/README_LOD3.md`. No other tree asset or instance count changed.

`AuthoredTreeLod` instantiates the available models once and configures native `GeometryInstance3D` visibility ranges. All three models share a root-space union AABB and pivot, with 3 m native hysteresis margins; fades are disabled to preserve cutout rendering. LOD3 shadows default to **off**. If a specified third model contains no meshes, it is hidden and LOD2 covers the remaining distance with a warning.

The imported assets remain in memory after instantiation: **visual LOD changes which mesh draws, not which scene is streamed from disk**. There are no per-frame tree scripts, distance checks, timers or material/mesh regeneration. The shared trunk collider is outside the LOD visual roots and never changes during a switch.

`DistantModel` and `DistantSwitchDistance` are optional exports, so older 2-level pines retain their settings and behaviour. For PineTree002, the scene's `CullDistance` is now 600 m instead of 280 m.

## F1 debugging

F1 → FOLIAGE has LOD1→LOD2 (5–140 m, default 70), new **LOD2→LOD3** (160–550 m, default 280), and Auto/Force LOD1/Force LOD2/**Force LOD3**. Both the original authored pines and the forest use these controls, including newly placed trees. Forced LOD3 selects LOD2 on a 2-level PineTree003 as a safety fallback. Fir saplings are unchanged. The second slider only affects 3-level trees; all forced modes retain the per-asset final cull. Auto restores the range-based selection while preserving original shadow policy.

Debug slider values are session-only and do not rewrite the scene resources. Comparison reports record both split distances and the override mode.

## Validation

Source/scene references and code branch structure reviewed. Godot/.NET compilation and in-game native LOD transitions were **not run** here. After pulling, inspect F1 forced LOD3 at close range; switch back to Auto, view PineTree002 from roughly 70, 280, 400 and 600 metres, and verify colour, alpha coverage, silhouette, cull transitions and tree root alignment. Native ranges use the mesh AABB centre rather than trunk distance, and hysteresis slightly shifts crossings. Check LOD3 shadows remain disabled and PineTree003 still appears in every forced mode. Compare fixed-camera F2 frame times with cap off; no FPS improvement is claimed until measured.

Reference: https://docs.godotengine.org/en/stable/tutorials/3d/visibility_ranges.html
