#setting title, Enhance
#setting description, Local contrast and saturation enhancements as well as global saturation.

#param Radius, RADIUS, int, 10, 3, 20
#param Brightness Contrast, BR_CONTRAST, float, 1.0, 0.0, 10.0
#param Color Contrast, COLOR_CONTRAST, float, 1.0, 0.0, 10.0
#param Saturation, SATURATION, float, 1.0, 0.0, 10.0

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
	float2 avgBrSat = float2(0.0, 0.0);
	float weightSum = 0.0;
	float3 centerColor = readPixel(pixelCoord, size);

	// Decompose center color
	float centerBr = dot(centerColor, float3(0.299, 0.587, 0.114)) + 1e-6;
	float2 ab = float2(centerColor.r - 0.5 * (centerColor.g + centerColor.b),
					0.866025404 * (centerColor.g - centerColor.b));
	float centerSat = length(ab) / centerBr;
	//float centerH = atan(b, a);
	if(centerBr != centerBr) {
		centerBr = 0.5;
		centerSat = 0.5;
		ab = float2(0.0, 0.0);
	}

	// Gaussian filter to get the average brightness and saturation
	for(int y = -RADIUS; y <= RADIUS; y++)
	for(int x = -RADIUS; x <= RADIUS; x++)
	{
		float3 color = readPixel(pixelCoord + int2(x,y), size);
		float brightness = dot(color, float3(0.299, 0.587, 0.114)) + 1e-6;
		if(brightness != brightness) continue;
		float saturation = length(float2(color.r - 0.5 * (color.g + color.b), 0.866025404 * (color.g - color.b))) / brightness;
		float d = sqrt(x*x + y*y);
		float w = kernel(d, RADIUS * RADIUS / 4.0) * kernel(brightness - centerBr, 0.5);
		weightSum += w;
		avgBrSat += w * float2(brightness, saturation);
	}
	avgBrSat /= weightSum;

	// Increase contrasts
	float newSat = avgBrSat.y + (centerSat - avgBrSat.y) * COLOR_CONTRAST;
	newSat = newSat * SATURATION;
	float newBr = max(0, avgBrSat.x + (centerBr - avgBrSat.x) * BR_CONTRAST);

	// Return to RGB
	ab *= newSat * newBr / (centerSat * centerBr + 1e-6);
	float3 outColor = newBr + float3(0.701 * ab.x - 0.273086677 * ab.y,
								-0.299 * ab.x + 0.304263592 * ab.y,
								-0.299 * ab.x - 0.850436947 * ab.y);

	return float4(outColor, 1.0);
}