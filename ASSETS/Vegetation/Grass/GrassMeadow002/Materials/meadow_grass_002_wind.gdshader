shader_type spatial;
render_mode cull_disabled;
// Lit, cutout foliage. Shared by both MeadowGrass002 placement layers and authored LODs.
uniform sampler2D grass_atlas : source_color, filter_linear_mipmap_anisotropic;
uniform float alpha_cutoff : hint_range(0.05,0.95) = 0.43;
uniform float wind_strength : hint_range(0.0,0.2) = 0.045;
uniform float wind_speed : hint_range(0.0,5.0) = 1.1;
uniform vec2 wind_direction = vec2(0.82,0.57);
uniform bool distance_fade_enabled = false;
uniform float sparse_distance = 44.0;
uniform float end_distance = 68.0;

group_uniforms Foliage;
uniform float plant_height : hint_range(0.1,2.0) = 0.85;
uniform float color_variation : hint_range(0.0,1.0) = 0.35;
uniform vec3 fresh_tint : source_color = vec3(0.94,1.0,0.92);
uniform vec3 dry_tint : source_color = vec3(1.0,0.97,0.86);
uniform float brightness_variation : hint_range(0.0,0.25) = 0.06;
uniform float root_darkening : hint_range(0.0,0.5) = 0.10;
uniform float root_shade_height : hint_range(0.05,0.8) = 0.30;
uniform bool two_sided_lighting = true;
uniform float transmission_strength : hint_range(0.0,0.5) = 0.18;
uniform float surface_roughness : hint_range(0.5,1.0) = 0.90;
group_uniforms;

varying flat float distance_presence;
varying flat vec3 clump_tint;
varying float blade_height;

// Stable world-root seed: no INSTANCE_ID dependence, so LOD switches/batch compaction keep colours.
float clump_random(uvec2 cell, uint salt) {
    uint h = cell.x * 1597334677u ^ cell.y * 3812015801u ^ salt;
    h = (h ^ (h >> 16u)) * 2246822519u;
    h = (h ^ (h >> 13u)) * 3266489917u;
    return float(h ^ (h >> 16u)) / 4294967295.0;
}
void vertex(){
    vec3 root_world = (MODEL_MATRIX * vec4(0.0,0.0,0.0,1.0)).xyz;
    uvec2 seed = uvec2(ivec2(floor(root_world.xz * 100.0)));
    vec3 tint = mix(fresh_tint, dry_tint, clump_random(seed, 17u));
    float value = mix(1.0 - brightness_variation, 1.0 + brightness_variation, clump_random(seed, 71u));
    clump_tint = mix(vec3(1.0), tint, color_variation) * value;
    // Use original local height, independent of distance collapse, wind and placement scale.
    blade_height = clamp(VERTEX.y / max(plant_height,0.01),0.0,1.0);
    float d = distance(root_world.xz,CAMERA_POSITION_WORLD.xz);
    distance_presence = distance_fade_enabled ? 1.0 - smoothstep(sparse_distance,end_distance,d) : 1.0;
    float anchored = smoothstep(0.08,0.72,VERTEX.y);
    float offset = dot(root_world.xz,vec2(0.27,0.71));
    float wave = sin(TIME * wind_speed + offset + VERTEX.y * 1.9)
       + 0.28 * sin(TIME * wind_speed * 0.61 + offset * 1.4);
    VERTEX.xz += wind_direction / max(length(wind_direction),0.001) * wave * wind_strength * anchored * anchored;
    VERTEX.y *= distance_presence;
}
void fragment(){
    if (distance_presence < 0.001) discard;
    vec4 t = texture(grass_atlas,UV);
    float roots = 1.0 - smoothstep(0.0,root_shade_height,blade_height);
    float shade = 1.0 - root_darkening * roots;
    ALBEDO = t.rgb * COLOR.rgb * clump_tint * shade;
    ALPHA = t.a;
    ALPHA_SCISSOR_THRESHOLD = alpha_cutoff;
    vec3 n = normalize(NORMAL);
    // Orient the existing card normal toward its viewed side; avoid a second unconditional flip.
    if (two_sided_lighting && dot(n,VIEW) < 0.0) n = -n;
    NORMAL = n;
    ROUGHNESS = surface_roughness;
    SPECULAR = 0.25;
    METALLIC = 0.0;
    AO = 1.0 - root_darkening * roots * 0.5;
    AO_LIGHT_AFFECT = 0.0;
    // Built-in direct-light backlighting, not emission: darker roots transmit less.
    BACKLIGHT = ALBEDO * transmission_strength * mix(0.35,1.0,blade_height);
}
