# Terrain excavation

**Documentation foundation only — no excavation code or changes to voxel terrain have been made.**

[Implementation plan](EXCAVATION_IMPLEMENTATION_PLAN.md) specifies phased, isolated work. This directory owns material/depth resistance rules, excavation requests, and future persistence coordination. It must not own the native Voxel Tools implementation: that stays in `WORLD/Terrain/Integration/`, behind a small game-owned interface.

Proposed future modules (create only when implementing): `ExcavationService.cs` (rules and orchestration), `ExcavationRules.cs` (resistance/material decision), `Configuration/ExcavationSettings.cs` and `ExcavationMaster.tres`. The boundary contract may live in `WORLD/Terrain/ITerrainExcavation.cs`. Avoid adding a parallel terrain streamer.
