shader_type canvas_item;

uniform int[16384] pixels;

void vertex() {
	// Called for every vertex the material is visible on.
}

vec2 RNGV2(vec2 p) {
	vec3 a = fract(vec3(p.x, p.y, p.x) * vec3(111.11,333.33,444.44));
	a += dot(a, a+33.51);
	return fract(vec2(a.x*a.y, a.y*a.z)); //outputs a random vec2 between 0 and 1
}

void fragment() {
	vec2 uv = UV;
	float time = 121.233;// + TIME * 1.0; //Remove time for a static effect
	float minDist = 100.0;
	float minDist2 = 100.0;
	
	uv *= 3.0; // x/y range = (0.0,3.0)
	
	vec2 chunkCoordinate = fract(uv) - 0.5; // x/y range = (-0.5,0.5)
	vec2 myChunkID = floor(uv); // x/y range(0.0 or 1.0 or 2.0)
	bool is_voronoi_point = false;
	
	for(float y = -1.0; y <= 1.0; y++){
		for(float x = -1.0; x <= 1.0; x++){
			// Loop from (-1,-1) to (1,1)
			vec2 currChunkOffset = vec2(x, y); // x/y range = (-1.0 or 0.0 or 1.0)
			//if(myChunkID + currChunkOffset == vec2(1.0,1.0)) continue;
			if((myChunkID + currChunkOffset).x == 1.0) continue;
			vec2 currChunkRandVec2 = RNGV2(myChunkID + currChunkOffset); // x/y range = (0.0,1.0)
			vec2 currChunkVoronoiPoint = currChunkOffset + sin(currChunkRandVec2 * time) * 0.5;
			float d = length(chunkCoordinate - currChunkVoronoiPoint);
			if(d < 0.01) is_voronoi_point = true;
			if(d < minDist){
				minDist2 = minDist;
				minDist = d;
			}
		}
	}
	
	vec4 col = vec4(minDist,minDist,minDist, 1.0);
	
	COLOR = vec4(col);
	ivec2 intUV = ivec2(int(fract(uv.x) * 100.0), int(fract(uv.y) * 100.0));
	if(intUV.x == 0 || intUV.y == 0) COLOR = vec4(1.0,0.0,0.0,1.0);
	if(is_voronoi_point) COLOR = vec4(1.0,0.0,0.0,1.0);
	if(minDist2 - minDist < 0.01) COLOR = vec4(0.0,0.0,1.0,1.0);
}

//void light() {
	// Called for every pixel for every light affecting the CanvasItem.
	// Uncomment to replace the default light processing function with this one.
//}
