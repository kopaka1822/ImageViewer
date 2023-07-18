#setting sepa, true
#setting title, Dilation
#setting description, Expands a shape with a min or max operation over a rectangular kernel.
#setting type, dynamic

#param Radius, radius, int, 200, 1
#param Operation, operation, enum {Min; Max}, Min

#define OP_MIN 0
#define OP_MAX 1

float4 getPixel(int3 pos, int3 size)
{
	// clamp to border
	pos = clamp(pos, 0, size-1);
	return src_image[texel(pos)];
}

float4 filter(int3 pixelCoord, int3 size)
{
    float4 value = src_image[texel(pixelCoord)];
	
	for(int d = -radius; d <= radius; d++)
    {
        int3 pos = d * filterDirection + pixelCoord;
        float4 newValue = getPixel(pos, size);
		if(operation == OP_MIN)
			value = min(value, newValue);
		else
			value = max(value, newValue);
	}
	return value;
}