#setting title, Alpha Test
#setting description, Applies alpha testing with a fixed threshold
#setting type, COLOR

#param Threshold, t, float, 0.5, 0, 1
#param Smoth with Alpha, smooth, bool, false

#paramprop Threshold, onSubtract, -0.05, add
#paramprop Threshold, onAdd, 0.05, add
#keybinding Threshold, Add, 0.05, add
#keybinding Threshold, OemPlus, 0.05, add
#keybinding Threshold, Subtract, -0.05, add
#keybinding Threshold, OemMinus, -0.05, add

float4 filter(float4 color)
{
	float4 res = color;
	if(color.a < t) res.a = 0.0;
	else if(smooth)
	{
		res.a = saturate((color.a - t) / (1.0 - t));
	} 
	else res.a = 1.0; // simply pass
	return res;
}