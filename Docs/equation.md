# Image Equations

## General Syntax

Image Equations determine how the image will look like before any filter was applied.

The default image equation `I0` references the pixels from the first image. More images can be referenced with `I1`, `I2` and so on. Images can be combined with following operators: `* + - / ^`. Numerical constants and some [predefined constants](#Constants) can be used as well. The image color of each pixel is evaluated as follows:

```c++
color.rgb = ("RGB_Equation").rgb;
color.a = ("A_Equation").a;
```

### Example 1 

`RGB: 0.5 * I0 + I1` results in `color[uv].rgb = 0.5 * I0[uv].rgb + I1[uv].rgb` with `uv` being the texture coordinates.

## Functions

Most of the HLSL functions may be used as well:

* **abs**(value)
* **acos**(value)
* **all**(value)
* **any**(value)
* **asin**(value)
* **atan**(value)
* **atan2**(value1, value2)
* **ceil**(value)
* **clamp**(value, min, max)
* **cos**(value)
* **cross**(value1, value2)*
* **distance**(value1, value2)*
* **dot**(value1, value2)*
* **exp**(value)
* **exp2**(value)
* **floor**(value)
* **fmod**(value1, value2)
* **frac**(value)
* **length**(value)*
* **lerp**(value1, value2, factor)
* [**log**](#Log)(value)
* [**log10**](#Log)(value)
* [**log2**](#Log)(value)
* **max**(value1, value2)
* **min**(value1, value2)
* [**normalize**](#Normalize)(value)*
* [**pow**](#Pow)(value1, value2)
* **radians**(value)
* **sign**(value)
* **sin**(value)
* [**sqrt**](#Sqrt)(value)
* **step**(value1, value2)
* **tan**(value)
* **trunc**(value)

Additionaly you can use:

* **red**(v) - return v.rrrr
* **green**(v) - returns v.gggg
* **blue**(v) - returns v.bbbb
* **alpha**(v) - returns v.aaaa
* **rgb**(v1,v2,v3) - returns float4(v1.r, v2.r, v3.r, 1)
* **toSrgb**(v)* - converts linear to srgb
* **fromSrgb**(v)* - converts srgb to linear
* **srgbAsUnorm**(v)* - reinterprets srgb data as unorm
* **srgbAsSnorm**(v)* - reinterprets srgb data as snorm
* **equal**(v1, v2) - v1 == v2 ? 1 : 0
* **smaller**(v1, v2) - v1 < v2 ? 1 : 0
* **smallereq**(v1, v2) - v1 <= v2 ? 1 : 0
* **bigger**(v1, v2) - v1 > v2 ? 1 : 0
* **biggereq**(v1, v2) - v1 >= v2 ? 1 : 0

Functions marked with * will only use the rgb components for computation and ignore alpha.

### Example 2

`rgb(green(I0), 0, 1)`

* Red = green channel from I0
* Green = 0 for all pixels
* Blue = 1 for all pixels

## Alpha channel

The alpha channel has its own equation and is locked to the alpha channel of the first used image by default. A custom equation can be used by clicking the chains to unlock the alpha equation.



### Example 3
* RGB: 0.5
* A: red(I1)

evaluates to:
```c++
color.rgb = 0.5;
color.a = I1.r; // == float4(I1.r, I1.r, I1.r, I1.r).a
```

# Constants

The following (case insensitive) constants can be used:

|Identifier|Result
|-|-
|pi|3.14159274
|e|2.71828175
|infinity/inf|infinity
|float_max/fmax|3.40282347E+38
|nan|NaN

# Specialized Function Implementations

## Log

**log**(value), **log10**(value), **log2**(value)

|Value|Result
|-|-
|<0|NaN
|0|-infinity
|>0|log*(value)

## Normalize
**normalize**({r,g,b,a})

|Value|Result
|-|-
|{0, 0, 0, a}|{NaN, NaN, NaN, a}
|else|{normalize({r, g, b})), a}

## Pow

**pow**(X, Y) special cases:

|X|Y|Result
|-|-|-
|<0|even| pow(-X, Y)
|<0|odd| -pow(-X, Y)
|<0|other| NaN
|=0|<0| infinity
|=0|=0| NaN
|=0|>0| 0
|>0|<0| 1/pow(X,-Y)
|>0|=0| 1
|>0|>0| pow(X,Y)

## Sqrt

**sqrt**(value)

|Value|Result
|-|-
|<0|NaN
|>=0|sqrt(value)
