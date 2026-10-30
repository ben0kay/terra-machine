# Grass Medium 001

Owner-exported Models/grass_medium_01_LOD0/1/2.gltf, paired binaries and shared Textures. Each scene contains 17 named grass variants (large, mid, small, tall and tiny). Preserve names when re-exporting: the runtime pairs matching names by LOD suffix. Keep matching origins/scale; authored display translations are ignored by the scatter.

Materials/ForestGrass.tres remains available as the original GrassMedium001 cutout/GPU wind override, but the active `ForestMeadowScatter` no longer references this asset. It was replaced by the much lighter GrassMeadow002 LOD1/LOD2 clumps to reduce grass geometry and draw overhead. All 17 named variants and three authored LOD exports remain in the project for reference or future selective use; the previous large near meshes reached 6,422 triangles per tuft. The active forest recipe is documented in WORLD/Vegetation/GrassScatter/Configuration/FOREST_MEADOW_README.md.

Source links and 17 matching LOD names checked; actual Godot import, shader compilation, visual quality and FPS await local validation.
