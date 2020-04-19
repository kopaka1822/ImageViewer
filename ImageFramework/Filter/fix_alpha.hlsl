#setting sepa, true
#setting title, Fix Alpha
#setting description, Clamps alpha channel between 0 and 1. Additionally bleeds color over to fully transparent pixels to reduce artifacts when using linear interpolation.
#setting type, dynamic

bool IsFirstInvocation()
{
	return filterDirection.x == 1;
}

bool IsLastInvocation()
{
#if3D 
	return filterDirection.z == 1;
#else
	return filterDirection.y == 1;
#endif
}

float4 filter(int3 pixelCoord, int3 size)
{
	float4 color = src_image[texel(pixelCoord)];
	// keep value
	color.a = min(color.a, 1.0);
	if(color.a > 0.0) return color;

	if(IsFirstInvocation())
	{
		color = 0.0; // reset color
	}

	// take color from neighboring pixels
	float4 left = src_image[texel(pixelCoord - filterDirection)];
	float4 right = src_image[texel(pixelCoord + filterDirection)];

	if(left.a > 0.0){
		color.rgb += left.rgb;
		color.a -= 1.0; // count sum
	} 
	if(right.a > 0.0) {
		color.rgb += right.rgb;
		color.a -= 1.0;
	} 

	if(IsLastInvocation())
	{
		if(color.a != 0.0)
		{
			color = float4(color.rgb / -color.a, 0.0);
		}
	}
	
	return color;
}