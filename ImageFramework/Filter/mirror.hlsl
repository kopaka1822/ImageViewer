#setting title, Mirror
#setting description, Mirrors the image

#param Flip horizontally, flipx, bool, false
#param Flip vertically, flipy, bool, false

float4 filter(int2 pixelCoord, int2 size)
{
	if(flipx) pixelCoord.x = size.x - 1 - pixelCoord.x;
	if(flipy) pixelCoord.y = size.y - 1 - pixelCoord.y;

	return src_image[pixelCoord];
}