#setting title, Luminance values
#setting description, Transforms the image into a grayscale image with luminance values. color = dot((r,g,b),Luminance). Alpha value remains unchanged.
#setting type, COLOR

#param Luminance, lumType, enum {(0.2126 0.7152 0.0722);(0.299 0.587 0.114)}

float4 filter(float4 color)
{
	float3 lum = lumType == 0 ? float3(0.2126, 0.7152, 0.0722) : float3(0.299, 0.587, 0.114);
	float val = dot(color.rgb, lum);
	return float4(float3(val, val, val), color.a);
}