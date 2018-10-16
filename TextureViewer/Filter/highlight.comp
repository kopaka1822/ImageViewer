#setting title, Highlighting
#setting description, Highlights certain colors. Uses RGB average.

#param Negative Values (blue), negative, bool, true
#param Positive Values (red), positive, bool, true
#param Oversaturated Values (green), grOne, bool, false

void main()
{
	ivec2 pixelCoord = ivec2(gl_GlobalInvocationID.xy) + pixelOffset;
	vec4 color = texelFetch(src_image, pixelCoord, 0);
	float average = (color.r + color.g + color.b) / 3.0;
	if( negative && (average < 0.0) )
		color = vec4(0.0, 0.0, -average, 1.0);
	if( positive && (average > 0.0) )
		color = vec4(average, 0.0, 0.0, 1.0);
	if( grOne && (average > 1.0) )
		color = vec4(0.0, average, 0.0, 1.0);
	imageStore(dst_image, pixelCoord, color);

}