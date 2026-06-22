shader_type spatial;
render_mode diffuse_toon, specular_disabled;

uniform vec3 albedo : source_color = vec3(0.8, 0.8, 0.8);

void fragment() {
	ALBEDO = albedo;
	METALLIC = 0.0;
	ROUGHNESS = 1.0;
}
