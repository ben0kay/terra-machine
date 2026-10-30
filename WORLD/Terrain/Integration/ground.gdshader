shader_type spatial;
render_mode cull_back;
#include "res://WORLD/Terrain/Integration/transvoxel.gdshaderinc"
// Material textures stay in world space across chunk boundaries and vertical faces.
uniform sampler2D forest_color : source_color, filter_linear_mipmap_anisotropic, repeat_enable;
uniform sampler2D forest_normal : hint_normal, filter_linear_mipmap_anisotropic, repeat_enable;
uniform sampler2D forest_roughness : filter_linear_mipmap_anisotropic, repeat_enable;
uniform sampler2D grass_color : source_color, filter_linear_mipmap_anisotropic, repeat_enable;
uniform sampler2D grass_normal : hint_normal, filter_linear_mipmap_anisotropic, repeat_enable;
uniform sampler2D grass_roughness : filter_linear_mipmap_anisotropic, repeat_enable;
uniform sampler2D rock_color : source_color, filter_linear_mipmap_anisotropic, repeat_enable;
uniform sampler2D rock_normal : hint_normal, filter_linear_mipmap_anisotropic, repeat_enable;
uniform sampler2D rock_roughness : filter_linear_mipmap_anisotropic, repeat_enable;
// Leaf-litter colour blends into the existing soil shading only inside the forest footprint.
uniform sampler2D leaf_color : source_color, filter_linear_mipmap_anisotropic, repeat_enable;
uniform vec2 leaf_area_center = vec2(18.0, 118.0);
uniform vec2 leaf_area_radii = vec2(82.0, 86.0);
uniform float forest_patch_spacing = 0.0;
uniform float forest_patch_radius = 5.0;
uniform int forest_patch_seed = 9876;
uniform float forest_patch_min_up = 0.8660254;
uniform float leaf_coverage : hint_range(0.0, 1.0) = 0.9;
uniform float leaf_tile_meters : hint_range(0.5, 12.0) = 2.5;
// Fixed-resolution initial-surface slope avoids LOD-dependent material thresholds.
uniform sampler2D surface_slope : filter_linear, repeat_enable;
uniform float surface_map_size = 1024.0;
uniform float texture_warp : hint_range(0.0, 1.0) = 0.32;
uniform float tile_size_meters : hint_range(0.5, 12.0) = 2.5;
uniform float patch_size_meters : hint_range(2.0, 80.0) = 18.0;
uniform float grass_coverage : hint_range(0.0, 1.0) = 0.38;
uniform float rock_slope_start : hint_range(0.0, 1.0) = 0.22;
uniform float rock_slope_end : hint_range(0.0, 1.0) = 0.48;
uniform bool normal_maps_enabled = true;
uniform float normal_strength : hint_range(0.0, 1.5) = 0.65;
uniform float macro_variation : hint_range(0.0, 0.4) = 0.12;
varying vec3 world_position;
varying vec3 world_normal;

