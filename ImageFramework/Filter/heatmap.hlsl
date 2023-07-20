#setting title, Heatmap
#setting description, Maps the red input channel to a color map. Values outside [Min Value, Max Value] are clamped. A screen overlay can be enabled via Overlays->Heatmap.
#setting type, COLOR

#param Type, type, enum { Grayscale; Inferno; CoolWarm; BlackBody }, Inferno

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

float3 fromSrgb(float3 c){
	return float3(fromSrgb(c.x), fromSrgb(c.y), fromSrgb(c.z));
}

// color tables from: https://www.kennethmoreland.com/color-advice/
static float4 inferno[] = {
    float4(0, 0, 0, 0.0),
    float4(40, 11, 84, 0.14),
    float4(101, 21, 110, 0.29),
    float4(159, 42, 99, 0.43),
    float4(212, 72, 66, 0.57),
    float4(245, 125, 21, 0.71),
    float4(250, 193, 39, 0.86),
    float4(252, 255, 164, 1.0),
};

static float4 coolWarm[] = {
    float4(59, 76, 192, 0.0),
    float4(99, 125, 213, 0.142857143),
    float4(149, 173, 227, 0.285714286),
    float4(209, 220, 238, 0.428571429),
	float4(242, 242, 242, 0.5),
    float4(236, 215, 203, 0.571428571),
    float4(222, 158, 134, 0.714285714),
    float4(203, 99, 79, 0.857142857),
    float4(180,4,38, 1.0),
};

static float4 blackBody[] = {
    float4(0, 0, 0, 0),
    float4(65, 23, 18, 0.142857143),
    float4(128, 31, 27, 0.285714286),
    float4(188, 51, 32, 0.428571429),
    float4(224, 101, 10, 0.571428571),
    float4(232, 161, 26, 0.714285714),
    float4(231, 218, 48, 0.857142857),
    float4(255, 255, 255, 1),
};


#define COLOR_TABLE_FUNC(SIZE)                            \
float3 getColorFromTable##SIZE(float v, float4 table[SIZE]) {   \
	float4 c1 = table[0]; float4 c2 = table[1];           \
	[unroll] for(int i = 1; i < SIZE; i++) {              \
        c2 = table[i];                                    \
		if(v < c2.w) break;                               \
		c1 = c2;                                          \
    }                                                     \
    float3 c = lerp(fromSrgb(c1.rgb/255.0), fromSrgb(c2.rgb/255.0), (v - c1.w) / max(c2.w - c1.w, 0.0001));	\
	return c;                                             \
}

COLOR_TABLE_FUNC(8)
COLOR_TABLE_FUNC(9)

float3 getColorGray(float v)
{
    return float3(fromSrgb(v), fromSrgb(v), fromSrgb(v));
}

float4 filter(float4 incolor)
{
	float v = max(0,min(1,(incolor.x - minVal) / (maxVal - minVal)));
	float4 color = float4(0.0, 0.0, 0.0, 1.0);

	switch(type)
	{
    case 0:
        color.xyz = getColorGray(v);
		break;
    case 1:
		color.xyz = getColorFromTable8(v, inferno);
        break;
    case 2:
		color.xyz = getColorFromTable9(v, coolWarm);
        break;
    case 3:
        color.xyz = getColorFromTable8(v, blackBody);
        break;
	}
	
	return color;
}