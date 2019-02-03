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

vec3 getPixel(int x, int y)
{
	x = clamp(x, 0, textureSize(src_image, 0).x - 1);
	y = clamp(y, 0, textureSize(src_image, 0).y - 1);
	return texelFetch(src_image, ivec2(x,y), 0).rgb;
}

void main()
{
	ivec2 pixelCoord = ivec2(gl_GlobalInvocationID.xy) + pixelOffset;
	
	if(pixelCoord.x < imageSize(dst_image).x && pixelCoord.y < imageSize(dst_image).y)
	{
		vec3 pixelSum = vec3(0.0);
		float weightSum = 0.0;
		float alpha = texelFetch(src_image, pixelCoord, 0).a;
		
		for(int d = -blur_radius; d <= blur_radius; d++)
		{			
			float w = kernel(d);
			weightSum += w;
			ivec2 pos = d * filterDirection + pixelCoord;
			pixelSum += w * getPixel(pos.x, pos.y);
			
		}
		
		imageStore(dst_image, pixelCoord, vec4(pixelSum / weightSum, alpha));
	}	
}