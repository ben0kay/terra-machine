# Terrain excavation

DG1 is implemented in source and awaiting local validation. [Plan](EXCAVATION_IMPLEMENTATION_PLAN.md) and [local checks](DG1_VALIDATION.md) distinguish later phases.

ExcavationService owns request validation and the used Configuration/ExcavationMaster.tres work caps (reach 4 m, radius 1.5 m; absolute safety ceilings 5/1.5 m). It calls optional ITerrainExcavation. Only Integration/VoxelTerrainAdapter.cs knows the native voxel tool, target ray, SDF removal and loaded-area checks. No second streamer, geology model or per-frame service callback.

The first experiment is a 1 m radius rounded subtraction per captured click. Native mesh/collision rebuilding is asynchronous; feedback acknowledges an edit request rather than completion. This is runtime-only testing: edits may disappear after eviction/restart, grass may remain over holes, and exposed walls retain surface shader colours. Hardness/depth/soil-rock rendering are DG2; saving DG3; trap/navigation behaviour DG4. No later phase is automatically started.
