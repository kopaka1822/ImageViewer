# Image Framework Requirements

- [x] open ldr images (png, jpg, bmp)
- [x] open hdr images (hdr, pfm, exr)
- [x] open ktx, dds
- [x] export ldr
- [x] export hdr
- [x] export ktx, dds
- [x] crop image before export
- [x] combine images with custom formula
- [x] use custom shader/filter on each image
- [x] disable filter per image
- [x] custom parameters in filter (int, float, bool, image)
- [x] generate/delete image mipmaps
- [x] get basic image statistics (average, min, max)
- [x] import custom formula as image
- [ ] generate cube map from 6 images
- [x] open compressed formats
- [ ] export compressed formats

# Image Viewer Requirements

- [x] view image slice (1 layer, 1 mipmap)
- [x] view image cubemap (layer == 6)
- [x] view all cubemap layers in one view (layer == 6)
- [x] view polar coordinate maps (layer == 1)
- [x] modify displayed brightness with +/- keys (don't change pixel colors itself)
- [x] show multiple image equations at once (up to 4)
- [x] open multiple windows
- [x] enable/disable linear interpolation for output
- [x] show export crop box
- [x] display texel in linear/srgb/float
- [x] display texel values summed over a radius
- [x] help windows for equations and filter