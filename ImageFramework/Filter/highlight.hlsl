#setting title, Highlighting
#setting description, Highlights certain colors. Uses RGB average. NaNs will be yellow.
#setting type, COLOR

#param Negative Values (blue), negative, bool, true
#param Positive Values (red), positive, bool, true
#param Oversaturated Values (green), grOne, bool, false

float4 filter(float4 color)
{
	float average = (color.r + color.g + color.b) / 3.0;
	if( negative && (average < 0.0) )
		color = float4(0.0, 0.0, -average, 1.0);
	if( positive && (average > 0.0) )
		color = float4(average, 0.0, 0.0, 1.0);
	if( grOne && (average > 1.0) )
		color = float4(0.0, average, 0.0, 1.0);

	if(isnan(average))
		color = float4(1.0, 1.0, 0.0, 1.0);

	return color;
}