# Terrain

`terrain.tscn` exposes `ITerrainService`: availability/status, binding a viewer transform, and a collision-ground query. The concrete implementation and all native dependencies live in `Integration/`. Do not add Voxel Tools method strings/types to player, tools, structures, entities or world composition. Extend game-owned operations only when an approved feature needs them.

The adapter validates native class availability before loading `Integration/voxel_terrain.tscn`. Missing extension produces a clear HUD/error message and leaves the player suspended. Addon generation, meshing, collision and streaming remain native. BindViewer attaches a native viewer to the target so no C# frame callback is needed; adapter exit frees the viewer even though it has an external parent.

`Integration/TerrainMaster.tres` is a native Noise2D generator producing **SDF voxels**, not a replacement heightmap mesh. Initial surface height is derived from 2D noise, but the terrain volume is suitable for later excavation. No edits/persistence are implemented yet. Bounds/LOD/collision are on `Integration/voxel_terrain.tscn`, viewer distance follows its view_distance. Keep geometry bounds aligned to chunk/LOD sizes; changing the finite extent should also update WorldMaster's playable safety bound.

`Integration/ground.gdshader` uses slope/position to color terrain and includes Voxel Tools' MIT Transvoxel transition helpers for LOD seams. Colors do not represent mineable material IDs. Source: https://github.com/Zylann/godot_voxel/blob/master/shaders/dev/transvoxel_minimal.gdshader ; license retained in `addons/zylann.voxel/LICENSE.md`.

Editor terrain preview and live tuning are deferred. Run the test scene to generate terrain. Actual addon/engine compatibility and LOD/collision behavior still require local validation.
