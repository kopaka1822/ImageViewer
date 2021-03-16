#setting title, Divergent
#setting description, Computes the component wise divergent (gradient_x + gradient_y + gradient_z)
#setting type, DYNAMIC

#param Absolute (abs(grad_x) + abs(grad_y)), absGradient, bool, false

float4 filter(int3 pixelCoord, int3 size)
{
	float alpha = src_image[texel(pixelCoord)].a;

	int x = pixelCoord.x;
	int y = pixelCoord.y;
	int z = pixelCoord.z;

	float4 gradX = src_image[texel(int3(min(x + 1, size.x - 1), y, z))] - src_image[texel(int3(max(x - 1, 0), y, z))];
	float4 gradY = src_image[texel(int3(x, min(y + 1, size.y - 1), z))] - src_image[texel(int3(x, max(y - 1, 0), z))];
	float4 gradZ = src_image[texel(int3(x, y, min(z + 1, size.z - 1)))] - src_image[texel(int3(x, y, max(z - 1, 0)))];

	if(absGradient)
	{
		gradX = abs(gradX);
		gradY = abs(gradY);
		gradZ = abs(gradZ);
	}
	return float4((gradX + gradY + gradZ).rgb, alpha);
}