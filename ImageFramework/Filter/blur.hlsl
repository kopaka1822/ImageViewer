#setting sepa, true
#setting title, Gaussian Blur
#setting description, The visual effect of this blurring technique is a smooth blur resembling that of viewing the image through a translucent screen, distinctly different from the bokeh effect produced by an out-of-focus lens or the shadow of an object under usual illumination
#setting type, dynamic

#param Blur Radius, blur_radius, int, 20, 1
#param Variance, variance, float, 26, 1
#param Blur Alpha, blur_alpha, bool, false

#param Border Handling, border_handling, enum {Clamp; Repeat}, Clamp
#define BORDER_CLAMP 0
#define BORDER_REPEAT 1

// Simple Gauss-Kernel. Normalization is not included and must be
// done by dividing through the weight sum.
float kernel(int _offset)
{
	return exp(-0.5 * _offset * _offset / variance);
}

float4 getPixel(int3 pos, int3 size)
{
	if(border_handling == BORDER_CLAMP)
		pos = clamp(pos, 0, size-1);
	else if(border_handling == BORDER_REPEAT)
		pos = int3(uint3(pos + blur_radius * size) % uint3(size)); // % is only well defined for positive numbers, since blur_radius > 1 and size > 1 the expression (pos + blur_radius * size) should be positive

	return src_image[texel(pos)];
}

float4 filter(int3 pixelCoord, int3 size)
{
	float4 pixelSum = 0.0;
	float weightSum = 0.0;
	float alpha = src_image[texel(pixelCoord)].a;
	
	for(int d = -blur_radius; d <= blur_radius; d++)
	{			
		float w = kernel(d);
		weightSum += w;
		int3 pos = d * filterDirection + pixelCoord;
		pixelSum += w * getPixel(pos, size);
	}
	
	if(blur_alpha)
		alpha = pixelSum.a / weightSum;

	return float4(pixelSum.rgb / weightSum, alpha);
}