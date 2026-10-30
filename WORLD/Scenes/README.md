# Test world scene

`test.tscn` is the active scene and retains root `Terrain`, `Environment`, `Player`, `TestHUD` and `DebugOverlay`. Natural scenery is now grouped beneath a simple `NatureRoot`:

```text
Test
├─ Terrain
├─ Environment
├─ Player
├─ NatureRoot
│  ├─ Trees
│  │  ├─ TreePreview
│  │  └─ ForestPreview
│  ├─ Grass
│  │  ├─ GrassScatter
│  │  ├─ MeadowPreview
│  │  ├─ ForestMeadowScatter
│  │  └─ SpawnMeadow002
│  └─ Rocks
│     └─ BoulderPreview
├─ TestHUD
└─ DebugOverlay
```

This mirrors the named-grouping approach in War Machine's active `WORLD/World.tscn`, without importing unrelated actors, saving, terrain infrastructure or systems managers. `TestWorld.cs` and F1 debug paths were updated for these fixed nodes. Grouping does not alter transforms, physics ownership, generation, camera/player paths, or visibility except for explicit hierarchy toggles. Use the owning feature README for tuning and local validation.

Player collision now scans terrain bit 1 and static boulders bit 4 (mask 5), while native terrain placement rays stay on bit 1. This prevents new rock colliders from being mistaken for voxel ground by vegetation scatter.

`SpawnMeadow002` is an additional node under NatureRoot/Grass for bounded Meadow Grass 2 bunches near the first twelve authored spawn firs (TreePreview site order), with separate settings and F1 visibility. Its placement does not change the existing forest meadow or other vegetation systems.