float hash_cell(vec2 p) {
    uvec2 cell = uvec2(ivec2(p));
    uint h = cell.x * 1597334677u ^ cell.y * 3812015801u;
    h = (h ^ (h >> 16u)) * 2246822519u;
    h = (h ^ (h >> 13u)) * 3266489917u;
    return float(h ^ (h >> 16u)) / 4294967295.0;
}
float patch_noise(vec2 p) {
    vec2 cell = floor(p);
    vec2 f = fract(p);
    f = f * f * (3.0 - 2.0 * f);
    return mix(mix(hash_cell(cell), hash_cell(cell + vec2(1.0, 0.0)), f.x),
        mix(hash_cell(cell + vec2(0.0, 1.0)), hash_cell(cell + vec2(1.0, 1.0)), f.x), f.y);
}
// Continuous low-frequency UV distortion breaks straight repeated rows without extra texture reads.
vec2 ground_uv(vec2 uv) {
    vec2 warp = vec2(patch_noise(uv / 3.7), patch_noise(uv / 3.7 + vec2(29.3, 17.1))) - vec2(0.5);
    return uv + warp * texture_warp * 2.0;
}
vec3 triplanar_color(sampler2D image, vec3 weights, vec2 ux, vec2 uy, vec2 uz) {
    return texture(image, ux).rgb * weights.x + texture(image, uy).rgb * weights.y + texture(image, uz).rgb * weights.z;
}
vec3 unpack_normal(vec3 value) {
    vec2 xy = (value.xy * 2.0 - 1.0) * normal_strength;
    return normalize(vec3(xy, sqrt(max(1.0 - dot(xy, xy), 0.001))));
}
// Whiteout reorientation preserves the actual surface normal for flat normal maps.
vec3 triplanar_normal(sampler2D image, vec3 n, vec3 weights, vec2 ux, vec2 uy, vec2 uz) {
    vec3 x = unpack_normal(texture(image, ux).rgb);
    vec3 y = unpack_normal(texture(image, uy).rgb);
    vec3 z = unpack_normal(texture(image, uz).rgb);
    vec3 nx = vec3(n.x * x.z, n.y + x.y, n.z - x.x);
    vec3 ny = vec3(n.x + y.x, n.y * y.z, n.z - y.y);
    vec3 nz = vec3(n.x + z.x, n.y + z.y, n.z * z.z);
    return normalize(nx * weights.x + ny * weights.y + nz * weights.z);
}
// Matches GrassScatter.PatchRandom / InsidePatch: same cells, uint hash and jitter.
float forest_patch_random(uint value) {
    value ^= value >> 16u; value *= 0x7feb352du;
    value ^= value >> 15u; value *= 0x846ca68bu; value ^= value >> 16u;
    return float(value & 0x00ffffffu) / 16777216.0;
}
float forest_grass_patch(vec2 point) {
    if (forest_patch_spacing <= 0.0) return 0.0;
    float spacing = max(2.0, forest_patch_spacing);
    ivec2 cell = ivec2(floor(point / spacing));
    float edge = -10000.0;
    for (int z = -1; z <= 1; z++) {
        for (int x = -1; x <= 1; x++) {
            ivec2 key = cell + ivec2(x, z);
            uint hash = uint(key.x) * 73856093u ^ uint(key.y) * 19349663u ^ uint(forest_patch_seed);
            vec2 jitter = vec2(forest_patch_random(hash), forest_patch_random(hash + 1u));
            vec2 center = (vec2(key) + vec2(0.15) + jitter * 0.7) * spacing;
            float radius = clamp(forest_patch_radius, 0.2, spacing * 0.45)
                * mix(0.75, 1.2, forest_patch_random(hash + 2u));
            edge = max(edge, radius - distance(point, center));
        }
    }
    return smoothstep(-0.6, 0.4, edge);
}
void vertex() {
    VERTEX = get_transvoxel_position(VERTEX, CUSTOM0);
    world_position = (MODEL_MATRIX * vec4(VERTEX, 1.0)).xyz;
    world_normal = normalize(MODEL_NORMAL_MATRIX * NORMAL);
}
void fragment() {
    vec3 n = normalize(world_normal);
    vec3 weights = pow(abs(n), vec3(4.0));
    weights /= max(weights.x + weights.y + weights.z, 0.0001);
    float surface_up = texture(surface_slope, (world_position.xz + vec2(0.5)) / surface_map_size).r;
    // Soil/grass projections follow the same baked slope across differently tessellated chunks.
    vec3 ground_weights = mix(vec3(0.0, 1.0, 0.0), weights, smoothstep(0.15, 0.5, 1.0 - surface_up));
    vec3 p = world_position / max(tile_size_meters, 0.1);
    // Fixed projection orientation avoids flipping UVs at normal-sign boundaries.
    vec2 ux = ground_uv(vec2(-p.z, p.y));
    vec2 uy = ground_uv(vec2(p.x, -p.z));
    vec2 uz = ground_uv(vec2(p.x, p.y));
    float patch = patch_noise(world_position.xz / max(patch_size_meters, 0.1));
    float grass = smoothstep(1.0 - grass_coverage - 0.16, 1.0 - grass_coverage + 0.16, patch);
    float rock = smoothstep(rock_slope_start, max(rock_slope_start + 0.01, rock_slope_end), 1.0 - surface_up + (patch - 0.5) * 0.08);
    vec2 forest_delta = (world_position.xz - leaf_area_center) / max(leaf_area_radii, vec2(1.0));
    float forest_area = 1.0 - smoothstep(0.98, 1.02, length(forest_delta));
    float grass_patch = forest_grass_patch(world_position.xz)
        * smoothstep(forest_patch_min_up - 0.02, forest_patch_min_up + 0.02, surface_up);
    if (forest_patch_spacing > 0.0) grass = mix(grass, grass_patch, forest_area);
    vec3 fc = triplanar_color(forest_color, ground_weights, ux, uy, uz);
    vec3 gc = triplanar_color(grass_color, ground_weights, ux, uy, uz);
    vec3 rc = triplanar_color(rock_color, weights, ux, uy, uz);
    float macro = patch_noise(world_position.xz / 37.0 + vec2(43.0));
    ALBEDO = mix(mix(fc, gc, grass), rc, rock) * mix(1.0 - macro_variation, 1.0 + macro_variation, macro);
    vec2 leaf_delta = (world_position.xz - leaf_area_center) / max(leaf_area_radii, vec2(1.0));
    float leaf_area = 1.0 - smoothstep(0.65, 1.05, length(leaf_delta));
    float leaf_patch = smoothstep(0.22, 0.7, patch_noise(world_position.xz / 9.0 + vec2(13.7, 8.1)));
    float leaf_blend = leaf_area * (1.0 - grass_patch) * leaf_coverage * mix(0.45, 1.0, leaf_patch)
        * (1.0 - rock) * smoothstep(0.75, 0.95, surface_up);
    if (leaf_blend > 0.001) {
        vec2 leaf_uv = ground_uv(vec2(world_position.x, -world_position.z) / max(leaf_tile_meters, 0.1));
        vec3 litter = texture(leaf_color, leaf_uv).rgb;
        ALBEDO = mix(ALBEDO, litter * mix(1.0 - macro_variation, 1.0 + macro_variation, macro), leaf_blend);
    }
    float fr = triplanar_color(forest_roughness, ground_weights, ux, uy, uz).r;
    float gr = triplanar_color(grass_roughness, ground_weights, ux, uy, uz).r;
    float rr = triplanar_color(rock_roughness, weights, ux, uy, uz).r;
    ROUGHNESS = clamp(mix(mix(fr, gr, grass), rr, rock), 0.35, 1.0);
    if (normal_maps_enabled) {
    vec3 fn = triplanar_normal(forest_normal, n, ground_weights, ux, uy, uz);
    vec3 gn = triplanar_normal(grass_normal, n, ground_weights, ux, uy, uz);
    vec3 rn = triplanar_normal(rock_normal, n, weights, ux, uy, uz);
    vec3 mapped = normalize(mix(mix(fn, gn, grass), rn, rock));
    NORMAL = normalize((VIEW_MATRIX * vec4(mapped, 0.0)).xyz);
    }
}
