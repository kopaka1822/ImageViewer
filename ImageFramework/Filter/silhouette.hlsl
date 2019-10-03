#setting title, Normal Depth Silhouette
#setting description, Makes a silhouette based on a normal guide and a depth guide.

#texture Normal Guide, GetNormal

#texture Depth Guide, GetDepth

const ivec2 g_offsets[] = {ivec2(-1, 0), ivec2(1, 0), ivec2(0, -1), ivec2(0, 1)};

void main()
{
	ivec2 pixelCoord = ivec2(gl_GlobalInvocationID.xy) + pixelOffset;

	bool isEdge = false;
	vec3 base = GetNormal(pixelCoord).xyz;
	vec4 color = texelFetch(src_image, pixelCoord, 0);
	for(int i = 0; i < g_offsets.length(); ++i)
	{
		vec3 o = GetNormal(pixelCoord + g_offsets[i]).xyz;
		vec3 p = base-o;
		if(abs(p.r)+abs(p.g)+abs(p.b) > 0.1)
			isEdge = true;
	}
	vec3 base2 = GetDepth(pixelCoord).xyz;
	for(int i = 0; i < g_offsets.length(); ++i)
	{
		vec3 o = GetDepth(pixelCoord + g_offsets[i]).xyz;
		vec3 p = base2-o;
		if(abs(p.r) > 0.01)
			isEdge = true;
	}
	
	if(isEdge)
		imageStore(dst_image, pixelCoord, vec4(0.0, 0.0, 0.0, 1.0));
	else
		imageStore(dst_image, pixelCoord, color);
	
}