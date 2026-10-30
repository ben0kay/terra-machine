# Terrain excavation

DG1 is implemented in source and awaiting local validation. [Plan](EXCAVATION_IMPLEMENTATION_PLAN.md) and [local checks](DG1_VALIDATION.md) distinguish later phases.

ExcavationService owns request validation and the used Configuration/ExcavationMaster.tres work caps (reach 4 m, radius 1.5 m; absolute safety ceilings 5/1.5 m). It calls optional ITerrainExcavation. Only Integration/VoxelTerrainAdapter.cs knows the native voxel tool, target ray, SDF removal and loaded-area checks. No second streamer, geology model or per-frame service callback.

The first experiment is a 1 m radius rounded subtraction per captured click. Native mesh/collision rebuilding is asynchronous; feedback acknowledges an edit request rather than completion. This is runtime-only testing: edits may disappear after eviction/restart, collidable props remain supported independently, and exposed walls retain surface shader colours. Hardness/depth/soil-rock rendering are DG2; saving DG3; trap/navigation behaviour DG4. No later phase is automatically started.

## Smaller default and debug amount

F1 now has EXCAVATION → Dig amount, 0.25–6.0, default 1.0, applied to future clicks only. It multiplies penetration depth, not brush radius or literal removed volume. Shovel radius stays 1 m for the existing one-metre voxel lattice. SurfaceInset=-0.75 places the sphere centre outside the surface, giving a 0.25 m theoretical cap depth at 1.0. On a flat continuous surface this is ~0.18 m³ versus the prior ~2.56 m³ scoop (about 93% less). Actual voxel removal/appearance can differ, and some sub-voxel shallow clicks may make little/no visible change. At 6.0 depth caps at 1.5 m; cooldown/reach and native loaded bounds remain unchanged. This is no material hardness system.

ITerrainExcavation.Excavated reports the accepted world-space brush sphere. The service forwards it and TestWorld removes ground foliage through each registered scatter owner. No collision-refresh polling is added. WORLD/Vegetation/GroundFoliageEdits compacts affected MultiMesh instances and indexes cut history in 8 m cells so pending/rebuilt grass is also filtered. Root checks include 12 cm grounding tolerance. All native LOD batches use the same roots and removal. No whole-world physics rays, per-plant ticks, zero-scale shader transforms or shader-only invisible draw instances.

Trees, rocks and collidable branches are not removed/repositioned; support physics is separate. Ground cut history is session-local and follows the current DG1 lifetime (terrain persistence remains DG3). If terrain edits are lost on native chunk eviction, foliage cut history can leave bare regenerated soil until restart. Existing placement HUD counts are initial placements rather than remaining live blades. Source/reference/loop/geometry review passed; compilation/native rendering/contact/FPS remain unverified locally.
