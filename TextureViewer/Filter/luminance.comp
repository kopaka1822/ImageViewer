#setting title, Luminance values
#setting description, Transforms the image into a grayscale image with luminance values. color = dot((r,g,b),(0.2126, 0.7152, 0.0722)). Alpha value remains unchanged.

#param Use vec3(0.299 0.587 0.114) instead, useOtherLuminance, bool, false

void main()
{
	ivec2 pixelCoord = ivec2(gl_GlobalInvocationID.xy) + pixelOffset;
	vec4 color = texelFetch(src_image, pixelCoord, 0);
	vec3 lum = vec3(0.2126, 0.7152, 0.0722);
	if(useOtherLuminance) lum = vec3(0.299, 0.587, 0.114);
	imageStore(dst_image, pixelCoord, vec4(vec3(dot(color.rgb, lum)), color.a));
}