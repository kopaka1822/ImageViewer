#setting title, Box Filter Mipmaps
#setting description, Mipmap generation with a box filter that can handle non power of two textures
#setting type, dynamic

uint3 getLevel0Size()
{
#if2D
	uint w, h, p1, p2;
	src_image_ex.GetDimensions(0, w, h, p1, p2);
	return uint3(w, h, 1);
#else
	uint w, h, d, p1;
	src_image_ex.GetDimensions(0, w, h, d, p1);
	return uint3(w, h, d);
#endif
}

// indicates how much of the box is covered
float getVisibility(float position, float start, float end)
{
	if(position < start) return position + 1.0 - start;
	return min(end - position, 1.0);
}

float4 filter(int3 pixelCoord, int3 size)
{
	float4 color = 0.0;

	if(level == 0){
		color = src_image[texel(pixelCoord)];
	} else {
		double4 dcolor = 0.0; // high preision color because large sums can screw with float precision

		// calculate filter dimension based on level 0 size
		uint3 baseSize = getLevel0Size();	
#if2D
		size.z = 1; // set layer count to 1
		pixelCoord.z = layer; // set z coordinate to layer
#endif
		float3 filterSize = float3(baseSize) / float3(size);

		// calculate filter range
		float3 start = float3(pixelCoord) * filterSize;
		float3 end = float3(pixelCoord + 1) * filterSize;

		int3 pos;
		int3 starti = floor(start);
		int3 endi = ceil(end);
		endi.xy = min(endi.xy, baseSize.xy);
		float3 vis = 0; // visibility for each axis
		bool hasAlpha = false; // indicates if at least one pixel had alpha != 1

		for(pos.z = starti.z; pos.z < endi.z; ++pos.z)
		{
			vis[2] = getVisibility(pos.z, start.z, end.z);
			for(pos.y = starti.y; pos.y < endi.y; ++pos.y)
			{
				vis[1] = getVisibility(pos.y, start.y, end.y);
				for(pos.x = starti.x; pos.x < endi.x; ++pos.x)
				{
					vis[0] = getVisibility(pos.x, start.x, end.x);
					float visibility = vis.x * vis.y * vis.z;

					// sum up pixels
					float4 c = src_image_ex.mips[0][pos];
					if(c.a != 1.0) hasAlpha = true;
					dcolor.a += (double)(c.a * visibility); // sum up all alpha
					// scale color with alpha (higher alpha values have more impact on the actual color)
					dcolor.rgb += double3(c.a * c.rgb * visibility);
				}
			}
		}
		
		// divide sum by filter size
		dcolor /= (double)(filterSize.x * filterSize.y * filterSize.z);
		if(hasAlpha)
		{
			// scale color according to alpha
			if(dcolor.a != 0.0) dcolor.rgb /= dcolor.a;
		}
		else
		{
			dcolor.a = 1.0; // not always true due to precision errors
		}
		
		color = (float4)(dcolor);
	}

	return color;
}