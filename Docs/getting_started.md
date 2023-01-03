# Getting Started Guide

## Overview

* [Import Images](#import-images)
* [Compare Images](#compare-images)
* [Image Equations](#image-equations)
* [Export](#export)
* [Filter](#filter)
* [Statistics](#statistics) (MSE, SSIM)
* [Videos](#videos)
* [Cubemaps and 360 degree images](#cubemaps-and-360-degree-images)
* [3D Images](#image-3d)
* [Image Console](#image-console) (batch processing via console)
* [Further Reading](#further-reading)

# Import Images

Images can be imported via File->Import (ctrl+i) in the menu bar or by using drag and drop. 
The imported images will be displayed on the right side of the editor in the "Images" tab:

![](img/gs_import.png)

* A: List of imported images
* B: Position of the mouse cursor in the image
* C: Color at the mouse cursors position (default: linear color values from 0.0 to 1.0). If you want to change the display format to 0-255, go to View->PixelDisplay in the menu and select 'byte (sRGB)'
* D: Zoom level (can be changed with the mouse wheel)

The supported image formats are:
* bmp, jpg, png, tga, gif
* hdr, pfm, exr (import only)
* dds, ktx, ktx2

# Compare Images

When you import more than one image, you can compare them side by side in the editor (Images need to be the same size):

![](img/gs_compare.png)

* A: Click the 'eye' icon to toggle the visibility of another image equation. This equation will then be displayed on the right side of the cursor. The first equation will be on the left side of the cursor
* B: Change the image equation to 'i2', 'i3', 'i4' etc. to display a specific image from the import list. Apply the changes by hitting the 'Apply' button (C)
* D: The pixel colors of the second equation will be displayed next to the pixel colors of the first equation

You can compare up to four images in the image viewer. The position of the first, second, third and fourth image will be: top left, top right, bottom left, bottom right (relative to the cursor).
You can also fix the comparison position by double-clicking a position inside the image. This can be undone by double-clicking again.

If you work with .hdr files, you can adjust the exposure with the '+' and '-' keys. The current exposure is displayed in the status bar on the bottom (here: 'x1').

# Image Equations

The image equations can be used for per-pixel calculations. In the following example, the difference of two imported images in visualized:

![](img/gs_equations.png)

Here, the third equation states 'I0-I1' which subtracts the pixel colors of I1 from I0. Alternatively, you can use 'abs(I0 - I1)' to display the absolute error or '(I0-I1)^2' to display the squared error. For more information and examples on the image equations, please refer to the [Equation Manual](equation.md).

Beware of a few things:
* Negative values will be displayed as if they were positive if View->DisplayAbsoluteValues is enabled (default).
* All values will be clamped between 0.0 and 1.0 in the status bar if any of the sRGB display modes are active. The default 'decimal' display mode will show negative numbers.

# Export

Before exporting, make sure that only a single image equation is visible (disable them by hitting the 'eye' symbol).
Select File->Export in the menu bar to start exporting:
1. Select a destination in the "Save as" dialog. Make sure to select the correct file type in the dropdown (.png .hdr etc.)
2. In the "Export" dialog you can select the desired pixel format and enable cropping if desired. The format specifies which components are saved (R=Red, G=Green, B=Blue, A=Alpha). If you work with a colored image without transparency, RGB8_SRGB should be the correct format. If you work with a grayscale image R8_SRGB can be sufficient. If you work with linear data (like normal maps or height fields) you might want to select a different data type (UNorm, SNorm, UFloat, SFloat). If you are not familiar with the different types of images formats you can refer to the [OpenGL wiki](https://www.khronos.org/opengl/wiki/Image_Format).

If you have made minor changes to the input image and you want to overwrite the image without changing the format, you can select the File->Overwrite from the menu bar.

The File->AnimatedDiff feature can generate an animed image comparison. For this, exactly two images need to be visible:

![](img/einstein.gif)

# Filter

Filter are HLSL compute shader that can be imported by the ImageViewer. Only a single function needs to be implemented that will be called for each pixel of the image. User defined parameters can be set from within the GUI. Some filter, like the gaussian blur, are already implemented and can be imported via the filter tab:

![](img/filter.jpg)

* A: switch to the filter tab and open a filter via 'Add Filter'
* B: press 'Apply' to apply the changes
* C: You can change the filter user parameters in the menu below

The equation visibility user parameter can be used to selectively enable/disable a filter for a specific image equation.

For more information on how to write your own filter, please refer to the [Filter Manual](filter_manual.md).

# Statistics

## MSE and RMSE

Accumulated per-pixel statistics can be retrieved from the Statistics tab on the right side:

![](img/gs_statistics.png)

* A: The statistics tab
* B: The type of statistic. Here: Average weights the red, blue and green channel equally with 1/3, as described in the text below C. Luminance would weight the image based on the perceived brightness. More information about the individual types can be found in the [Statistics Manual](statistics.md)
* C: The accumulated per-pixel statistics. Average shows the average pixel color of the image. Min and Max are the minimum and maximum values of the image.

This example shows how to calculate the *mean squared error (MSE)* of the two einstein images from the [Image Equations](#image-equations) section above. Here, the image equation (Equation 1) was set to `(i0 - i1)^2` to obtain the squared distance of each pixel. The MSE can then be read from the 'Average' box in the statistics tab (here: `0.00156`). The *root mean squared error (RMSE)* can be read from the 'Root Average' box.

## SSIM

The [Structural Similarity (SSIM) index](https://www.cns.nyu.edu/pub/eero/wang03-reprint.pdf) can be determined in the statistics tab as well:

![](img/gs_ssim.png)

* A: The statistics tab
* B: Select 'SSIM' here
* C: Select the two images to compare (equations can also be used).
* D: The average SSIM value is displayed here. The three components of SSIM are shown above (Luminance, Contrast and Structure). The interpretation is shown below:

|SSIM|Interpretation|
|----|--------------|
|1   |Images are identical|
|0   |Images have no relation|
|-1  |Images are inversed|

* E: The per-pixel maps can also be imported into the image list by clicking on the import button next to the statistics value. The SSIM map of the two example images is shown below:



![](img/gs_ssim2.png)

# Videos

Common video files (mp4, mov, mpeg, avi, flv, webm, mpeg, mkv, vmv) and animated gif files can be imported as well. In this case, the files will be interpreted as *texture 2D arrays* which are natively supported by image formats like dds, ktx and ktx2.
After the import, a dialog will pop up, in which you can specify the range of frames that should be imported. Beware that long and high-resolution videos can not be imported as a whole, because the *texture 2D array* does only support up to 2048 layers/frames. Furthermore, each frame will be stored in an uncompressed format so the gpu can run out of memory rather quickly.

Tip: Consider using 'Frame skip' in the import dialog, if your video is too long or you run out of gpu memory. This setting will only import every Xth frame.

An example of the video interface is shown below:

![](img/gs_videos.png)

The important controls can be found in the status bar on the bottom. From left to right:
* Configure the playback FPS
* Play and Pause the video
* Go to next frame, go to previous frame
* Jump to a specific frame
* Enable or disable *auto repeat*
* Timestamp and playback bar

All the usual image viewer features are supported as well (import and comparison of multiple videos, custom image equations and filters, etc.)

Videos can be exported into an image format like dds, ktx and ktx2 via File->Export or as .mp4 via File->ExportVideo. However, the sound will be lost after import. Individual frames can also be exported via File->Export (for .png, .hdr etc.).

# Cubemaps and 360 degree images
 
You can open cubemaps in dds and ktx format or open 360° images (Latitude-Longitude maps). Select
'Polar360' to view your 360 degree images in 3D.

![](img/polar.jpg)

![](img/polar2.jpg)

### Lat-Long Cubemap Conversion
Convert between Lat-Long (polar 360 degree) and Cubemaps with `Tools->LatLong to Cubemap` and `Tools->Cubemap to LatLong`. You can create a Cubemap from multiple 2D images with `File->Import as Array`.


# Image 3D

3D images can be displayed as well and are supported by the majority of image viewer features (export, custom filtering, mipmap generation and more). Simple flat shading and transparency rendering is also supported to help visualize certain datasets.

![](img/volume_view.jpg)

Additionally, you can explore the insides of a 3D texture with the 'Slice' feature:

![](img/volume_slice.jpg)

For more on 3D images see [here](volumetric.md).

# Image Console

Next to the ImageViewer.exe there is also an ImageConsole.exe supplied in each release. 
This executable is a console application that includes the most important features of the image viewer.
For example, opening a .bmp file and converting it to a .png file could look like this:

```>> .\ImageConsole.exe -open file.bmp -export file.png RGB8_SRGB```

The '-open' command opens the file.bmp and the '-export' command will export it to file.png with the RGB8_SRGB format.
If you are unsure which formats are supported for a specific file format, you can use the '-tellformats' command:

```
>> .\ImageConsole.exe -tellformats pfm
RGB32_SFLOAT
R32_SFLOAT
```

The above format listed all available formats for the pfm extension. RGB represent red green and blue. 32 is the number of bits per color channel. SFLOAT indicates that each color is represented by a signed floating point. The syntax of the image formats is identical to the OpenGL formats. Please refer to the [OpenGL Wiki](https://www.khronos.org/opengl/wiki/Image_Format) for more information.

The full list of commands can be queried with the '-help' command. At the time of writing, the following commands are available:
```
>> .\ImageConsole.exe -help
Commands:
-addfilter "file1" ["file2" ...]                      adds all filter to the pipeline
-cin                                                  keeps the console open to retrieve commands via cin
-close                                                stops reading from cin
-delete [index]                                       deletes the image with the specified index or all images if no index was specified
-deletefilter [index]                                 deletes the filter with the given index or all filter if no index is given
-deletemipmaps                                        keeps only the most detailed mipmap
-equation "color equation" ["alpha equation"]         sets image combine equations
-export filename gliFormat                            saves the current image with the filename
-exportcrop true/false [xStart yStart xEnd yEnd]      enables/disables export cropping and sets cropping boundaries
-exportlayer layer                                    sets the layer that should be exported. -1 means all layers
-exportmipmap mipmap                                  sets the mipmap that should be exported. -1 means all mipmaps
-exportquality quality                                sets the quality level for jpg exports. Between 1 and 100
-filterparam index "param name" value                 sets the parameter of the filter at index
-genmipmaps                                           (re)generates mipmaps
-help                                                 lists all commands
-move oldIndex newIndex                               moves the image to the given image index
-open "file1" "file2" ...                             imports all filenames as images
-silent                                               disables progress output
-ssim imageId1 imageId2                               prints ssim of the two input images
-stats "min/max/avg" "luminance/luma/avg/lightness"   prints the statistic
-tellalpha                                            prints true if any pixel has alpha that is not 1
-tellfilter                                           prints list of filters
-tellfilterparams index                               prints the filter parameters of the filter at index
-tellformats "file extension"                         prints the available export formats for a specific file extension
-telllayers                                           print number of layers
-tellmipmaps                                          prints number of mipmaps
-tellpixel x y [layer mipmap radius]                  prints the pixel color in linear and in srgb
-tellsize [mipmapIndex]                               prints the width and height of the mipmap
-thumbnail size                                       creates a thumbnail with the specified size and returns byte stream
```

The quotation marks "" indicate the the command needs to be put into quotation marks if it contains whitespaces. The arguments in square brackets [] denote optional arguments. The slash / enumerates lists of possible inputs.

### Batch Processing

The following Python script converts all pfm files from the pictures directory to png files:
```python
import os
ImageConsolePath = "C:/ImageViewer/ImageConsole.exe"
ImageDirectory = "pictures"

files = os.listdir(ImageDirectory)
files = [ImageDirectory + "/" + f for f in files if f.endswith('.pfm')]

for file in files:
	command = ImageConsolePath + " -open \"" + file + "\" -export \"" + file + ".png\" RGB8_SRGB"
	os.system(command)
```

# Further Reading

Detailed descriptions are available for:
* [Image Equations](equation.md) (+ list of all instrinsic functions)
* [Filters](filter_manual.md) (how to write your own filters)
* [Statistics](statistics.md) (MSE, MAE, PSNR, SSIM)
* [Mipmaps](mipmaps.md) (lanczos filter, alpha testing => preserve coverage)
* [3D Images](volumetric.md) (create and view)
* [Overlays](overlays.md) (Zoomboxes and arrows)