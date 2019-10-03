#setting title, Color Clamping
#setting description, Clamps the RGB values between minimum and maximum. Alpha won't be changed.

#param Minimum, minimum, float, 0
#param Maximum, maximum, float, 1

void main()
{
	ivec2 pixelCoord = ivec2(gl_GlobalInvocationID.xy) + pixelOffset;

	vec4 color = texelFetch(src_image, pixelCoord, 0);
	imageStore(dst_image, pixelCoord, vec4(clamp(color.rgb, vec3(minimum), vec3(maximum)), color.a));
	
}