#setting title, Height to Normal
#setting description, Generates a normal map from a height map

#param Output, output, enum {Unsigned sRGB; Unsigned; Signed}
#define OUTPUT_LDR_SRGB 0
#define OUTPUT_LDR 1
#define OUTPUT_SIGNED 2
#param Aspect Ratio of heightmap height to texture size (what is the maximum height of the height map in world space if the texture would span one world unit?), aspectRatio, float, 0.01, 0.001, 1000.0

float3 fromSrgb(float3 c){
    float3 r;
    [unroll]
    for(int i = 0; i < 3; ++i){
        if(c[i] >= 1.0) r[i] = 1.0;
        else if(c[i] <= 0.0) r[i] = 0.0;
        else if(c[i] <= 0.04045) r[i] = c[i] / 12.92;
        else r[i] = pow(max((c[i] + 0.055)/1.055, 0.0), 2.4);
    }
    return r;
}

float getPixel(int2 coord, int2 size) {
	coord = clamp(coord, int2(0, 0), int2(size.x - 1, size.y - 1));
	return src_image[coord].x;
}

float4 filter(int2 pixelCoord, int2 size)
{
	float left = getPixel(pixelCoord + int2(-1, 0), size);
	float right = getPixel(pixelCoord + int2(1, 0), size);
	float top = getPixel(pixelCoord + int2(0, 1), size);
	float bottom = getPixel(pixelCoord + int2(0, -1), size);

	float heightScale = size.x * aspectRatio;
	float3 xVec = float3(2.0, 0.0, (right - left) * heightScale);
	float3 yVec = float3(0.0, 2.0, (bottom - top) * heightScale);

	float4 res = float4(normalize(cross(xVec, yVec)), 1.0);
	if (output == OUTPUT_LDR_SRGB || output == OUTPUT_LDR) res.xyz = (res.xyz + 1.0) * 0.5;
	if (output == OUTPUT_LDR_SRGB) res.xyz = fromSrgb(res.xyz);
	return res;
}