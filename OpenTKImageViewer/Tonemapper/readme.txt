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

----------------------------------------
Inputs and Outputs:
The shader will be called per pixel of the image. The pixel positon can be determined
with: "ivec2 pixelCoord = ivec2(gl_GlobalInvocationID.xy) + pixelOffset;"

Source and Destination Image:

The source image can be accesed via "readonly image2D src_image"
The destination image can be accesed via "writeonly image2D dst_image"

----------------------------------------
Examples:
See gamma.comp for a simple example.
See blur.comp for a simple seperatable shader example (Gaussian Blur) 