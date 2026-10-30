# Ground material configuration

Edit `GroundMaterial.tres` shader parameters: tile_size_meters (2.5m starting repeat), patch_size_meters, grass_coverage, rock_slope_start/end, normal_strength and macro_variation. Color/normal/roughness maps use three existing asset sets: forest_ground_06, grass_ground and gray_rocks. Texture repeat scale is a tunable visual starting point, not a claim of exact source capture dimensions.

The material is shared by native terrain chunks. World-space triplanar mapping prevents UV stretching on steep faces and preserves coordinates across chunks. Broad deterministic grass patches mix with forest floor; slope controls exposed rock. Triplanar whiteout normal mapping retains the geometric normal at zero normal strength. Roughness is sampled linearly, color in sRGB, normals in OpenGL convention. Transvoxel transition handling remains inside the integration shader.

The shader samples three layers with three projection axes for color/normal/roughness: 27 texture reads per shaded fragment. No per-frame C# material updates. This is a quality baseline, not a profiled final terrain renderer; packing/branching and texture-sharing improvements can follow measured need. No displacement, geometry replacement, terrain editing, biome/material voxel channels or new terrain generation is added.

Six EXR source maps use compression rejected by Godot 4.4.1 in the headless check (Unknown compression type). Runtime PNG conversions live in `ASSETS/Terrain/Runtime/`; original sources remain unchanged. See that folder's README. Import recipes enable mipmaps and VRAM compression; normal blue is reconstructed from RG in the shader.


## Muted forest ground / sparse grassy soil (October 2026)

The forest no longer replaces soil with the bright open-meadow `GrassGround` texture at every `ForestGrass001` patch location. The shader preserves the open-meadow mix outside the forest, but fades out the bright ground-grass contribution from 78% to 104% of the forest ellipse radii. Inside, it blends the existing `ForestGround06` soil with a small, tinted portion of `leafy_grass_diff_1k.jpg` (the nearest available sparse/leafy grass texture in ASSETS/Terrain), and overlays a reduced amount of `brown_mud_leaves_01` for varied brown woodland soil. The muted sparse-grass contribution varies smoothly over the forest instead of drawing hard bright islands under individual tufts.

Tune `GroundMaterial.tres`:
- `forest_sparse_grass_mix=0.27` for sparse green flecks; set to 0 for brown ground, increase gently for more grass.
- `forest_sparse_grass_tint=Color(0.74,0.78,0.71,1)` controls the desaturated/darkened look.
- `leaf_coverage=0.6` keeps muddy leaf litter visible without covering everything.

`GrassScatter` and the scatter's actual patches, heights, LODs, density, and scene positions are untouched. Its patch mask now only nudges sparse-grass tint slightly and leaves some mud/litter directly beneath clumps. Shader uses one additional diffuse texture read where forest sparse grass is visible; no CPU loops, new terrain physics or per-frame writes. Normal/roughness maps continue to use the existing forest set under the forest; the sparse texture is colour-only in this first pass.

Check daytime and dusk in Godot, including edges of the forest ellipse, and verify soil/green blending on different slopes and relative to the plants; use the F1 terrain texturing modes to check without normal-map noise. No Godot renderer run or FPS measurements were performed here.
