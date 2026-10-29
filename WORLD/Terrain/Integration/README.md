# Native terrain integration

Only this folder owns Voxel Tools types and dynamic property/method calls. `VoxelTerrainAdapter.cs` validates the addon, prepares the game-owned test landscape once, duplicates the image generator, sets its image before entering the scene tree, and exposes stable ground/viewer/debug contracts. `TerrainMaster.tres` supplies SDF channel 1 and the -32..192 m height encoding used by `../Landscape/TestLandscape.cs`.

`voxel_terrain.tscn` owns native meshing, bounds, LOD and collision. Shaders retain the shared Transvoxel transition helpers. Textured and normal-disabled debug materials share the same private route mask; debug swaps never regenerate terrain. See the parent and Landscape READMEs for configuration and validation limitations.
