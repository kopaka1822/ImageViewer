#setting title, Height to Normal
#setting description, Generates a normal map from a height map

#param Ldr Output, ldrOut, bool, false
#param Ldr Srgb Output, ldrSrgbOut, bool, true
#param Strength, strength, float, 1.0, 0.1, 100.0

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

	float3 xVec = float3(2.0, 0.0, right - left);
	float3 yVec = float3(0.0, 2.0, bottom - top);

	float4 res = float4(normalize(cross(xVec, yVec) * float3(1, 1, 1.0 / strength)), 1.0);
	if (ldrOut || ldrSrgbOut) res.xyz = (res.xyz + 1.0) * 0.5;
	if (ldrSrgbOut) res.xyz = fromSrgb(res.xyz);
	return res;
}