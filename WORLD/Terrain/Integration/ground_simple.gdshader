shader_type spatial;
render_mode cull_back;
#include "res://WORLD/Terrain/Integration/transvoxel.gdshaderinc"
// Original untextured baseline; preserve identical terrain geometry and LOD seams.
varying vec3 world_position;
varying vec3 world_normal;
void vertex() {
    VERTEX = get_transvoxel_position(VERTEX, CUSTOM0);
    world_position = (MODEL_MATRIX * vec4(VERTEX, 1.0)).xyz;
    world_normal = normalize(MODEL_NORMAL_MATRIX * NORMAL);
}
void fragment() {
    float steep = 1.0 - smoothstep(0.55, 0.88, normalize(world_normal).y);
    float variation = sin(world_position.x * 0.31) * sin(world_position.z * 0.27);
    vec3 grass = mix(vec3(0.18, 0.24, 0.10), vec3(0.28, 0.32, 0.15), variation * 0.5 + 0.5);
    ALBEDO = mix(grass, vec3(0.30, 0.29, 0.26), steep);
    ROUGHNESS = 0.95;
}
