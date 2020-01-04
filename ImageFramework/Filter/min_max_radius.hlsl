#setting title, Min Max Radius
#setting description, Chooses the pixel with the min/max luminance within the filter radius.
#setting type, DYNAMIC

#param Radius, RADIUS, Int, 5, 1
#param Square Radius, UseSquare, Bool, false
#param Min (otherwise Max), UseMin, Bool, false

#define FLOAT_MAX 3.40282e+03

int lenSq(int3 v)
{
	return dot(v, v);
}

float4 filter(int3 pixelCoord, int3 size)
{
	float alpha = src_image[texel(pixelCoord)].a;
	float best = -FLOAT_MAX;
	float3 value = 0.0;
	if(UseMin) best = FLOAT_MAX;
	float radiusSq = RADIUS * RADIUS;

	for(int z = max(0, pixelCoord.z - RADIUS); z <= min(pixelCoord.z + RADIUS, size.z-1); ++z)
	for(int y = max(0, pixelCoord.y - RADIUS); y <= min(pixelCoord.y + RADIUS, size.y-1); ++y)
	for(int x = max(0, pixelCoord.x - RADIUS); x <= min(pixelCoord.x + RADIUS, size.x-1); ++x)
	{
		if(!UseSquare && lenSq(int3(x, y, z) - pixelCoord) > radiusSq)
		 continue;

		float3 v = src_image[texel(int3(x,y,z))].rgb;
		float lum = dot(float3(0.299, 0.587, 0.114), v);
		if(UseMin != (lum > best))
		{
			best = lum;
			value = v;
		}
	}

	return float4(value, alpha);
}