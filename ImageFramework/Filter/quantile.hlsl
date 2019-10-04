#setting title, Quantile Filter
#setting description, Computes a sorted list of pixels, removes the 'q' smallest and largest values and computes the average of the remaining. Additionally, only values which are brighter/darker as this average by a factor greater or equal threshold are replaced.
#setting groupsize, 5

#param Radius, RADIUS, Int, 1, 1, 6
#param Quantile, QUANTIL, Int, 2, 1, 84
#param Threshold, THRESHOLD, float, 10.0, 1.0, 1000.0
#paramprop Threshold, onAdd, 2.0, multiply
#paramprop Threshold, onSubtract, 0.5, multiply

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
			
float4 filter(int2 pixelCoord, int2 size)
{
	const float3 threeInv = float3(1.0/3.0, 1.0/3.0, 1.0/3.0);
	float4 buf[169];

	// fill buffer
	uint count = 0;
	for(int y = max(0, pixelCoord.y - RADIUS); y <= min(pixelCoord.y + RADIUS, size.y-1); ++y)
	for(int x = max(0, pixelCoord.x - RADIUS); x <= min(pixelCoord.x + RADIUS, size.x-1); ++x)
	{
		buf[count].rgb = src_image[int2(x,y)].rgb;
		buf[count].w = dot(buf[count].rgb, threeInv);
		// Do not use the sample if it is NaN
		if(buf[count].w == buf[count].w)
			++count;
	}
	
	// sort the buffer
	bubbleSort(count, buf);
	
	// Compute average
	float3 sum = float3(0.0, 0.0, 0.0);
	int q = min((count - 1) / 2, QUANTIL); // Keep at least 1 or 2 elements
	for(uint i = q; i < count-q; ++i)
		sum += buf[i].rgb;
	sum /= max(1, count - q * 2);
	
	float4 centerColor = src_image[pixelCoord];
	float centerAvg = dot(centerColor.rgb, threeInv);
	float sumAvg = dot(sum, threeInv);
	//float factor = min(centerAvg / sumAvg, sumAvg / centerAvg);
	float factor = max(sumAvg / centerAvg, centerAvg / sumAvg);
	if(factor > THRESHOLD || centerAvg != centerAvg)
		centerColor.rgb = sum;
	
	return centerColor;
}