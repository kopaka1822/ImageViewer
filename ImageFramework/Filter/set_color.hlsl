#setting title, Set Color Values
#setting description, Sets color channels to fixed values.

#param Set Red, setChRed, bool, false
#param Red Channel Value, chRed, float, 1
#param Set Green, setChGreen, bool, false
#param Green Channel Value, chGreen, float, 1
#param Set Blue, setChBlue, bool, false
#param Blue Channel Value, chBlue, float, 1
#param Set Alpha, setChAlpha, bool, false
#param Alpha Channel Value, chAlpha, float, 1

void main()
{
	ivec2 pixelCoord = ivec2(gl_GlobalInvocationID.xy) + pixelOffset;
	vec4 color = texelFetch(src_image, pixelCoord, 0);
	if(setChRed) color.r = chRed;
	if(setChGreen) color.g = chGreen;
	if(setChBlue) color.b = chBlue;
	if(setChAlpha) color.a = chAlpha;
	imageStore(dst_image, pixelCoord, color);
	
}