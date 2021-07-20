#setting title, Guided Bilateral Filter
#setting description, Also known as Joined/Cross Bilateral Filter. Works like the bilateral filter but uses the pixel values from the guidance image for the intensity weight.
#setting type, TEX2D

#texture Guide, GuideTex
#param Radius, blur_radius, int, 20, 1
#param Spatial Variance, variance_spatial, float, 26, 0.0000001
#param Intensity Variance, variance_intensity, float, 0.05, 0.000000001
#paramprop Intensity Variance, OnAdd, 2.0, multiply
#paramprop Intensity Variance, OnSubtract, 0.5, multiply
#param Blur Alpha, blur_alpha, bool, false

// Simple Gauss-Kernel. Normalization is not included and must be
// done by dividing through the weight sum.
float kernel(float offset, float variance)
{
	return exp(-0.5 * offset * offset / variance);
}

float4 getPixel(int2 pos, int2 size)
{
	pos = clamp(pos, 0, size-1);
	return src_image[pos];
}

float4 getGuidePixel(int2 pos, int2 size)
{
	pos = clamp(pos, 0, size-1);
	return GuideTex[pos];
}

float luminance(float4 pixel)
{
	return dot(pixel.rgb, float3(0.2126, 0.7152, 0.0722));
}

float4 filter(int2 pixelCoord, int2 size)
{
	float4 pixelSum = 0.0;
	float weightSum = 0.0;
	float4 srcPixel = src_image[pixelCoord];
	srcPixel.rgb = GuideTex[pixelCoord].rgb;
	
	int2 d;
	for(d.y = -blur_radius; d.y <= blur_radius; ++d.y)
	for(d.x = -blur_radius; d.x <= blur_radius; ++d.x)
	{			
		int2 pos = pixelCoord + d;

		float spatialWeight = kernel(length(d), variance_spatial);
		float intensityWeight = kernel(luminance(abs(getGuidePixel(pos, size) - srcPixel)), variance_intensity);

		float w = spatialWeight * intensityWeight;
		
		weightSum += w;
		pixelSum += w * getPixel(pos, size);
	}
	
	if(blur_alpha)
		srcPixel.a = pixelSum.a / weightSum;

	return float4(pixelSum.rgb / weightSum, srcPixel.a);
}