shader_type spatial;

group_uniforms Colors;
uniform vec3 grass_color : source_color = vec3(0.25, 0.55, 0.2);
uniform vec3 dirt_color : source_color = vec3(0.35, 0.25, 0.18);

group_uniforms Grass_Settings;
uniform float noise_scale = 5.0;
uniform float grass_depth = 0.15; // Fraction of block height for the grass cap (0.0 to 1.0)
uniform float edge_roughness = 0.05;

float hash12(vec2 p) {
    vec3 p3 = fract(vec3(p.xyx) * .1031);
    p3 += dot(p3, p3.yzx + 33.33);
    return fract((p3.x + p3.y) * p3.z);
}

float noise(vec2 p) {
    vec2 i = floor(p);
    vec2 f = fract(p);
    f = f * f * (3.0 - 2.0 * f);
    return mix(mix(hash12(i + vec2(0.0, 0.0)), hash12(i + vec2(1.0, 0.0)), f.x),
               mix(hash12(i + vec2(0.0, 1.0)), hash12(i + vec2(1.0, 1.0)), f.x), f.y);
}

varying vec3 v_world_pos;
varying vec3 v_local_pos;
varying vec3 v_local_normal;

void vertex() {
    v_world_pos = (MODEL_MATRIX * vec4(VERTEX, 1.0)).xyz;
    v_local_pos = VERTEX;
    v_local_normal = NORMAL;
}

void fragment() {
    // 1. Calculate depth along the block's actual tilted height axis (handles rotation & scaling)
    vec3 block_center = MODEL_MATRIX[3].xyz;
    vec3 block_up_vector = MODEL_MATRIX[1].xyz;
    float block_scale_y = length(block_up_vector);
    vec3 local_up_dir = block_up_vector / max(block_scale_y, 0.001);
    
    // Find the exact center of the top face in world space, even when tilted
    vec3 block_top_center = block_center + 0.5 * block_up_vector;
    
    // Measure distance down from that tilted top face
    float depth_from_top = dot(block_top_center - v_world_pos, local_up_dir);
    float normalized_depth = depth_from_top / max(block_scale_y, 0.001);

    // 2. Project noise onto local faces so drips rotate right along with the block
    vec3 abs_normal = abs(v_local_normal);
    vec2 noise_uv;
    
    if (abs_normal.y > 0.5) {
        noise_uv = v_local_pos.xz;
    } else if (abs_normal.x > 0.5) {
        noise_uv = v_local_pos.zy;
    } else {
        noise_uv = v_local_pos.xy;
    }

    // Add local noise for organic edges
    float n = noise(noise_uv * noise_scale);
    float noisy_depth = normalized_depth + (n - 0.5) * edge_roughness;

    // Check if pixel falls within the grass depth
    float is_grass = step(noisy_depth, grass_depth);

    ALBEDO = mix(dirt_color, grass_color, is_grass);
}