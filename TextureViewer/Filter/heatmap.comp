#setting title, Heat Map
#setting description, Maps the red input channel to a color map. Values outside [Min Value, Max Value] are clamped.

#param Min Value, minVal, float, 1
#param Max Value, maxVal, float, 1

void main()
{
	ivec2 pixelCoord = ivec2(gl_GlobalInvocationID.xy) + pixelOffset;
	float valueScaled = max(0,min(1,(texelFetch(src_image, pixelCoord, 0).x - minVal) / (maxVal - minVal)));
	vec4 color = vec4(0.0, 0.0, 0.0, 1.0);
	if(valueScaled < 0.2)
		color.xyz = mix(vec3(0.0, 0.0, 0.0), vec3(0.0, 0.0, 1.0), valueScaled * 5.0);
	else if(valueScaled < 0.4)
		color.xyz = mix(vec3(0.0, 0.0, 1.0), vec3(0.0, 1.0, 1.0), (valueScaled - 0.2) * 5.0);
	else if(valueScaled < 0.6)
		color.xyz = mix(vec3(0.0, 1.0, 1.0), vec3(0.0, 1.0, 0.0), (valueScaled - 0.4) * 5.0);
	else if(valueScaled < 0.8)
		color.xyz = mix(vec3(0.0, 1.0, 0.0), vec3(1.0, 1.0, 0.0), (valueScaled - 0.6) * 5.0);
	else
		color.xyz = mix(vec3(1.0, 1.0, 0.0), vec3(1.0, 0.0, 0.0), (valueScaled - 0.8) * 5.0);
	imageStore(dst_image, pixelCoord, color);
}