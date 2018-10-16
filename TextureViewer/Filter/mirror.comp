#setting title, Mirror
#setting description, Mirrors the image

#param Flip horizontally, flipx, bool, false
#param Flip vertically, flipy, bool, false

void main()
{
	ivec2 pixelCoord = ivec2(gl_GlobalInvocationID.xy) + pixelOffset;

	if(pixelCoord.x < imageSize(dst_image).x && pixelCoord.y < imageSize(dst_image).y)
	{
		vec4 color = texelFetch(src_image, pixelCoord, 0);
		if(flipx) pixelCoord.x = imageSize(dst_image).x - 1 - pixelCoord.x;
		if(flipy) pixelCoord.y = imageSize(dst_image).y - 1 - pixelCoord.y;
		imageStore(dst_image, pixelCoord, color);
	}	
}