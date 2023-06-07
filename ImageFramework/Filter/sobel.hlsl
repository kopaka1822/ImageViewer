#setting title, Sobel Filter
#setting description, Edge detection filter with the immediate neighborhood
#setting type, DYNAMIC

float4 calcEdge(int3 pixelCoord, int3 size, int3 dir)
{
	float4 left = src_image[texel(clamp(pixelCoord - dir, 0, size - 1))];
    float4 right = src_image[texel(clamp(pixelCoord + dir, 0, size - 1))];
    return left - right;
}

float4 pow2(float4 v)
{
	return v*v;
}

float4 filter(int3 pixelCoord, int3 size)
{
	float alpha = src_image[texel(pixelCoord)].w;

    float4 sum = 0;
	sum += pow2(calcEdge(pixelCoord, size, int3(1, 0, 0)));
    sum += pow2(calcEdge(pixelCoord, size, int3(0, 1, 0)));
#if3D
	sum += pow2(calcEdge(pixelCoord, size, int3(0, 0, 1)));
#endif
	return float4(sqrt(sum.xyz), alpha);
}