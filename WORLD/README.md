# WORLD

`Scenes/test.tscn` assembles the reusable player, terrain and environment scenes. `Scenes/TestWorld.cs` owns only composition and test safety. Player/Terrain NodePaths are explicit, not scene-tree group scans. A 0.1s collision probe runs only during startup/recovery and stops after success or a 60s timeout. HUD updates through a 0.25s timer. Steady state retains only a cheap player bounds check per physics tick; no continuous ground raycasts.

Configuration: `Configuration/WorldMaster.tres` owns spawn/loading/safety. Terrain generation and streaming are owned separately under Terrain. Environment lighting is owned under Environment. No world population, persistence, vegetation or advanced gameplay in this slice.

To rearrange scenes, retain/update the exported PlayerPath and TerrainPath. `ITerrainService` is the composition boundary; do not reach into the adapter's native children. See `FIRST_PASS.md` for the acceptance handoff.

Ground-and-fir visual slice: see VISUAL_TEST_PASS.md. TreePreview is an optional composed scene; it starts only after first grounded player spawn and sleeps after bounded initial placement. No new streaming population system.
