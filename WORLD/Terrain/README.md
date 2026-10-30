# Terrain

`terrain.tscn` exposes `ITerrainService`: availability/status, binding a viewer transform, and a collision-ground query. The concrete implementation and all native dependencies live in `Integration/`. Do not add Voxel Tools method strings/types to player, tools, structures, entities or world composition. Extend game-owned operations only when an approved feature needs them.

The adapter validates native class availability before loading `Integration/voxel_terrain.tscn`. Missing extension produces a clear HUD/error message and leaves the player suspended. Addon generation, meshing, collision and streaming remain native. BindViewer attaches a native viewer to the target so no C# frame callback is needed; adapter exit frees the viewer even though it has an external parent.

`Landscape/Configuration/LandscapeMaster.tres` now authors the initial surface: meadow, broad mountain shoulder, plateau/cliff and distant ridge. `Landscape/TestLandscape.cs` prepares height and slope-map images once. `Integration/TerrainMaster.tres` is a native Image generator producing **SDF voxels**, not a replacement heightmap mesh. The adapter supplies a private generator/image before native streaming starts. See `Landscape/README.md` for authoring, performance and validation. DG1 now adds bounded runtime edits; persistence remains unimplemented. Bounds/LOD/collision remain on `Integration/voxel_terrain.tscn`; viewer distance follows its view_distance.

`Configuration/GroundMaterial.tres` now selects three color/normal/roughness texture sets and controls world-space triplanar material blending. `Integration/ground.gdshader` retains Voxel Tools' MIT Transvoxel transition helpers for LOD seams. Colors do not represent mineable material IDs. Source: https://github.com/Zylann/godot_voxel/blob/master/shaders/dev/transvoxel_minimal.gdshader ; license retained in `addons/zylann.voxel/LICENSE.md`.

Editor terrain preview and live tuning are deferred. Run the test scene to generate terrain. Actual addon/engine compatibility and LOD/collision behavior still require local validation.

Render debug is optional through ITerrainRenderDebug. The adapter owns cached simple/textured/normal-disabled materials and native visibility; collision and streaming continue. Transition code is shared in Integration/transvoxel.gdshaderinc. No debug native calls leak into the overlay.

`ITerrainSurfaceService` is an optional game-owned ground-position/normal query used by grass slope filtering. The adapter shares the existing physics ray with TryFindGround; no additional per-frame terrain work or native API leakage is introduced.

## Forest leaf litter — awaiting local validation

GroundMaterial now references the owner's brown_mud_leaves_01 diffuse JPEG. The ground shader softly blends this colour over the existing soil/grass shading inside the actual forest ellipse, with irregular 9 m patches, 2.5 m texture repeat and 0.9 maximum coverage. The edge fades from 65% to 105% of the ellipse radius; steep rock is excluded. Existing soil roughness and normal shading remain shared in this first colour-layer pass. The source EXR normal/roughness imports are marked invalid, so they are not introduced as runtime dependencies. No displacement, decals, floating planes or collision changes are added.

ITerrainForestFloor is an optional presentation contract. TestWorld binds centre/radii from ForestMaster plus world spawn once at initialisation; VoxelTerrainAdapter updates its textured and normal-disabled cached materials. No additional C# frame/timer work is added. The visible leaf layer costs one additional diffuse texture sample where its spatial mask is active; no measured FPS claim. Set leaf_coverage=0 in GroundMaterial to disable. Shader/reference/loop review only; Godot shader compilation, scale, colour, fade edges and FPS need local validation. Moving the forest resource and restarting moves the litter mask as well.

## Ground beneath GrassMedium forest clumps

The forest-floor pass now accepts the scatter's world-anchored patch spacing/radius/seed/slope through SetForestGrassPatches, called once by TestWorld. The shader mirrors GrassScatter's integer hash and 3×3 neighbouring-cell test with a soft metre-wide edge. Inside the forest, existing grass colour/normal/roughness follows the clump footprint; brown leaf colour is suppressed there and kept in open gaps. Outside the forest, the previous global ground blend remains. Baked slope and actual collision slope can differ slightly; individual missing/rejected plants do not alter ground colouring. The layout intentionally persists when grass visibility or distance culling changes. Compilation/shader/FPS validation remains local. Current settings and checks: ../Vegetation/GrassScatter/Configuration/FOREST_MEADOW_README.md.

DG1 exposes optional ITerrainExcavation through this same adapter. ExcavationService validates tool requests; adapter verifies installed native methods/constants, caches one SDF-removal tool, raycasts on a queued physics frame, checks exact terrain ownership/full resident edit bounds and invokes a bounded do_sphere. It supports unscaled transformed terrain via ToLocal, and rejects scaled terrain. Native post-edit rebuilding handles meshes/collision; no edit polling or parallel terrain manager. See Excavation/DG1_VALIDATION.md for local compatibility/contact testing and known visual/persistence limitations.


## Sparse grass and soil mix within the forest (October 2026)

`GroundMaterial.tres` now suppresses the bright global grass-ground colour within the active forest ellipse rather than painting it directly wherever the ForestGrass001 patch mask is present. A new optional colour-only `leafy_grass_diff_1k.jpg` contribution (default 27% maximum, softened by noise/slope and tinted darker) combines with forest soil and a quieter `brown_mud_leaves_01` litter mix (coverage 0.6). The ground remains green-brown but no longer creates hard bright-grass patches under every tuft. The open meadow beyond the forest remains on its original ground shader behaviour; the edge mixes gradually.

The existing `ITerrainForestFloor` calls still bind the ellipse and tuft patch recipe once. Placement itself is unchanged. Only a forest-local single extra sparse texture lookup may be introduced, with no extra C# work. Tunable parameters and validation are described in `Configuration/README.md`.
