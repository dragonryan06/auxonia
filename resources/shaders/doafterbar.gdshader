shader_type spatial;
render_mode unshaded;

uniform float percentage = 0.0;
uniform sampler2D gradient;

void vertex() {
	// Billboard
    mat4 billboarded_model_view = VIEW_MATRIX * mat4(
        INV_VIEW_MATRIX[0],
        INV_VIEW_MATRIX[1],
        INV_VIEW_MATRIX[2],
        MODEL_MATRIX[3]
    );
    MODELVIEW_MATRIX = billboarded_model_view;
}

void fragment() {
	ALBEDO = vec3(0.025,0.025,0.025);
	if (UV.x <= percentage) {
		ALBEDO = texture(gradient, vec2(percentage, percentage)).rgb;
	}
}
