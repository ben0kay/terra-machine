# Grass Medium 001

Owner-exported Models/grass_medium_01_LOD0/1/2.gltf, paired binaries and shared Textures. Each scene contains 17 named grass variants (large, mid, small, tall and tiny). Preserve names when re-exporting: the runtime pairs matching names by LOD suffix. Keep matching origins/scale; authored display translations are ignored by the scatter.

Materials/ForestGrass.tres is the active shared cutout/GPU wind override. Forest clump composition, scales and native distance ranges are in WORLD/Vegetation/GrassScatter/Configuration/ForestMeadowMaster.tres. Nine large/mid/tall variants are currently selected, with LOD0/1/2 at ~0–14/14–32/32–70 m and 3–4 size variation. Source plants are small (large ~14 cm high, tall ~22–32 cm before scale). Near large variants can reach 6,422 triangles per tuft, so local frame-time/draw/primitive profiling matters before raising density.

Source links and 17 matching LOD names checked; actual Godot import, shader compilation, visual quality and FPS await local validation.
