shader_type spatial;
render_mode cull_disabled;
// Optional material for dense medium grass. No per-instance scripts.
uniform sampler2D grass_atlas : source_color, filter_linear_mipmap_anisotropic;
uniform float alpha_cutoff : hint_range(0.05,0.95) = 0.43;
uniform float wind_strength : hint_range(0.0,0.2) = 0.045;
uniform float wind_speed : hint_range(0.0,5.0) = 1.1;
uniform vec2 wind_direction = vec2(0.82,0.57);
// Forest scatter opts in; the original spawn-area material keeps distance fading disabled.
uniform bool distance_fade_enabled = false;
uniform float sparse_distance = 44.0;
uniform float end_distance = 68.0;
// Optional restrained, light-dependent transmission. 0 = geometry-only lighting baseline.
uniform float foliage_backlight_strength : hint_range(0.0,0.35) = 0.12;
varying flat float distance_presence;
void vertex(){
    vec3 root_world = (MODEL_MATRIX * vec4(0.0,0.0,0.0,1.0)).xyz;
    float d = distance(root_world.xz,CAMERA_POSITION_WORLD.xz);
    distance_presence = distance_fade_enabled ? 1.0 - smoothstep(sparse_distance,end_distance,d) : 1.0;
    float anchored = smoothstep(0.08,0.72,VERTEX.y);
    float offset = dot(root_world.xz,vec2(0.27,0.71));
    float wave = sin(TIME * wind_speed + offset + VERTEX.y * 1.9)
       + 0.28 * sin(TIME * wind_speed * 0.61 + offset * 1.4);
    VERTEX.xz += normalize(wind_direction) * wave * wind_strength * anchored * anchored;
    VERTEX.y *= distance_presence;
}
void fragment(){
    if (distance_presence < 0.001) discard;
    vec4 t = texture(grass_atlas,UV);
    ALBEDO = t.rgb * COLOR.rgb;
    ALPHA = t.a;
    ALPHA_SCISSOR_THRESHOLD = alpha_cutoff;
    ROUGHNESS = 0.9;
    METALLIC = 0.0;
    // Godot BACKLIGHT receives direct lighting; not self-illumination.
    // Preserve two-sided rendering without manually flipping NORMAL again.
    BACKLIGHT = ALBEDO * foliage_backlight_strength;
}
