shader_type spatial;
render_mode cull_disabled;
#include "res://WORLD/Vegetation/Wind/vegetation_wind.gdshaderinc"
// Optional GPU wind for tuft geometry; use as material override, not per-blade C# animation.
uniform sampler2D grass_atlas : source_color, filter_linear_mipmap_anisotropic;
uniform float alpha_cutoff : hint_range(0.1,0.9) = 0.42;
uniform float wind_response = 1.0;
uniform float wind_plant_height = 0.22;
// Forest batches opt in; original fixed meadow around trees keeps its existing look.
uniform bool distance_fade_enabled = false;
uniform float sparse_distance = 42.0;
uniform float end_distance = 62.0;
varying flat float distance_presence;
void vertex() {
    vec3 root_world = (MODEL_MATRIX * vec4(0.0,0.0,0.0,1.0)).xyz;
    float d = distance(root_world.xz, CAMERA_POSITION_WORLD.xz);
    distance_presence = distance_fade_enabled ? 1.0 - smoothstep(sparse_distance, end_distance, d) : 1.0;
    VERTEX = vegetation_bend(VERTEX, MODEL_MATRIX, TIME, wind_plant_height, wind_response);
    VERTEX.y *= distance_presence;
}
void fragment() {
    if (distance_presence < 0.001) discard;
    vec4 tex = texture(grass_atlas,UV);
    ALBEDO = tex.rgb * COLOR.rgb;
    ALPHA = tex.a;
    ALPHA_SCISSOR_THRESHOLD = alpha_cutoff;
    ROUGHNESS = 0.91;
    METALLIC = 0.0;
}
