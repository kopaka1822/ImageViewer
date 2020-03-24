#setting title, Heat Map
#setting description, Maps the red input channel to a color map. Values outside [Min Value, Max Value] are clamped.
#setting type, COLOR

#param Min Value, minVal, float, 1
#param Max Value, maxVal, float, 1

#keybinding Max Value, Add, 1, add
#keybinding Max Value, OemPlus, 1, add
#keybinding Max Value, Subtract, -1, add
#keybinding Max Value, OemMinus, -1, add

float4 filter(float4 incolor)
{
	float valueScaled = max(0,min(1,(incolor.x - minVal) / (maxVal - minVal)));
	float4 color = float4(0.0, 0.0, 0.0, 1.0);
	if(valueScaled < 0.2)
		color.xyz = lerp(float3(0.0, 0.0, 0.0), float3(0.0, 0.0, 1.0), valueScaled * 5.0);
	else if(valueScaled < 0.4)
		color.xyz = lerp(float3(0.0, 0.0, 1.0), float3(0.0, 1.0, 1.0), (valueScaled - 0.2) * 5.0);
	else if(valueScaled < 0.6)
		color.xyz = lerp(float3(0.0, 1.0, 1.0), float3(0.0, 1.0, 0.0), (valueScaled - 0.4) * 5.0);
	else if(valueScaled < 0.8)
		color.xyz = lerp(float3(0.0, 1.0, 0.0), float3(1.0, 1.0, 0.0), (valueScaled - 0.6) * 5.0);
	else
		color.xyz = lerp(float3(1.0, 1.0, 0.0), float3(1.0, 0.0, 0.0), (valueScaled - 0.8) * 5.0);
	
	return color;
}