#setting title, Ordered Dithering
#setting description, Displays and image with smaller color depth. Common 16 bit color resolution 32R 64G 32B. Common 8 bit resolution 8R 8G 4B
#setting type, TEX2D

#param Red Resolution, R_RES, int, 32, 2, 256
#param Green Resolution, G_RES, int, 64, 2, 256
#param Blue Resolution, B_RES, int, 32, 2, 256
#param Alpha Resolution, A_RES, int, 256, 2, 256

#param Dither Matrix (nxn) with n=2^x. x, X, int, 2, 1, 5 

// see https://en.wikipedia.org/wiki/Ordered_dithering
// other good reference: https://bisqwit.iki.fi/story/howto/dither/jy/

uint bit_reverse(uint x) { return reversebits(x) >> (32 - 2 * X); }
uint bitwise_xor(uint x, uint y) { return x ^ y; }
uint bit_interleave(uint x, uint y) // x,y should be at most 16 bit
{
	// transform x := x0 x1 x2 x3..., y := y1 y2 y3
	// to x0 y0 x1 y1
	uint res = 0;
	uint mask = 1;
	[unroll]
	for(uint i = 0; i < 16; ++i)
	{
		res |= (x & mask) << i;
		res |= (y & mask) << (i + 1);
		mask = mask << 1;
	}
	
	return res;
}

float4 filter(int2 pixelCoord, int2 size)
{
	float4 srcColor = src_image[pixelCoord];
	srcColor = saturate(srcColor);
	//srcColor.rgb = pow(srcColor.rgb, 1.0 / 2.4);

	// compute dither matrix n
	int n = 2;
	int i;
	for(i = 1; i < X; ++i) n *= 2;

	// dither matrix coordinates
	uint2 ij = uint2(pixelCoord.yx) % uint2(n, n);
	// bitcode range [0, n * n - 1]
	uint bitcode = bit_reverse(bit_interleave(bitwise_xor(ij[0], ij[1]), ij[0]));

	uint4 RES = uint4(R_RES - 1, G_RES - 1, B_RES - 1, A_RES - 1); // inclusive resolution range RES = [0, RGBA_RES - 1]
	
	// quantize source
	float4 fcolor = srcColor * RES; // fcolor range [0, RES] float
	uint4 icolor = (uint4)(fcolor); // icolor range [0, RES] unsigned int

	float4 err = fcolor - icolor; // err range [0, 1)
	// remove gamma from error threshold 
	//err.rgb = pow(max(err.rgb, 0.0), 2.4);

	// multiply by bitcode range
	uint4 ierr = (uint4)(err * n * n); // err range [0, n * n)

	icolor += (uint4)(ierr > bitcode); // add + 1 if bigger than bitcode threshold
	// convert back
	fcolor = (float4)icolor / (float4)RES;
	//fcolor.rgb = pow(fcolor.rgb, 2.4);

	return fcolor;
}