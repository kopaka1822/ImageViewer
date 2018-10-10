#setting title, Bilateral Denoise
#setting description, A bilateral gaussian blur (non-seperated).

#param Blur Radius, BLUR_RADIUS, int, 4, 1, 8
#param Variance, VARIANCE, float, 15.0, 0.5, 50.0
#param Color Variance, COLOR_VARIANCE, float, 0.1, 0.0001, 2.0
#paramprop Variance, onAdd, 0.5, add
#paramprop Variance, onSubtract, -0.5, add

// Simple Gauss-Kernel. Normalization is not included and must be
// done by dividing through the weight sum.
float kernel(float _offset, float _variance)
{
	return exp(- _offset * _offset / _variance);
}

vec3 readPixel(ivec2 pos)
{
	pos = clamp(pos, ivec2(0), textureSize(src_image, 0).xy - 1);
	return texelFetch(src_image, pos, 0).rgb;
}

void main()
{
	ivec2 pixelCoord = ivec2(gl_GlobalInvocationID.xy) + pixelOffset;
	
	vec3 pixelSum = vec3(0.0);
	float weightSum = 0.0;
	vec3 centerColor = readPixel(pixelCoord);
	float centerAvg = dot(centerColor, vec3(1/3.0));
	
	for(int y = -BLUR_RADIUS; y <= BLUR_RADIUS; y++)
	for(int x = -BLUR_RADIUS; x <= BLUR_RADIUS; x++)
	{
		vec3 color = readPixel(pixelCoord + ivec2(x,y));
		float d = sqrt(x*x + y*y);
		vec3 colorDiff = (color - centerColor);
		float colorDist = length(colorDiff);
		colorDist /= dot(color, vec3(1/3.0)) + dot(centerColor, vec3(1/3.0));
		//float colorAvg = dot(color, vec3(1/3.0));
		//float colorDist = max(colorAvg / centerAvg, centerAvg / colorAvg);
		//float colorDist = abs(colorAvg - centerAvg);
		float w = kernel(d, VARIANCE) * kernel(colorDist, COLOR_VARIANCE);
		weightSum += w;
		pixelSum += w * color;
	}
	
	imageStore(dst_image, pixelCoord, vec4(pixelSum / weightSum, 1.0));
}