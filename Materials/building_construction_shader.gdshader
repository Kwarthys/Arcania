shader_type spatial;
render_mode unshaded, world_vertex_coords;

uniform vec2 startEndHeights;
uniform float completion : hint_range(0, 1);

varying float height;

void vertex()
{
	height = VERTEX.y;
}

void fragment()
{
	float heightThreshold = startEndHeights.x * (1.0 - completion) + startEndHeights.y * completion;
	bool draw = height < heightThreshold;

	if(draw)
	{
		ALBEDO = vec3(0.1, 1.0, 0.5);
		ALPHA = 1.0;
	}
	else
	{
		ALBEDO = vec3(0.2, 0.2, 0.9);
		ALPHA = 0.9;
	}
}
