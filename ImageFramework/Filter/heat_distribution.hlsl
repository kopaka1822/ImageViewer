#setting title, Heat Distribution
#setting description, Initial heat values should be in the red channel. If the green channel is 1.0 (255 in sRGB), the pixel is considered a constant heat source. By default, the final values will be converted to a colored heat map (check Output Heat Values to output the actual heat values instead).
#setting type, TEX2D
#setting iterations, true

#param Max iterations, num_iterations, int, 20, 1
#param Speed, c, float, 0.1, 0.01, 0.25
#param Output Heat Values, output_heat, bool, false

float getPixel(int2 pos, int2 size)
{
	pos = clamp(pos, 0, size-1);
	return src_image[pos].x;
}

float4 heatmap_color(float value);

float4 filter(int2 pixelCoord, int2 size)
{
	float4 pixelSum = 0.0;
	float weightSum = 0.0;
	
	float4 src = src_image[pixelCoord];
	float srcTemp = src.x; // temperature
	bool isConstant = src.y == 1.0;
	
	// update value if not a light source
	if(!isConstant)
	{
		float delta = c * (getPixel(pixelCoord - int2(1, 0), size) + getPixel(pixelCoord + int2(1, 0), size) + getPixel(pixelCoord - int2(0, 1), size) + getPixel(pixelCoord + int2(0, 1), size) - 4.0 * srcTemp);	
		srcTemp += delta;
	}
	
	if(int(iteration) + 1 < num_iterations)
	{
		return float4(srcTemp, src.y, 1.0, 1.0);	
	}
	
	// prepare data for displaying
	abort_iterations();
	if(output_heat) return float4(srcTemp, srcTemp, srcTemp, 1.0);

	if(isConstant && srcTemp <= 0.0)
	{
		return 0.5; // dark gray for sinks
	} 
	return heatmap_color(srcTemp);
}

float4 heatmap_color(float value)
{
	float4 color = float4(0.0, 0.0, 0.0, 1.0);
	if(value < 0.2)
		color.xyz = lerp(float3(0.0, 0.0, 0.0), float3(0.0, 0.0, 1.0), value * 5.0);
	else if(value < 0.4)
		color.xyz = lerp(float3(0.0, 0.0, 1.0), float3(0.0, 1.0, 1.0), (value - 0.2) * 5.0);
	else if(value < 0.6)
		color.xyz = lerp(float3(0.0, 1.0, 1.0), float3(0.0, 1.0, 0.0), (value - 0.4) * 5.0);
	else if(value < 0.8)
		color.xyz = lerp(float3(0.0, 1.0, 0.0), float3(1.0, 1.0, 0.0), (value - 0.6) * 5.0);
	else
		color.xyz = lerp(float3(1.0, 1.0, 0.0), float3(1.0, 0.0, 0.0), (value - 0.8) * 5.0);
	
	return color;
}