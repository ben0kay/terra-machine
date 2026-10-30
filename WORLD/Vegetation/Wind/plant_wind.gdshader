shader_type spatial;
render_mode cull_disabled;
#include "res://WORLD/Vegetation/Wind/vegetation_wind.gdshaderinc"
uniform sampler2D plant_color : source_color, filter_linear_mipmap_anisotropic;
uniform sampler2D plant_normal : hint_normal, filter_linear_mipmap_anisotropic;
uniform sampler2D plant_roughness : filter_linear_mipmap_anisotropic;
uniform float alpha_cutoff = 0.4;
uniform float wind_response = 0.7;
uniform float wind_plant_height = 0.35;

void vertex() {
    VERTEX = vegetation_bend(VERTEX, MODEL_MATRIX, TIME, wind_plant_height, wind_response);
}
void fragment() {
    vec4 color = texture(plant_color, UV);
    ALBEDO = color.rgb * COLOR.rgb;
    ALPHA = color.a;
    ALPHA_SCISSOR_THRESHOLD = alpha_cutoff;
    NORMAL_MAP = texture(plant_normal, UV).rgb;
    ROUGHNESS = clamp(texture(plant_roughness, UV).r, 0.35, 1.0);
    METALLIC = 0.0;
}
