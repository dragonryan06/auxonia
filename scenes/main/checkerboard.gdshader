shader_type spatial;

uniform int DIMENSIONS = 1024;
uniform int MINOR_CELL_SIZE = 4;
uniform int MAJOR_CELL_SIZE = 64;
uniform vec3 MAJOR_CELL_COLOR = vec3(0.1,0.5,1.0);

void fragment() {
	ivec2 smallpos = ivec2(UV*float(DIMENSIONS/MINOR_CELL_SIZE));
	if (smallpos.x%2==0) {
		if (smallpos.y%2==0) {
			ALBEDO.rgb *= 0.05;
		}
	} else {
		if ((smallpos.y-1)%2==0) {
			ALBEDO.rgb *= 0.05;
		}
	}
	ivec2 bigpos = ivec2(UV*float(DIMENSIONS/MAJOR_CELL_SIZE));
	if (bigpos.x%2==0) {
		if (bigpos.y%2==0) {
			ALBEDO.rgb *= MAJOR_CELL_COLOR;
		}
	} else {
		if ((bigpos.y-1)%2==0) {
			ALBEDO.rgb *= MAJOR_CELL_COLOR;
		}
	}
}
