#setting title, Heatmap
#setting description, Maps the red input channel to a color map. Values outside [Min Value, Max Value] are clamped. A screen overlay can be enabled via Overlays->Heatmap.
#setting type, COLOR

#param Type, type, enum {BlackRed; BlackBlueGreenRed}, BlackBlueGreenRed

#param Min Value, minVal, float, 0.0
#param Max Value, maxVal, float, 1.0

#keybinding Max Value, Add, 2.0, multiply
#keybinding Max Value, OemPlus, 2.0, multiply
#keybinding Max Value, Subtract, 0.5, multiply
#keybinding Max Value, OemMinus, 0.5, multiply

float fromSrgb(float c){
    if(c >= 1.0) return 1.0;
    if(c <= 0.0) return 0.0;
    if(c <= 0.04045) return c / 12.92;
    return pow(max((c + 0.055)/1.055, 0.0), 2.4);
}

float3 getColorBlackRed(float v)
{
	return float3(fromSrgb(v), 0.0, 0.0);
}

float3 getColorBlackBlueGreenRed(float v)
{
	if(v < 0.2)
		return lerp(float3(0.0, 0.0, 0.0), float3(0.0, 0.0, 1.0), v * 5.0);
	if(v < 0.4)
		return lerp(float3(0.0, 0.0, 1.0), float3(0.0, 1.0, 1.0), (v - 0.2) * 5.0);
	if(v < 0.6)
		return lerp(float3(0.0, 1.0, 1.0), float3(0.0, 1.0, 0.0), (v - 0.4) * 5.0);
	if(v < 0.8)
		return lerp(float3(0.0, 1.0, 0.0), float3(1.0, 1.0, 0.0), (v - 0.6) * 5.0);
	return lerp(float3(1.0, 1.0, 0.0), float3(1.0, 0.0, 0.0), (v - 0.8) * 5.0);
}

float4 filter(float4 incolor)
{
	float v = max(0,min(1,(incolor.x - minVal) / (maxVal - minVal)));
	float4 color = float4(0.0, 0.0, 0.0, 1.0);

	switch(type)
	{
	case 0:
		color.xyz = getColorBlackRed(v);
		break;
	case 1:
		color.xyz = getColorBlackBlueGreenRed(v);
		break;
	}
	
	return color;
}