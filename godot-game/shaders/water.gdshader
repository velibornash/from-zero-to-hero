shader_type spatial;
render_mode blend_mix, specular_schlick_ggx;

uniform vec4 shallow_color : source_color = vec4(0.0, 0.5, 0.7, 0.8);
uniform vec4 deep_color : source_color = vec4(0.0, 0.15, 0.35, 0.9);
uniform float wave_speed = 0.5;
uniform float wave_height = 0.08;

void vertex() {
	float t = TIME * wave_speed;
	float wave1 = sin(VERTEX.x * 1.8 + t) * wave_height;
	float wave2 = cos(VERTEX.z * 2.3 + t * 1.3) * wave_height * 0.7;
	float wave3 = sin((VERTEX.x + VERTEX.z) * 1.2 + t * 0.9) * wave_height * 0.5;
	VERTEX.y += wave1 + wave2 + wave3;
}

void fragment() {
	float blend = abs(NORMAL.y);
	ALBEDO = mix(deep_color.rgb, shallow_color.rgb, blend);
	ALPHA = mix(deep_color.a, shallow_color.a, blend);
	METALLIC = 0.0;
	ROUGHNESS = 0.2;
	SPECULAR = 0.6;
}
