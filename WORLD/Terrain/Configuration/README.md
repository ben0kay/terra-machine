# Ground material configuration

Edit `GroundMaterial.tres` shader parameters: tile_size_meters (2.5m starting repeat), patch_size_meters, grass_coverage, rock_slope_start/end, normal_strength and macro_variation. Color/normal/roughness maps use three existing asset sets: forest_ground_06, grass_ground and gray_rocks. Texture repeat scale is a tunable visual starting point, not a claim of exact source capture dimensions.

The material is shared by native terrain chunks. World-space triplanar mapping prevents UV stretching on steep faces and preserves coordinates across chunks. Broad deterministic grass patches mix with forest floor; slope controls exposed rock. Triplanar whiteout normal mapping retains the geometric normal at zero normal strength. Roughness is sampled linearly, color in sRGB, normals in OpenGL convention. Transvoxel transition handling remains inside the integration shader.

The shader samples three layers with three projection axes for color/normal/roughness: 27 texture reads per shaded fragment. No per-frame C# material updates. This is a quality baseline, not a profiled final terrain renderer; packing/branching and texture-sharing improvements can follow measured need. No displacement, geometry replacement, terrain editing, biome/material voxel channels or new terrain generation is added.

Six EXR source maps use compression rejected by Godot 4.4.1 in the headless check (Unknown compression type). Runtime PNG conversions live in `ASSETS/Terrain/Runtime/`; original sources remain unchanged. See that folder's README. Import recipes enable mipmaps and VRAM compression; normal blue is reconstructed from RG in the shader.
