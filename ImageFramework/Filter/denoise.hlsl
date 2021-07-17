#setting title, Bilateral Denoise
#setting description, A bilateral gaussian blur (non-separated). Smaller Variance values result in sharper but more noisy results. High variance might overblur the result.

#param Blur Radius, BLUR_RADIUS, int, 4, 1, 8
#param Distance Variance, VARIANCE, float, 15.0, 0.5, 50.0
#param Color Variance, COLOR_VARIANCE, float, 0.05, 0.0001, 2.0
#paramprop Distance Variance, onAdd, 2.0, multiply
#paramprop Distance Variance, onSubtract, 0.5, multiply
#paramprop Color Variance, onAdd, 2.0, multiply
#paramprop Color Variance, onSubtract, 0.5, multiply

// Simple Gauss-Kernel. Normalization is not included and must be
// done by dividing through the weight sum.
float kernel(float _offset, float _variance)
{
	return exp(- _offset * _offset / _variance);
}

float3 readPixel(int2 pos, int2 size)
{
	pos = clamp(pos, int2(0, 0), size - int2(1, 1));
	return src_image[pos].rgb;
}

float4 filter(int2 pixelCoord, int2 size)
{
	float3 pixelSum = float3(0.0, 0.0, 0.0);
	float weightSum = 0.0;
	float3 centerColor = readPixel(pixelCoord, size);
	const float3 threeInv = float3(1/3.0, 1/3.0, 1/3.0);
	float centerAvg = dot(centerColor, threeInv);
	
	for(int y = -BLUR_RADIUS; y <= BLUR_RADIUS; y++)
	for(int x = -BLUR_RADIUS; x <= BLUR_RADIUS; x++)
	{
		float3 color = readPixel(pixelCoord + int2(x,y), size);
		float d = sqrt(x*x + y*y);
		float3 colorDiff = (color - centerColor);
		float colorDist = length(colorDiff);
		colorDist /= (abs(dot(color, threeInv) + dot(centerColor, threeInv)) + 0.0001); // add small offset to avoid dividing by zero
		//float colorAvg = dot(color, threeInv);
		//float colorDist = max(colorAvg / centerAvg, centerAvg / colorAvg);
		//float colorDist = abs(colorAvg - centerAvg);
		float w = kernel(d, VARIANCE) * kernel(colorDist, COLOR_VARIANCE);
		weightSum += w;
		pixelSum += w * color;
	}
	
	return float4(pixelSum / weightSum, 1.0);
}