#setting title, Normal Depth Silhouette
#setting description, Makes a silhouette based on a normal guide and a depth guide.

#texture Normal Guide, NormalTex

#texture Depth Guide, DepthTex

static const int2 g_offsets[] = {int2(-1, 0), int2(1, 0), int2(0, -1), int2(0, 1)};

float4 filter(int2 pixelCoord, int2 size)
{
	bool isEdge = false;
	float3 base = NormalTex[pixelCoord].xyz;
	float4 color = src_image[pixelCoord];
	for(int i = 0; i < 4; ++i)
	{
		float3 o = NormalTex[pixelCoord + g_offsets[i]].xyz;
		float3 p = base-o;
		if(abs(p.r)+abs(p.g)+abs(p.b) > 0.1)
			isEdge = true;
	}
	float3 base2 = DepthTex[pixelCoord].xyz;
	for(i = 0; i < 4; ++i)
	{
		float3 o = DepthTex[pixelCoord + g_offsets[i]].xyz;
		float3 p = base2-o;
		if(abs(p.r) > 0.01)
			isEdge = true;
	}
	
	if(isEdge)
		return float4(0.0, 0.0, 0.0, 1.0);
	else
		return color;
	
}