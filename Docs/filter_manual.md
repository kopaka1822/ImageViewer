# Filter Manual

Filters are simple HLSL compute shader with a custom entry point. Filters can be written for 2D and 3D images. All filters have the entry point `float4 filter(...)` that will be invoked for each pixel in each layer in each mipmap. If "Gen Mipmaps" is enabled, the filter is only executed on the most detailed mipmap and mipmaps will be regenerated in the end.

### Source Image:

The source image can be accesed via `Texture2D src_image` or `Texture3D src_image`. This is a texture view of the currently processed layer and mipmap. Additionally, you can use the global variables `uint level`, `uint layer`, `uint levels` and `uint layers` to get information about the currently processed mipmap level, layer and total number of mipmap levels or layers. If you need to acces a custom layer/mipmap you can use `Texture2DArray src_image_ex` or `Texture3D src_image_ex` to get the view of the entire resource.
A linear and point `SamplerState` for texture filtering can be accessed via `linearSampler` or `pointSampler` respecitvely (clamped coordinates).

## Additional Preprocessor directives:
### Settings:

**#setting** title, *example title*

Sets the title of the shader that will be displayed in the filter list

**#setting** description, *example description*

Sets the description of the shader that will be displayed in the filter tab

**#setting** type, *filter type*

Specifies what kind of filter function is provided. List of types and resulting filter functions:

| *filter type* | entry point | supports
|-|-|-|
|TEX2D      | `float4 filter(int2 pixel, int2 size)`| 2D
|TEX3D      | `float4 filter(int3 pixel, int3 size)`| 3D
|COLOR          | `float4 filter(float4 pixelColor)` | 2D,3D
|DYNAMIC        | `float4 filter(int3 pixel, int3 size)`| 2D,3D

refer to [*Writing DYNAMIC filter*](#Writing-DYNAMIC-filter) for a detailed explanation on DYNAMIC

**#setting** sepa, *true/false*

Specifies if the shader is a seperatable shader. If sepa is set to true, the shader will be executed one time for each dimension (2 or 3 times). In the first run, the (global) variable `int3 filterDirection` will be set to `int3(1,0,0)`. In the second run, the variable will be set to `int3(0,1,0)`. If we are processing a 3D image, the variable will be set to `int3(0,0,1)` in the third run. The default value is false.

**#setting** iterations, *true/false*

If set to true, the shader will be dispatched multiple times. The (global) variable `uint iteration` contains the number of the current iteration. The function `abort_iterations()` can be called to stop the shader after the current iteration. This setting can not be used together with the sepa setting. The default value is false.

**#setting** groupsize, size

Sets the compute shaders `numthreads(size)`. The default value for 2D and 3D are (2x) **32** and (3x) **8**. It is recommended to set this to a lower number for compute heavy shaders

---

### Parameters:

To set variables from the filter tab, you have to specify parameters.
The syntax is:

**#param** *Displayed Name*, *Variable Name*, *Type*, *DefaultValue* [, *Minimun* [, *Maximum*]]

*Displayed Name*: Will be displayed in the filter tab as variable name.

*Variable Name*: Name of the shader variable.

*Type*: type of the variable. Valid types are: int, float, bool, enum (see below at example).

*DefaultValue*: Initial value of the variable.

*Minimum*: (Optional) Minimum allowed value of the variable.

*Maximum*: (Optional) Maximum allowed value of the variable.

---

Additional properties can be specified via:

**#paramprop** *Displayed Name*, *Action*, ...

*Displayed Name*: Name of the affected parameter (same as **#parameter** name)
*Action*: Event that happened. Currently following actions are defined:
    
- OnAdd: this action will be activated when the up button on the property number box (in the filter tab) is pushed. Aditional parameters are Value, Operation. See Keybindings for an explanation.
    
- OnSubtract: this action will be activated when the down button on the property number box (in the filter tab) is pushed. Aditional parameters are Value, Operation. See Keybindings for an explanation.

***Example:***

`#param Gamma, gma, float, 1.0, 0.0`

`#paramprop Gamma, onAdd, 2.0, multiply`

`float a = pow(0.5, gma); // variable usage`

***Enum Example:***

`#param Type, type, enum {A; B; C}, A`

`if(type == 0) ... // enum usage (A == 0, B == 1, C == 2)`

Note: Do not put '`,`' inside the enum brackets, only '`;`' to separate

---

### Texture Parameters:

The original (imported) images can be accessed by using the **#texture** directive:

**#texture** *Displayed Name*, *Shader Name*

*Displayed Name*: Will be displayed in the filter tab as texture name.

*Shader Name*: Name of the `Texture2D` or `Texture3D` variable that will be provided to access the texture data.  

The desired (imported) texture can be selected in the filter menu.
 
***Example:***

`#texture Normal Texture, NormalTex`

`float4 firstPixel = NormalTex[int2(0, 0)];`

---

## Keybindings

To quickly change parameters within the application you can create keybindings.

**#keybinding** *Displayed Name*, *Keycode*, *Value*, *Operation*

*Displayed Name*: Name of the affected parameter (same as *#parameter* name).

*Keycode*: C# keycode for the corresponding keybinding.

*Value*: (decimal) value to modify the old parameter.

*Operation*: how to modify the parameter. Valid types: add, multiply, set.

When pressing the key, the new parameter value will be: parameterValue (operation) Value

***Example:***

`#keybinding Gamma, P, 0.5, multiply`

=> after pressing P the gamma value will be multiplied by 0.5

`#keybinding Gamma, I, 10.0, set`

=> after pressing I the gamma value will be set to 10.0

More examples:

* See gamma.hlsl for a simple example.
* See blur.hlsl for a simple seperatable shader example (Gaussian Blur) 
* See silhouette.hlsl for an example with texture bindings.

## Writing DYNAMIC filter

Dynamic filter work with both, 2D and 3D textures. In order accesss the textures correctly the following helper functions are defined:

|function| 2D return value | 3D ret. value |
|-|-|-|
|`texel(int3 coord)`| `coord.xy` | `coord.xyz`|
|`texel(int3 coord, int layer)`|`coord.xy,layer`|`coord.xyz`|

Additionally you can use `#if2D` and `#if3D` in combination with `#else` and `#endif` for preprocessing.

* See mirror.hlsl for a simple example