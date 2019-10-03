#setting title, Alpha Blending Background
#setting description, Sets the background for alpha blending

#param Red Channel Value, chRed, float, 1
#param Green Channel Value, chGreen, float, 1
#param Blue Channel Value, chBlue, float, 1

void main()
{
	ivec2 pixelCoord = ivec2(gl_GlobalInvocationID.xy) + pixelOffset;

	vec4 color = texelFetch(src_image, pixelCoord, 0);
	vec3 res = color.rgb * color.a + (1.0 - color.a) * vec3(chRed, chGreen, chBlue);
	imageStore(dst_image, pixelCoord, vec4(res, 1.0));
}