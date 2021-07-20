#setting title, Fix Alpha
#setting description, Clamps alpha channel between 0 and 1. Additionally, iteratively bleeds color over to fully transparent pixels to reduce artifacts when using linear interpolation and conventional mipmap generation (check "Debug Alpha" to preview the bleeding).
#setting type, dynamic
#setting iterations, true

#param Debug Alpha, alpha_one, bool, false

float4 filter(int3 pixelCoord, int3 size)
{
	float4 color = src_image[texel(pixelCoord)];

	if(iteration == 0) // initialize values
	{
		if(color.a <= 0.0) color.a = -1.0; // indicate invalid pixel
		else if(alpha_one) color.a = 1.0;
		else color.a = min(color.a, 1.0); // assure that it is not bigger than 1
		return color;
	}

	if(color.a >= 0.0) // pixel already converged
	{
		abort_iterations();
		return color;
	}

	// take average color from neighboring valid pixels
	float weightSum = 0.0;
	float3 sum = 0.0;

	// choose big radius to covnerge faster (the lookups are not that expensive apparently)
	#define RADIUS 2
	for(int z = max(0, pixelCoord.z - RADIUS); z <= min(pixelCoord.z + RADIUS, size.z-1); ++z)
	for(int y = max(0, pixelCoord.y - RADIUS); y <= min(pixelCoord.y + RADIUS, size.y-1); ++y)
	for(int x = max(0, pixelCoord.x - RADIUS); x <= min(pixelCoord.x + RADIUS, size.x-1); ++x)
	{
		float4 c = src_image[texel(int3(x,y,z))];
		if(c.a < 0.0) continue; // this sample does not count
		
		float dist = distance(pixelCoord, int3(x,y,z));
		float weight = exp(-dist * dist); 
		sum += weight * c.rgb;
		weightSum += weight;
	}
	
	if(weightSum > 0.0) // this sample is now valid!
	{
		color.rgb = sum / weightSum;
		color.a = 0.0;
		if(alpha_one) color.a = 1.0;
		abort_iterations();
	}

	return color;
}