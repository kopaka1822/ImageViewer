How to write Tonemapper

Tonemapper are simple GLSL compute shader without the #version and local_size specification.

----------------------------------------
Additional Preprocessor directives:
---
Settings:

#setting title, example title
Sets the title of the shader that will be displayed in the tonemapper dialog

#setting description, example description
Sets the description of the shader that will be displayed in the tonemapper dialog

#setting sepa, true/false
Specifies if the shader is a seperatable shader. If sepa is set to true, 
the shader will be executed twice. In the first run, the unform variable 
"ivec2 filterDirection" will be set to ivec2(1,0). In the second run, the
variable will be set to ivec2(0,1). The default value is false.

#settings singleinvocation, true/false
Specifies if the shader is called with a single invocation. Set this value to
false if the shader takes several seconds to complete in order to avoid application crashes.
The default value is true.

---
Parameters:

To set uniform variables from the tonemapper dialog, you have to specify parameters.
The syntax is:
#param Displayed Name, Location, Type, DefaultValue [, Minimun [, Maximum]]
Displayed Name: Will be displayed in the tonemapper dialog as variable name.
Location: Location of the uniform variable.
Type: type of the variable. Valid types are: Int, Float, Bool.
DefaultValue: Initial value of the variable.
Minimum: (Optional) Minimum allowed value of the variable.
Maximum: (Optional) Maximum allowed value of the variable.

Note: Uniform locations 0 and 1 are reserved and should not be used.

Additional properties can be specified via:
#paramprop Displayed Name, Action, ...
Displayed Name: Name of the affected parameter (same as #parameter name)
Action: Event that happened. Currently following actions are defined:
    
	OnAdd:        this action will be activated when the up button on the property 
	              number box (in the tonampper dialog) is pushed. Aditional parameters
				  are Value, Operation. See Keybindings for an explanation.
    
	onSubtract:   this action will be activated when the down button on the property 
	              number box (in the tonampper dialog) is pushed. Aditional parameters
				  are Value, Operation. See Keybindings for an explanation.

Example:
#paramprop gamma, onAdd, 2.0, multiply

----------------------------------------
Inputs and Outputs:
The shader will be called per pixel of the image. The pixel positon can be determined
with: "ivec2 pixelCoord = ivec2(gl_GlobalInvocationID.xy) + pixelOffset;"


Source and Destination Image:

The source image can be accesed via "uniform sampler2D src_image"
level of detail should be 0 (for texelFetch etc.)
access with texture(...) will give linear interpolated values

The destination image can be accesed via "writeonly image2D dst_image"


Original Images:

The original (imported) images can be accessed via 
"uniform sampler2D textureX" where X is the index of the image
starting with 0. The level of detail should be 0 and using texture(textureX, ...) 
will result in linear interpolated values.
 
Example:
texelFetch(texture2, pixelCoord, 0) can be used to access the 3rd imported image

----------------------------------------
Keybinding:

To quickly change parameters within the application you can create keybindings.

#keybinding Displayed Name, Keycode, Value, Operation
Displayed Name: Name of the affected parameter (same as #parameter name)
Keycode: C# keycode for the corresponding keybinding
Value: (decimal) value to modify the old parameter
Operation: how to modify the parameter. Valid types: add, multiply, set

When pressing the key, the new parameter value will be: parameterValue (operation) Value
Example:
#keybinding gamma, P, 0.5, multiply
=> after pressing P the gamma value will be multiplied by 0.5
#keybinding gamma, I, 10.0, set
=> after pressing I the gamma value will be set to 10.0

Examples:
See gamma.comp for a simple example.
See blur.comp for a simple seperatable shader example (Gaussian Blur) 