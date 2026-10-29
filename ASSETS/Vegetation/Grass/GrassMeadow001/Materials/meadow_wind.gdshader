shader_type spatial;
render_mode cull_disabled;
// Optional GPU wind for tuft geometry; use as material override, not per-blade C# animation.
uniform sampler2D grass_atlas : source_color, filter_linear_mipmap_anisotropic;
uniform float alpha_cutoff : hint_range(0.1,0.9) = 0.42;
uniform float wind_strength : hint_range(0.0,0.2) = 0.065;
uniform float wind_speed : hint_range(0.0,5.0) = 1.25;
uniform vec2 wind_direction = vec2(0.83,0.55);
void vertex() {
    vec3 root_world = (MODEL_MATRIX * vec4(0.0,0.0,0.0,1.0)).xyz;
    float rooted = smoothstep(0.06,0.86,VERTEX.y);
    float phase = dot(root_world.xz,vec2(0.31,0.73));
    float wind = sin(TIME*wind_speed + phase + VERTEX.y*2.1)
               + 0.38*sin(TIME*wind_speed*0.63 + phase*1.2);
    VERTEX.xz += normalize(wind_direction)*wind*wind_strength*rooted*rooted;
}
void fragment() {
    vec4 tex = texture(grass_atlas,UV);
    ALBEDO = tex.rgb * COLOR.rgb;
    ALPHA = tex.a;
    ALPHA_SCISSOR_THRESHOLD = alpha_cutoff;
    ROUGHNESS = 0.91;
    METALLIC = 0.0;
}
