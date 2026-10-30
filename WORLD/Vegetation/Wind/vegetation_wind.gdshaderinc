// Shared world-space breeze. Root height is local mesh height; response varies by plant.
global uniform vec2 vegetation_wind_direction;
global uniform float vegetation_wind_strength;
global uniform float vegetation_wind_speed;
global uniform float vegetation_wind_gusts;

vec3 vegetation_bend(vec3 vertex, mat4 model, float time, float plant_height, float response) {
    if (vegetation_wind_strength <= 0.0 || response <= 0.0) return vertex;
    vec2 direction = vegetation_wind_direction / max(length(vegetation_wind_direction), 0.001);
    vec3 root = model[3].xyz;
    float rooted = smoothstep(0.04, 1.0, max(vertex.y, 0.0) / max(plant_height, 0.01));
    float phase = dot(root.xz, vec2(0.31, 0.73));
    float t = time * vegetation_wind_speed;
    float gust = 1.0 + vegetation_wind_gusts * sin(t * 0.37 + dot(root.xz, vec2(0.035, 0.02)));
    float wave = sin(t + phase) + 0.3 * sin(t * 0.63 + phase * 1.2);
    vec2 bend = direction * wave * gust * vegetation_wind_strength * response * rooted * rooted;
    // Convert world displacement to local space so random yaw/scale doesn't rotate the breeze.
    return vertex + inverse(mat3(model)) * vec3(bend.x, 0.0, bend.y);
}
