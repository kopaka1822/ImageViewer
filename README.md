# Image Viewer and Tonemapper

An image viewer for anyone realted to computer graphics.

## File Formats

Currently the following image formats can be imported:
* PNG, JPG, BMP
* HDR, PFM
* KTX, DDS

Exporting is supported for:
* PNG, JPG
* HDR

## View Modes
### Simple Images
View simple images with and without alpha channel. The status bar displays the current texture coordinates (cursor) along with the corresponding RGBA color values.

![alt text](https://github.com/kopaka1822/ImageViewer/blob/master/examples/transparent.png)

### Images with multiple mipmaps and faces

Select preferred mipmap level and layer (face) of DDS and KTX textures:

![alt text](https://github.com/kopaka1822/ImageViewer/blob/master/examples/layer_level_view.png)

View cubemaps in crossview:

![alt text](https://github.com/kopaka1822/ImageViewer/blob/master/examples/cross_view.png)

View and navigate through cubemaps in a projection view:

![alt text](https://github.com/kopaka1822/ImageViewer/blob/master/examples/cube_view.png)

## Polar Images

View the raw polar image:

![alt text](https://github.com/kopaka1822/ImageViewer/blob/master/examples/balcony_raw.png)

View and navigate through the polar image in a projection view:

![alt text](https://github.com/kopaka1822/ImageViewer/blob/master/examples/balcony_polar.png)
