#setting title, Flip Cubemaps
#setting description, Flips cubemap faces


float4 filter(int2 pixelCoord, int2 size)
{
	// in case we have multiple cubemaps
	int layerOffset = layer / 6;
	int srcLayer = layer % 6;
	switch(layer)
	{
		case 0:
			srcLayer = 1;
			break;
		case 1:
			srcLayer = 0;
			break;
		case 2:
			srcLayer = 3;
			break;
		case 3:
			srcLayer = 2;
			break;
	}

	// flip x and y coordinates
	pixelCoord.x = size.x - 1 - pixelCoord.x;
	pixelCoord.y = size.y - 1 - pixelCoord.y;

	return src_image_ex.mips[level][int3(pixelCoord, layerOffset + srcLayer)];
}