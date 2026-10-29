shader_type spatial;
render_mode cull_disabled;
// Shared card material: independent alpha mask, no alpha blending/sorting.
uniform sampler2D grass_color : source_color, filter_linear_mipmap_anisotropic;
uniform sampler2D grass_alpha : filter_linear_mipmap_anisotropic;
uniform sampler2D grass_normal : hint_normal, filter_linear_mipmap_anisotropic;
uniform sampler2D grass_roughness : filter_linear_mipmap_anisotropic;
uniform float alpha_cutoff : hint_range(0.05, 0.95) = 0.4;
void fragment() {
    ALBEDO = texture(grass_color, UV).rgb;
    ALPHA = texture(grass_alpha, UV).r;
    ALPHA_SCISSOR_THRESHOLD = alpha_cutoff;
    NORMAL_MAP = texture(grass_normal, UV).rgb;
    ROUGHNESS = clamp(texture(grass_roughness, UV).r, 0.35, 1.0);
}
