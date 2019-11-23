#setting title, Median Filter
#setting description, Chooses the pixel with the average luminance within the filter radius.
#setting type, DYNAMIC

#if2D
#setting groupsize, 5
#param Radius, RADIUS, Int, 1, 1, 6
#else
#setting groupsize, 3
#param Radius, RADIUS, Int, 1, 1, 2
#endif

// sum of temp registers and indexable temp registers times 64 threads exceeds the recommended total 16384.  Performance may be reduced
#pragma warning( disable : 4717 )

void bubbleSort(int numLength, inout float4 buf[169])
{
	int i = 0;
	int j = 0;
	int flag = 1;
	for(i = 1; (i <= numLength) && (flag != 0); i++)
	{
		flag = 0;
		for (j=0; j < (numLength - i); j++)
		{
			if (buf[j+1].w > buf[j].w)
			{
				float4 temp = buf[j];
				buf[j] = buf[j+1];
				buf[j+1] = temp;
				flag = 1;
			}
		}
	}
}
			
float4 filter(int3 pixelCoord, int3 size)
{
	float4 buf[169];

	float alpha = src_image[texel(pixelCoord)].a;
	// fill buffer
	uint count = 0;

	for(int z = max(0, pixelCoord.z - RADIUS); z <= min(pixelCoord.z + RADIUS, size.z-1); ++z)
	for(int y = max(0, pixelCoord.y - RADIUS); y <= min(pixelCoord.y + RADIUS, size.y-1); ++y)
	for(int x = max(0, pixelCoord.x - RADIUS); x <= min(pixelCoord.x + RADIUS, size.x-1); ++x)
	{
		buf[count].rgb = src_image[texel(int3(x,y,z))].rgb;
		buf[count].w = dot(float3(0.299, 0.587, 0.114), buf[count].rgb);
		++count;
	}
	
	// sort the buffer
	bubbleSort(count, buf);

	return float4(buf[count/2].rgb, alpha);
}