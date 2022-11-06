#setting title, Alpha Test
#setting description, Applies alpha testing with a fixed threshold
#setting type, COLOR

#param Threshold, t, float, 0.5, 0, 1

float4 filter(float4 color)
{
	float4 res = color;
	if(color.a < t) res.a = 0.0;
	else res.a = 1.0;
	return res;
}