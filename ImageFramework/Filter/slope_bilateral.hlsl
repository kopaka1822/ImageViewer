#setting title, Slope Bilateral Filter
#setting description, This is a Cross Bilateral Filter that can use the slopes of then intensity values for weight differences. It is also implemented as a separable filter with a blur in x and y direction.
#setting type, TEX2D
#setting sepa, true

#texture Guide, GuideTex
#param Radius, blur_radius, int, 11, 1
#param Spatial Variance, variance_spatial, float, 20, 0.0000001
#param Source Intensity, use_source_intensity, bool, true
#param Source Slope, use_source_slope, bool, true
#param Intensity Variance (Source), variance_intensity_source, float, 0.01, 0.000000001
#param Guide Intensity, use_guide_intensity, bool, true
#param Guide Slope, use_guide_slope, bool, true
#param Intensity Variance (Guide), variance_intensity_guide, float, 0.1, 0.000000001

#param Blur Alpha, blur_alpha, bool, false

// Simple Gauss-Kernel. Normalization is not included and must be
// done by dividing through the weight sum.
float kernel(float offset, float variance)
{
	return exp(-0.5 * offset * offset / variance);
	//return exp2(-0.5 * offset * offset / variance); // to convert, multiply variances by 1/ln(2)=3.322
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

void Blur(int2 p, int2 size, float4 srcCenter, float4 guideCenter, int2 d, inout float weightSum, inout float4 pixelSum)
{
	float sourceSlope;
	float guideSlope;
	for(int i = 1; i <= blur_radius; ++i)
	{
		int2 pos = p + d * i;
		float4 srcPixel = getPixel(pos, size);
		float4 guidePixel = getGuidePixel(pos, size);
		if(i == 1)
		{
			sourceSlope = luminance(srcPixel - srcCenter);
			guideSlope = luminance(guidePixel - guideCenter);
		}

		float spatialWeight = kernel(i, variance_spatial);

		float sourceDiff = luminance(srcPixel - srcCenter);
		if(use_source_slope)
			sourceDiff = sourceDiff - sourceSlope * i;
		float sourceIntensityWeight = kernel(sourceDiff, variance_intensity_source);
		
		float guideDiff = luminance(guidePixel - guideCenter);
		if(use_guide_slope)
			guideDiff = guideDiff - guideSlope * i;
		float guideIntensityWeight = kernel(guideDiff, variance_intensity_guide);

		float w = spatialWeight;
		if(use_source_intensity)
			w *= sourceIntensityWeight;
		if(use_guide_intensity)
			w *= guideIntensityWeight;

		weightSum += w;
		pixelSum += w * getPixel(pos, size);
	}
}

float4 filter(int2 pixelCoord, int2 size)
{

	float4 srcPixel = src_image[pixelCoord];
	float4 guidePixel = GuideTex[pixelCoord];
	
	float4 pixelSum = srcPixel;
	float weightSum = 1.0;

	Blur(pixelCoord, size, srcPixel, guidePixel, -filterDirection.xy, weightSum, 
	pixelSum);
	Blur(pixelCoord, size, srcPixel, guidePixel, filterDirection.xy, weightSum, pixelSum);
	
	if(blur_alpha)
		srcPixel.a = pixelSum.a / weightSum;

	return float4(pixelSum.rgb / weightSum, srcPixel.a);
}