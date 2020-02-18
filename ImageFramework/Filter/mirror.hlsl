#setting title, Mirror
#setting description, Mirrors the image
#setting type, DYNAMIC

#param Flip horizontally, flipx, bool, false
#param Flip vertically, flipy, bool, false
#param Flip depth, flipz, bool, false

float4 filter(int3 pixelCoord, int3 size)
{
	if(flipx) pixelCoord.x = size.x - 1 - pixelCoord.x;
	if(flipy) pixelCoord.y = size.y - 1 - pixelCoord.y;
	if(flipz) pixelCoord.z = size.z - 1 - pixelCoord.z;

	return src_image[texel(pixelCoord)];
}