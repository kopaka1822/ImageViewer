# Filter Manual

Filters are simple HLSL compute shader with a custom entry point: `float4 filter(int2 pixel, int2 size)`. This function will be called once for each pixel on each layer in each mipmap. 

### Source Image:

The source image can be accesed via `Texture2D src_image`. This is a texture view of the currently processed layer and mipmap. Additionally, you can use the global variables `uint layer` and `uint level` to get information about the currently processed layer or mipmap level.

## Additional Preprocessor directives:
### Settings:

**#setting** title, *example title*

Sets the title of the shader that will be displayed in the filter list

**#setting** description, *example description*

Sets the description of the shader that will be displayed in the filter tab

**#setting** sepa, *true/false*

Specifies if the shader is a seperatable shader. If sepa is set to true, the shader will be executed twice. In the first run, the variable "int2 filterDirection" will be set to int2(1,0). In the second run, the variable will be set to int2(0,1). The default value is false.

---

### Parameters:

To set variables from the filter tab, you have to specify parameters.
The syntax is:

**#param** *Displayed Name*, *Variable Name*, *Type*, *DefaultValue* [, *Minimun* [, *Maximum*]]

*Displayed Name*: Will be displayed in the filter tab as variable name.

*Variable Name*: Name of the shader variable.

*Type*: type of the variable. Valid types are: int, float, bool.

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

---

### Texture Parameters:

The original (imported) images can be accessed by using the **#texture** directive:

**#texture** *Displayed Name*, *Shader Name*

*Displayed Name*: Will be displayed in the filter tab as texture name.

*Shader Name*: Name of the `Texture2D` variable that will be provided to access the texture data.  

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
