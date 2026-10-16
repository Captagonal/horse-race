shader_type canvas_item;

uniform sampler2D screen: hint_screen_texture;
uniform float pixel_count = 120;
uniform float color_levels : hint_range(1.0, 64.0) = 20.0;

void fragment() {
    // Use SCREEN_UV for full-screen effects
    vec2 pix_uv = round(SCREEN_UV * pixel_count) / pixel_count;
    vec4 color = texture(screen, pix_uv);
    
    // 1. Get the original brightness
    float old_brightness = dot(color.rgb, vec3(0.299, 0.587, 0.114));
    
    // 2. Quantize the brightness value
    float new_brightness = round(old_brightness * color_levels) / color_levels;
    
    // 3. Put the new brightness back into the color (preserving hue & saturation)
    if (old_brightness > 0.0) {
        color.rgb *= (new_brightness / old_brightness);
    }
    
    COLOR = color;
}