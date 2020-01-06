#setting title, Kaiser Filter Mipmaps
#setting description, Mipmap generation with a kaiser filter that can handle non power of two textures
#setting type, dynamic
#if2D
#setting groupsize, 16
#endif

// approximation of bessel function from: A simple approximation for the modified Bessel function of zero order
float bessel_i0(float x)
{
    return cosh(x) / pow(1 + 0.25 * x * x, 0.25) * (1 + 0.24273 * x * x) / (1 + 0.43023 * x * x);
}

float window_kaiser(float x)
{
	const float PI = 3.141;
    const float alpha = 4.0 * PI;

    float k =
        bessel_i0( alpha * sqrt( 1.0f - x * x ) ) /
        bessel_i0( alpha );
    return k;
}

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

float getWeight(float position, float start, float end)
{
	// x in [0, 1]
	float x = (position + 0.5 - start) / (end - start);
	// x in [-1, 1]
	x = x * 2 - 1;
	return window_kaiser(clamp(x, -1, 1)) * getVisibility(position, start, end);
}

float4 filter(int3 pixelCoord, int3 size)
{
	if(level == 0){
		return src_image[texel(pixelCoord)];
	} else {
		double4 dcolor = 0.0; // high preision color because large sums can screw with float precision

#if2D
		size.z = 1; // set layer count to 1
		pixelCoord.z = layer; // set z coordinate to layer
#endif
		
		// calculate filter range
		float3 start;
		float3 end;
		{
			float3 filterSize = float3(getLevel0Size()) / float3(size);
			start = float3(pixelCoord) * filterSize;
			end = float3(pixelCoord + 1) * filterSize;
		}

		int3 pos;
		int3 starti = floor(start);
		int3 endi = ceil(end);
		endi.xy = min(endi.xy, getLevel0Size().xy);
		double weightSum = 0.0;
		bool hasAlpha = false; // indicates if at least one pixel had alpha != 1

		for(pos.z = starti.z; pos.z < endi.z; ++pos.z)
		{
			float wz = getWeight(pos.z, start.z, end.z);
			for(pos.y = starti.y; pos.y < endi.y; ++pos.y)
			{
				float wyz = wz * getWeight(pos.y, start.y, end.y);
				for(pos.x = starti.x; pos.x < endi.x; ++pos.x)
				{
					float w = wyz * getWeight(pos.x, start.x, end.x);
					weightSum += (double)(w);

					// sum up pixels
					float4 c = src_image_ex.mips[0][pos];
					if(c.a != 1.0) hasAlpha = true;
					dcolor.a += (double)(c.a * w); // sum up all alpha
					// scale color with alpha (higher alpha values have more impact on the actual color)
					dcolor.rgb += double3(c.a * c.rgb * w);
				}
			}
		}
		
		// divide sum by filter size
		dcolor /= weightSum;
		if(hasAlpha)
		{
			// scale color according to alpha
			if(dcolor.a != 0.0) dcolor.rgb /= dcolor.a;
		}
		else
		{
			dcolor.a = 1.0; // not always true due to precision errors
		}
		
		return (float4)(dcolor);
	}
}