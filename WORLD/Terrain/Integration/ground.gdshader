shader_type spatial;
render_mode cull_back;
// Transvoxel transition helpers adapted from Zylann/godot_voxel (MIT).
// See addons/zylann.voxel/LICENSE.md; keep these inside the integration.
// From Voxel Tools API
uniform int u_transition_mask;

float get_transvoxel_secondary_factor(int idata) {
	int transition_mask = u_transition_mask & 0xff;

	int cell_border_mask = idata & 63; // Which sides the cell is touching
	int vertex_border_mask = (idata >> 8) & 63; // Which sides the vertex is touching
	// If the vertex is near a side where there is a low-resolution neighbor,
	// move it to secondary position
	int m = transition_mask & cell_border_mask;
	float t = float(m != 0);
	// If the vertex lies on one or more sides, and at least one side has no low-resolution neighbor,
	// don't move the vertex.
	t *= float((vertex_border_mask & ~transition_mask) == 0);
	
	// Debugging
	//t *= 0.5 + 0.5 * sin(TIME * 4.0);
	//t *= 2.0;

	return t;
}

vec3 get_transvoxel_position(vec3 vertex_pos, vec4 fdata) {
	int idata = floatBitsToInt(fdata.a);

	// Move vertices to smooth transitions
	float secondary_factor = get_transvoxel_secondary_factor(idata);
	vec3 secondary_position = fdata.xyz;
	vec3 pos = mix(vertex_pos, secondary_position, secondary_factor);

	// If the mesh combines transitions and the vertex belongs to a transition,
	// when that transition isn't active we change the position of the vertices so
	// all triangles will be degenerate and won't be visible.
	// This is an alternative to rendering them separately,
	// which has less draw calls and less mesh resources to create in Godot.
	// Ideally I would tweak the index buffer like LOD does but Godot does not
	// expose anything to use it that way.
	int itransition = (idata >> 16) & 0xff; // Is the vertex on a transition mesh?
	float transition_cull = float(itransition == 0 || (itransition & u_transition_mask) != 0);
	pos *= transition_cull;

	return pos;
}
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
uniform float tile_size_meters : hint_range(0.5, 12.0) = 2.5;
uniform float patch_size_meters : hint_range(2.0, 80.0) = 18.0;
uniform float grass_coverage : hint_range(0.0, 1.0) = 0.38;
uniform float rock_slope_start : hint_range(0.0, 1.0) = 0.22;
uniform float rock_slope_end : hint_range(0.0, 1.0) = 0.48;
uniform float normal_strength : hint_range(0.0, 1.5) = 0.65;
uniform float macro_variation : hint_range(0.0, 0.4) = 0.12;
varying vec3 world_position;
varying vec3 world_normal;

float hash_cell(vec2 p) {
    return fract(sin(dot(p, vec2(127.1, 311.7))) * 43758.5453);
}
float patch_noise(vec2 p) {
    vec2 cell = floor(p);
    vec2 f = fract(p);
    f = f * f * (3.0 - 2.0 * f);
    return mix(mix(hash_cell(cell), hash_cell(cell + vec2(1.0, 0.0)), f.x),
        mix(hash_cell(cell + vec2(0.0, 1.0)), hash_cell(cell + vec2(1.0, 1.0)), f.x), f.y);
}
vec3 triplanar_color(sampler2D image, vec3 weights, vec2 ux, vec2 uy, vec2 uz) {
    return texture(image, ux).rgb * weights.x + texture(image, uy).rgb * weights.y + texture(image, uz).rgb * weights.z;
}
vec3 unpack_normal(vec3 value) {
    vec2 xy = (value.xy * 2.0 - 1.0) * normal_strength;
    return normalize(vec3(xy, sqrt(max(1.0 - dot(xy, xy), 0.001))));
}
// Whiteout reorientation preserves the actual surface normal for flat normal maps.
vec3 triplanar_normal(sampler2D image, vec3 n, vec3 signs, vec3 weights, vec2 ux, vec2 uy, vec2 uz) {
    vec3 x = unpack_normal(texture(image, ux).rgb);
    vec3 y = unpack_normal(texture(image, uy).rgb);
    vec3 z = unpack_normal(texture(image, uz).rgb);
    vec3 nx = vec3(n.x * x.z, n.y + x.y, n.z - x.x * signs.x);
    vec3 ny = vec3(n.x + y.x, n.y * y.z, n.z - y.y * signs.y);
    vec3 nz = vec3(n.x + z.x * signs.z, n.y + z.y, n.z * z.z);
    return normalize(nx * weights.x + ny * weights.y + nz * weights.z);
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
    vec3 signs = vec3(n.x < 0.0 ? -1.0 : 1.0, n.y < 0.0 ? -1.0 : 1.0, n.z < 0.0 ? -1.0 : 1.0);
    vec3 p = world_position / max(tile_size_meters, 0.1);
    vec2 ux = vec2(-p.z * signs.x, p.y);
    vec2 uy = vec2(p.x, -p.z * signs.y);
    vec2 uz = vec2(p.x * signs.z, p.y);
    float patch = patch_noise(world_position.xz / max(patch_size_meters, 0.1));
    float grass = smoothstep(1.0 - grass_coverage - 0.16, 1.0 - grass_coverage + 0.16, patch);
    float rock = smoothstep(rock_slope_start, max(rock_slope_start + 0.01, rock_slope_end), 1.0 - max(n.y, 0.0));
    vec3 fc = triplanar_color(forest_color, weights, ux, uy, uz);
    vec3 gc = triplanar_color(grass_color, weights, ux, uy, uz);
    vec3 rc = triplanar_color(rock_color, weights, ux, uy, uz);
    float macro = patch_noise(world_position.xz / 37.0 + vec2(43.0));
    ALBEDO = mix(mix(fc, gc, grass), rc, rock) * mix(1.0 - macro_variation, 1.0 + macro_variation, macro);
    float fr = triplanar_color(forest_roughness, weights, ux, uy, uz).r;
    float gr = triplanar_color(grass_roughness, weights, ux, uy, uz).r;
    float rr = triplanar_color(rock_roughness, weights, ux, uy, uz).r;
    ROUGHNESS = clamp(mix(mix(fr, gr, grass), rr, rock), 0.35, 1.0);
    vec3 fn = triplanar_normal(forest_normal, n, signs, weights, ux, uy, uz);
    vec3 gn = triplanar_normal(grass_normal, n, signs, weights, ux, uy, uz);
    vec3 rn = triplanar_normal(rock_normal, n, signs, weights, ux, uy, uz);
    vec3 mapped = normalize(mix(mix(fn, gn, grass), rn, rock));
    NORMAL = normalize((VIEW_MATRIX * vec4(mapped, 0.0)).xyz);
}
