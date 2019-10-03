#setting sepa, true
#setting title, Gaussian Blur
#setting description, The visual effect of this blurring technique is a smooth blur resembling that of viewing the image through a translucent screen, distinctly different from the bokeh effect produced by an out-of-focus lens or the shadow of an object under usual illumination

#param Blur Radius, blur_radius, int, 20, 1
#param Variance, variance, float, 72.46, 1

// Simple Gauss-Kernel. Normalization is not included and must be
// done by dividing through the weight sum.
float kernel(int _offset)
{
	return exp(- _offset * _offset / variance);
}

float3 getPixel(int x, int y, int2 size)
{
	x = clamp(x, 0, size.x - 1);
	y = clamp(y, 0, size.y - 1);
	return src_image[int2(x,y)].rgb;
}

float4 filter(int2 pixelCoord, int2 size)
{
	float3 pixelSum = float3(0, 0, 0);
	float weightSum = 0.0;
	float alpha = src_image[pixelCoord].a;
	
	for(int d = -blur_radius; d <= blur_radius; d++)
	{			
		float w = kernel(d);
		weightSum += w;
		int2 pos = d * filterDirection + pixelCoord;
		pixelSum += w * getPixel(pos.x, pos.y, size);
	}
	
	return float4(pixelSum / weightSum, alpha);
}