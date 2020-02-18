#setting title, Gamma Correction
#setting description, Nonlinear operation used to encode and decode luminance or tristimulus values in video or still image systems. Formula: |Factor * V| ^ (1/Gamma).
#setting type, COLOR

#param Gamma, gamma, float, 1, 0
#paramprop Gamma, onSubtract, -0.1, add
#paramprop Gamma, onAdd, 0.1, add

#param Factor, factor, float, 1.0, 0

#param Per Channel Gamma, perChannel, bool, false
#param Keep Sign, keepSign, bool, true

#keybinding Factor, Add, 2.0, multiply
#keybinding Factor, OemPlus, 2.0, multiply
#keybinding Factor, Subtract, 0.5, multiply
#keybinding Factor, OemMinus, 0.5, multiply

float4 filter(float4 color)
{
	float3 sgn = sign(color.rgb);
	color.rgb = abs(color.rgb * factor);
	const float invGamma = 1.0 / gamma;

	if(perChannel)
	{
		color.rgb = pow(color.rgb, float3(invGamma, invGamma, invGamma));
	}
	else
	{
		// this luminance looks better
		float lum = dot(color.rgb, float3(0.299, 0.587, 0.114));
		//float lum = dot(color.rgb, float3(0.2126, 0.7152, 0.0722));
		if(lum > 0)
		{
			float newLum = pow(lum, invGamma);
			color.rgb = color.rgb / lum * newLum;
		}
	}
	if(keepSign)
		color.rgb *= sgn;
	
	return color;
}