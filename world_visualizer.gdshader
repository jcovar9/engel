shader_type canvas_item;

// 📏 Parameters
uniform int pixels_per_side = 300;
uniform vec2 offset = vec2(0.0);
uniform float zoom = 1.0;
uniform float map_size = 6.0;
uniform float min_vsize = 1.0;
uniform int num_vlayers = 3;
uniform float edge_sharpness = 5.0;
uniform float seed = 0.0;        // Offset to vary the noise
uniform int octaves = 5;         // Number of noise layers
uniform float lacunarity = 2.0;  // Frequency multiplier per octave
uniform float gain = 0.6;        // Amplitude reduction per octave

vec2 mod289v2(const in vec2 x) { return x - floor(x * (1. / 289.)) * 289.; }
vec3 mod289v3(const in vec3 x) { return x - floor(x * (1. / 289.)) * 289.; }
vec3 permutev3(const in vec3 v) { return mod289v3(((v * 34.0) + 1.0) * v); }
float snoise(vec2 v) {
    const vec4 C = vec4(0.211324865405187,  // (3.0-sqrt(3.0))/6.0
                        0.366025403784439,  // 0.5*(sqrt(3.0)-1.0)
                        -0.577350269189626,  // -1.0 + 2.0 * C.x
                        0.024390243902439); // 1.0 / 41.0
    // First corner
    vec2 i  = floor(v + dot(v, C.yy));
    vec2 x0 = v -   i + dot(i, C.xx);

    // Other corners
    vec2 i1 = (x0.x > x0.y) ? vec2(1.0, 0.0) : vec2(0.0, 1.0);
    vec4 x12 = x0.xyxy + C.xxzz;
    x12.xy -= i1;

    // Permutations
    i = mod289v2(i); // Avoid truncation effects in permutation
    vec3 p = permutev3( permutev3( i.y + vec3(0.0, i1.y, 1.0 ))
    + i.x + vec3(0.0, i1.x, 1.0 ));

    vec3 m = max(0.5 - vec3(dot(x0,x0), dot(x12.xy,x12.xy), dot(x12.zw,x12.zw)), 0.0);
    m = m*m ;
    m = m*m ;

    // Gradients: 41 points uniformly over a line, mapped onto a diamond.
    // The ring size 17*17 = 289 is close to a multiple of 41 (41*7 = 287)

    vec3 x = 2.0 * fract(p * C.www) - 1.0;
    vec3 h = abs(x) - 0.5;
    vec3 ox = floor(x + 0.5);
    vec3 a0 = x - ox;

    // Normalise gradients implicitly by scaling m
    // Approximation of: m *= inversesqrt( a0*a0 + h*h );
    m *= 1.79284291400159 - 0.85373472095314 * ( a0*a0 + h*h );

    // Compute final noise value at P
    vec3 g;
    g.x  = a0.x  * x0.x  + h.x  * x0.y;
    g.yz = a0.yz * x12.xz + h.yz * x12.yw;
    return 130.0 * dot(m, g);
}

// 🌊 Fractal Brownian Motion
float fbm (vec2 pos) {
    // Initial values
    float amplitude = 0.5;
    float value = 0.0;
    //
    // Loop of octaves
    for (int i = 0; i < octaves; i++) {
        value += amplitude * snoise(pos + seed);
        pos *= lacunarity;
        amplitude *= gain;
    }
    return value;
}

float edge_falloff(vec2 pos) {
	vec2 center = vec2(map_size) * 0.5;
	vec2 dist = abs(pos - center) / center;
	float edge_dist = max(dist.x, dist.y);
	return 1.0 - pow(clamp(edge_dist, 0.0, 1.0), edge_sharpness);
}

float lerp(float from, float to, float value) {
	return from * (1.0 - value) + to * value;
}
float inverse_lerp(float from, float to, float value) {
	return float((value - from) / (to - from));
}

