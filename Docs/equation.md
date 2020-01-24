# Image Equations

## General Syntax

Image Equations determine how the image will look like before any filter was applied.

The default image equation `I0` references the pixels from the first image. More images can be referenced with `I1`, `I2` and so on. Images can be combined with following operators: `* + - / ^`. Numerical constants can be used as well.

### Example 1 

`0.5 * I0 + I1`

This will multiply the pixels from image 0 with 0.5 and then add them to the pixels from image 1.

## Functions

Most of the HLSL functions may be used as well:

* **abs(value)**
* **sin(value)**
* **cos(value)**
* **tan(value)**
* **asin(value)**
* **acos(value)**
* **atan(value)**
* **exp(value)**
* **log(value)**
* **exp2(value)**
* **log2(value)**
* **sqrt(value)**
* **sign(value)**
* **floor(value)**
* **ceil(value)**
* **frac(value)**
* **trunc(value)**
* **normalize(value)***
* **length(value)***
* **all(value)**
* **any(value)**
* **radians(value)**
* **min(value1, value2)**
* **max(value1, value2)**
* **pow(value1, value2)**
* **atan2(value1, value2)**
* **fmod(value1, value2)**
* **step(value1, value2)**
* **cross(value1, value2)***
* **dot(value1, value2)***
* **distance(value1, value2)***
* **lerp(value1, value2, factor)**
* **clamp(value, min, max)**

Additionaly you can use:

* **red(v)** - return v.rrrr
* **green(v)** - returns v.gggg
* **blue(v)** - returns v.bbbb
* **alpha(v)** - returns v.aaaa
* **rgb(v1,v2,v3)** - returns float4(v1.r, v2.r, v3.r, 1)
* **toSrgb(v)*** - converts linear to srgb
* **fromSrgb(v)*** - converts srgb to linear
* **srgbAsUnorm(v)*** - reinterprets srgb data as unorm
* **srgbAsSnorm(v)*** - reinterprets srgb data as snorm
* **equal(v1, v2)** - v1 == v2 ? 1 : 0
* **smaller(v1, v2)** - v1 < v2 ? 1 : 0
* **smallereq(v1, v2)** - v1 <= v2 ? 1 : 0
* **bigger(v1, v2)** - v1 > v2 ? 1 : 0
* **biggereq(v1, v2)** - v1 >= v2 ? 1 : 0

Functions marked with * will only use the rgb components for computation and ignore alpha.

### Example 2

`rgb( I0, 0, 1)`

* Red = Red from I0
* Green = 0 for all pixels
* Blue = 1 for all pixels

