shader_type spatial;
render_mode cull_disabled;
// Shared card material: independent alpha mask, no alpha blending/sorting.
uniform sampler2D grass_color : source_color, filter_linear_mipmap_anisotropic;
uniform sampler2D grass_alpha : filter_linear_mipmap_anisotropic;
uniform sampler2D grass_normal : hint_normal, filter_linear_mipmap_anisotropic;
uniform sampler2D grass_roughness : filter_linear_mipmap_anisotropic;
uniform float alpha_cutoff : hint_range(0.05, 0.95) = 0.4;
uniform float sparse_distance = 45.0;
uniform float end_distance = 60.0;
varying flat float grass_presence;
void vertex() {
    vec3 root = MODEL_MATRIX[3].xyz;
    float distance_to_camera = length(root.xz - CAMERA_POSITION_WORLD.xz);
    grass_presence = 1.0 - smoothstep(sparse_distance, end_distance, distance_to_camera);
    VERTEX.y *= grass_presence;
}
void fragment() {
    if (grass_presence < 0.001) discard;
    ALBEDO = texture(grass_color, UV).rgb;
    ALPHA = texture(grass_alpha, UV).r;
    ALPHA_SCISSOR_THRESHOLD = alpha_cutoff;
    NORMAL_MAP = texture(grass_normal, UV).rgb;
    ROUGHNESS = clamp(texture(grass_roughness, UV).r, 0.35, 1.0);
}
