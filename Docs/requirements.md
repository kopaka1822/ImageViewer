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
- [ ] import custom formula as image
- [ ] generate cube map from 6 images

# Image Viewer Requirements

- [ ] view image slice (1 layer, 1 mipmap)
- [ ] view image cubemap (layer == 6)
- [ ] view all cubemap layers in one view (layer == 6)
- [ ] view polar coordinate maps (layer == 1)
- [ ] modify displayed brightness with +/- keys (don't change pixel colors itself)
- [ ] show multiple image equations at once (up to 4)
- [ ] open multiple windows
- [ ] enable/disable linear interpolation for output
- [ ] show export crop box
- [ ] display texel in linear/srgb
- [ ] display texel values summed over a radius
- [ ] help windows for equations and filter