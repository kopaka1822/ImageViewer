#setting title, Alpha Blending Background
#setting description, Sets the background for alpha blending

#param Red Channel Value, chRed, float, 1
#param Green Channel Value, chGreen, float, 1
#param Blue Channel Value, chBlue, float, 1

float4 filter(int2 pixelCoord, int2 size)
{
	float4 color = src_image[pixelCoord];
	float3 res = color.rgb * color.a + (1.0 - color.a) * float3(chRed, chGreen, chBlue);
	return float4(res, 1.0);
}