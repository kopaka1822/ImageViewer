#setting title, Improves the quality of alpha test
#setting description, Modifies the alpha values of the most-detailled mipmap, so that the mipmaps have a slower rate of degenerating. Mipmaps need to be generated after this filter is executed => If mipmaps are enabled in the viewer, check the Gen Mipmaps box in the Images tab.  Algorithm from https://www.asawicki.info/articles/alpha_test.php5
#setting type, COLOR

//#param Threshold, t, float, 0.5, 0, 1

float4 filter(float4 color)
{
	float4 res = color;
    res.a = max(color.a, color.a / 3.0 + 1.0 / 3.0);
	// general formula
    //res.a = max(color.a, color.a / 3.0 + (2.0 * t) / 3.0);
	return res;
}