float rand(vec2 p) {
	p += seed;
	const vec2 k = vec2(127.1, 311.7);
	float n = sin(dot(p, k)) * 43758.5453 + seed;
	return fract(n);
}

vec2 rand_vec2(vec2 p) {
	p += seed;
	const vec2 k = vec2(127.1, 311.7);
	const vec2 l = vec2(269.5, 183.3);
	float n = sin(dot(p, k)) * 43758.5453 + seed;
	float m = sin(dot(p, l)) * 37493.2831 + seed;
	return fract(vec2(n, m));
}

vec2 get_vorigin(vec2 pos, float vsize) {
	return floor(pos / vsize) * vsize;
}

vec2 get_vpoint(vec2 vorigin, float vsize) {
	return vorigin + rand_vec2(vorigin) * vsize;
}

vec2 get_closest_vpoint(vec2 pos, float vsize) {
	vec2 vorigin = get_vorigin(pos, vsize);
	vec2 closest_vpoint;
	float closest_dist = 100000000.0;
	for(float x = -1.0; x < 1.5; x += 1.0) {
		for(float y = -1.0; y < 1.5; y += 1.0) {
			vec2 curr_vorigin = vorigin + vec2(x,y) * vsize;
			vec2 curr_vpoint = get_vpoint(curr_vorigin, vsize);
			float curr_dist = distance(curr_vpoint, pos);
			if(curr_dist < closest_dist) {
				closest_vpoint = curr_vpoint;
				closest_dist = curr_dist;
			}
		}
	}
	return closest_vpoint;
}

int ocean_or_land(vec2 land_vpoint, vec2 pos, float max_vsize) {
	vec2 curr_vorigin = get_vorigin(pos, min_vsize);
	vec2 curr_vpoint;
	vec2 curr_close_vpoint;
	float curr_vsize = min_vsize;
	while(curr_vsize <= max_vsize) {
		curr_vpoint = get_vpoint(curr_vorigin, curr_vsize);
		curr_close_vpoint = get_closest_vpoint(curr_close_vpoint, curr_vsize);
		curr_vsize *= 2.0;
	}
	if(curr_close_vpoint == land_vpoint) {
		return 1;
	} else {
		return 0;
	}
}

void fragment() {
	vec2 pixel_uv = floor(UV * float(pixels_per_side)) / float(pixels_per_side);
	vec2 pixel_pos = (((pixel_uv - 0.5) / zoom + 0.5) * map_size) + offset;
	
	float max_vsize = min_vsize * pow(2.0, float(num_vlayers));
	vec2 land_vorigin = get_vorigin(vec2(map_size, map_size) / 2.0, max_vsize);
	vec2 land_vpoint = get_vpoint(land_vorigin, max_vsize);
	
	if(ocean_or_land(land_vpoint, pixel_pos, max_vsize) == 0) {
		COLOR = vec4(0.0, 0.0, 1.0, 1.0);
	} else {
		COLOR = vec4(0.0, 1.0, 0.0, 1.0);
	}
	
	//float start_vsize = map_size / 2.0;
	//vec2 closest_vpoint = get_closest_vpoint(pixel_pos, start_vsize);
	//COLOR = vec4(distance(closest_vpoint, pixel_pos) / start_vsize, 0.0, 0.0, 1.0);

    //// Compute FBM value
    //float n = fbm(pixel_pos);
	//n = (n + 1.0) / 2.0;
	//n *= edge_falloff(pixel_pos);
	////float n = snoise(pixel_pos);
	////n += 1.0;
	////n /= 2.0;
    //// Output color
	//if(n < 0.0 || 1.0 < n) {
		//COLOR = vec4(1.0, 0.0, 0.0, 1.0);
	//} else if(n < 0.5) {
		//COLOR = vec4(0.0, 0.0, n, 1.0);
	//} else {
		//COLOR = vec4(0.0, n, 0.0, 1.0);
	//}
}
