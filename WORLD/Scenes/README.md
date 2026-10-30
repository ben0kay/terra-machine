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
│  │  └─ MeadowPreview
│  └─ Rocks
│     └─ BoulderPreview
├─ TestHUD
└─ DebugOverlay
```

This mirrors the named-grouping approach in War Machine's active `WORLD/World.tscn`, without importing unrelated actors, saving, terrain infrastructure or systems managers. `TestWorld.cs` and F1 debug paths were updated for these fixed nodes. Grouping does not alter transforms, physics ownership, generation, camera/player paths, or visibility except for explicit hierarchy toggles. Use the owning feature README for tuning and local validation.
