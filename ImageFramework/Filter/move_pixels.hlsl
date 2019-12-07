#setting title, Move Pixels
#setting description, Moves pixels in x or y direction

#param Move X, mx, int, 0
#param Move Y, my, int, 0

float4 filter(int2 pixelCoord, int2 size)
{
	uint2 off = uint2(abs(mx), abs(my));
	off %= uint2(size);

	pixelCoord += -sign(int2(mx, my)) * int2(off);

	[unroll] for(int i = 0; i < 2; ++i)
	{
		if(pixelCoord[i] < 0) pixelCoord[i] += size[i];
		if(pixelCoord[i] >= size[i]) pixelCoord[i] -= size[i];
	}

	return src_image[pixelCoord];
}