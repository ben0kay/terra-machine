# Terrain

`terrain.tscn` exposes `ITerrainService`: availability/status, binding a viewer transform, and a collision-ground query. The concrete implementation and all native dependencies live in `Integration/`. Do not add Voxel Tools method strings/types to player, tools, structures, entities or world composition. Extend game-owned operations only when an approved feature needs them.

The adapter validates native class availability before loading `Integration/voxel_terrain.tscn`. Missing extension produces a clear HUD/error message and leaves the player suspended. Addon generation, meshing, collision and streaming remain native. BindViewer attaches a native viewer to the target so no C# frame callback is needed; adapter exit frees the viewer even though it has an external parent.

`Landscape/Configuration/LandscapeMaster.tres` now authors the initial surface: meadow, curved uphill bench, plateau/cliff and distant ridge. `Landscape/TestLandscape.cs` prepares height and route-mask images once. `Integration/TerrainMaster.tres` is a native Image generator producing **SDF voxels**, not a replacement heightmap mesh. The adapter supplies a private generator/image before native streaming starts. See `Landscape/README.md` for authoring, performance and validation. No edits/persistence are implemented yet. Bounds/LOD/collision remain on `Integration/voxel_terrain.tscn`; viewer distance follows its view_distance.

`Configuration/GroundMaterial.tres` now selects three color/normal/roughness texture sets and controls world-space triplanar material blending. `Integration/ground.gdshader` retains Voxel Tools' MIT Transvoxel transition helpers for LOD seams. Colors do not represent mineable material IDs. Source: https://github.com/Zylann/godot_voxel/blob/master/shaders/dev/transvoxel_minimal.gdshader ; license retained in `addons/zylann.voxel/LICENSE.md`.

Editor terrain preview and live tuning are deferred. Run the test scene to generate terrain. Actual addon/engine compatibility and LOD/collision behavior still require local validation.

Render debug is optional through ITerrainRenderDebug. The adapter owns cached simple/textured/normal-disabled materials and native visibility; collision and streaming continue. Transition code is shared in Integration/transvoxel.gdshaderinc. No debug native calls leak into the overlay.
