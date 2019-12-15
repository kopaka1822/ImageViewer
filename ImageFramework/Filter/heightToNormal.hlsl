#setting title, Height to Normal
#setting description, Generates a normal map from a height map

#param Ldr Output, ldrOut, bool, false
#param Strength, strength, float, 1.0, 0.1, 100.0

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
	if (ldrOut) res.xyz = (res.xyz + 1.0) * 0.5;
	return res;
}