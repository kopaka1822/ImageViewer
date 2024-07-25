#setting title, Colorspace
#setting description, Converts color from one colorspace to another
#setting type, COLOR

#param Input, input_e, enum {Linear; YCrCb; YCgCo; YUV }, Linear
#param Output, output_e, enum {Linear; YCrCb; YCgCo; YUV }, YCrCb

float3 YCrCb2rgb(float3 color){ 
    float r = color.r-.5 + 1.403 * (color.g); 
    float g = color.r - .5 - 0.714 * (color.g ) - 0.344 * (color.b ); 
    float b = color.r - .5 + 1.773 * (color.b ); 
    return float3(r, g, b); 
}

float3 rgb2YCrCb(float3 color) { 
    float Y = 0.299*color.r + 0.587 * color.g + 0.114*color.b ; 
    float Cr = (color.r - Y) * 0.713 ; 
    float Cb = (color.b - Y) * 0.564 ; 
    return float3(Y+.5, Cr, Cb); 
} 

inline float3 RGBToYCgCo(float3 rgb)
{
    float Y = dot(rgb, float3(0.25f, 0.50f, 0.25f));
    float Cg = dot(rgb, float3(-0.25f, 0.50f, -0.25f));
    float Co = dot(rgb, float3(0.50f, 0.00f, -0.50f));
    return float3(Y, Cg, Co);
}

inline float3 YCgCoToRGB(float3 YCgCo)
{
    float tmp = YCgCo.x - YCgCo.y;
    float r = tmp + YCgCo.z;
    float g = YCgCo.x + YCgCo.y;
    float b = tmp - YCgCo.z;
    return float3(r, g, b);
}

///////////// YUV ////////////////
float3 RGBtoYUV(float3 rgb)
{
    float3 yuv;
    yuv.x = 0.299 * rgb.r + 0.587 * rgb.g + 0.114 * rgb.b;
    yuv.y = -0.14713 * rgb.r - 0.28886 * rgb.g + 0.436 * rgb.b;
    yuv.z = 0.615 * rgb.r - 0.51499 * rgb.g - 0.10001 * rgb.b;
    return yuv;
}

float3 YUVtoRGB(float3 yuv)
{
    float3 rgb;
    rgb.r = yuv.x + 1.13983 * yuv.z;
    rgb.g = yuv.x - 0.39465 * yuv.y - 0.58060 * yuv.z;
    rgb.b = yuv.x + 2.03211 * yuv.y;
    return rgb;
}

float4 filter(float4 color)
{
	float4 res = color;
	
	// step 1: convert to linear
	if(input_e == E_YCrCb) res.rgb = YCrCb2rgb(res.rgb);
	if(input_e == E_YCgCo) res.rgb = YCgCoToRGB(res.rgb);
	if(input_e == E_YUV) res.rgb = YUVtoRGB(res.rgb);

	// step 2: convert to output
	if(output_e == E_YCrCb) res.rgb = rgb2YCrCb(res.rgb);
	if(output_e == E_YCgCo) res.rgb = RGBToYCgCo(res.rgb);
	if(output_e == E_YUV) res.rgb = RGBtoYUV(res.rgb);

	return res;
}