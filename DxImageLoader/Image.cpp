#include "pch.h"
#include "Image.h"
#include <algorithm>

size_t image::IImage::calcNumPixels(uint32_t numLayer, uint32_t numLevels, uint32_t width, uint32_t height,
	uint32_t depth)
{
	size_t num = 0;

	for(uint32_t lvl = 0; lvl < numLevels; ++lvl)
	{
		num += size_t(width * height * depth);
		width = std::max(width / 2, 1u);
		height = std::max(height / 2, 1u);
		depth = std::max(depth / 2, 1u);
	}

	return num * numLayer;
}

size_t image::IImage::getNumPixels() const
{
	return calcNumPixels(getNumLayers(), getNumMipmaps(), getWidth(0), getHeight(0), getDepth(0));
}

image::SimpleImage::SimpleImage(gli::format originalFormat, gli::format internalFormat, uint32_t width, uint32_t height,
	uint32_t pixelByteSize)
	:
m_width(width), m_height(height), m_original(originalFormat), m_format(internalFormat)
{
	m_data.resize(size_t(width) * size_t(height) * size_t(pixelByteSize));
}

uint32_t image::pixelSize(gli::format format)
{
	assert(isSupported(format));
	return format == gli::format::FORMAT_RGBA32_SFLOAT_PACK32 ? 16 : 4;
}

gli::format image::getSupportedFormat(gli::format format)
{
	if (isSupported(format)) return format;

	switch (format) { 
		// unorm up to 8 bit
	case gli::FORMAT_RG4_UNORM_PACK8:
	case gli::FORMAT_RGBA4_UNORM_PACK16:
	case gli::FORMAT_BGRA4_UNORM_PACK16: 
	case gli::FORMAT_R5G6B5_UNORM_PACK16:
	case gli::FORMAT_B5G6R5_UNORM_PACK16: 
	case gli::FORMAT_RGB5A1_UNORM_PACK16: 
	case gli::FORMAT_BGR5A1_UNORM_PACK16: 
	case gli::FORMAT_A1RGB5_UNORM_PACK16: 
	case gli::FORMAT_R8_UNORM_PACK8:
	case gli::FORMAT_RG8_UNORM_PACK8:
	case gli::FORMAT_RGB8_UNORM_PACK8:
	case gli::FORMAT_BGR8_UNORM_PACK8:
	case gli::FORMAT_RGBA8_UNORM_PACK8:
	case gli::FORMAT_BGRA8_UNORM_PACK8:
	case gli::FORMAT_RGBA8_UNORM_PACK32:
	case gli::FORMAT_L8_UNORM_PACK8:
	case gli::FORMAT_A8_UNORM_PACK8:
	case gli::FORMAT_BGR8_UNORM_PACK32:
	case gli::FORMAT_RG3B2_UNORM_PACK8:
	// compressed
	case gli::FORMAT_RGB_DXT1_UNORM_BLOCK8:
	case gli::FORMAT_RGBA_DXT1_UNORM_BLOCK8:
	case gli::FORMAT_RGBA_DXT3_UNORM_BLOCK16:
	case gli::FORMAT_RGBA_DXT5_UNORM_BLOCK16:
	case gli::FORMAT_R_ATI1N_UNORM_BLOCK8:
	case gli::FORMAT_RG_ATI2N_UNORM_BLOCK16:
	case gli::FORMAT_RGBA_BP_UNORM_BLOCK16:
	case gli::FORMAT_RGB_ETC2_UNORM_BLOCK8:
	case gli::FORMAT_RGBA_ETC2_UNORM_BLOCK8:
	case gli::FORMAT_RGBA_ETC2_UNORM_BLOCK16:
	case gli::FORMAT_R_EAC_UNORM_BLOCK8:
	case gli::FORMAT_RG_EAC_UNORM_BLOCK16:
	case gli::FORMAT_RGBA_ASTC_4X4_UNORM_BLOCK16:
	case gli::FORMAT_RGBA_ASTC_5X4_UNORM_BLOCK16:
	case gli::FORMAT_RGBA_ASTC_5X5_UNORM_BLOCK16:
	case gli::FORMAT_RGBA_ASTC_6X5_UNORM_BLOCK16:
	case gli::FORMAT_RGBA_ASTC_6X6_UNORM_BLOCK16:
	case gli::FORMAT_RGBA_ASTC_8X5_UNORM_BLOCK16:
	case gli::FORMAT_RGBA_ASTC_8X6_UNORM_BLOCK16:
	case gli::FORMAT_RGBA_ASTC_8X8_UNORM_BLOCK16:
	case gli::FORMAT_RGBA_ASTC_10X5_UNORM_BLOCK16:
	case gli::FORMAT_RGBA_ASTC_10X6_UNORM_BLOCK16:
	case gli::FORMAT_RGBA_ASTC_10X8_UNORM_BLOCK16:
	case gli::FORMAT_RGBA_ASTC_10X10_UNORM_BLOCK16:
	case gli::FORMAT_RGBA_ASTC_12X10_UNORM_BLOCK16:
	case gli::FORMAT_RGBA_ASTC_12X12_UNORM_BLOCK16:
	case gli::FORMAT_RGB_PVRTC1_8X8_UNORM_BLOCK32:
	case gli::FORMAT_RGB_PVRTC1_16X8_UNORM_BLOCK32:
	case gli::FORMAT_RGBA_PVRTC1_8X8_UNORM_BLOCK32:
	case gli::FORMAT_RGBA_PVRTC1_16X8_UNORM_BLOCK32:
	case gli::FORMAT_RGBA_PVRTC2_4X4_UNORM_BLOCK8:
	case gli::FORMAT_RGBA_PVRTC2_8X4_UNORM_BLOCK8:
	case gli::FORMAT_RGB_ETC_UNORM_BLOCK8:
	case gli::FORMAT_RGB_ATC_UNORM_BLOCK8:
	case gli::FORMAT_RGBA_ATCA_UNORM_BLOCK16:
	case gli::FORMAT_RGBA_ATCI_UNORM_BLOCK16:
	case gli::FORMAT_LA8_UNORM_PACK8:
		return gli::FORMAT_RGBA8_UNORM_PACK8;

		// snorm up to 8 bit
	case gli::FORMAT_R8_SNORM_PACK8:
	case gli::FORMAT_RG8_SNORM_PACK8:
	case gli::FORMAT_RGB8_SNORM_PACK8:
	case gli::FORMAT_BGR8_SNORM_PACK8:
	case gli::FORMAT_RGBA8_SNORM_PACK8:
	case gli::FORMAT_BGRA8_SNORM_PACK8:
	case gli::FORMAT_RGBA8_SNORM_PACK32:
	// compressed
	case gli::FORMAT_R_ATI1N_SNORM_BLOCK8:
	case gli::FORMAT_RG_ATI2N_SNORM_BLOCK16:
	case gli::FORMAT_R_EAC_SNORM_BLOCK8:
	case gli::FORMAT_RG_EAC_SNORM_BLOCK16:
		return gli::FORMAT_RGBA8_SNORM_PACK8;

		// srgb
	case gli::FORMAT_R8_SRGB_PACK8:
	case gli::FORMAT_RG8_SRGB_PACK8:
	case gli::FORMAT_RGB8_SRGB_PACK8:
	case gli::FORMAT_BGR8_SRGB_PACK8:
	case gli::FORMAT_RGBA8_SRGB_PACK8:
	case gli::FORMAT_BGRA8_SRGB_PACK8:
	case gli::FORMAT_RGBA8_SRGB_PACK32:
	case gli::FORMAT_BGR8_SRGB_PACK32:
	// compressed
	case gli::FORMAT_RGB_DXT1_SRGB_BLOCK8:
	case gli::FORMAT_RGBA_DXT1_SRGB_BLOCK8:
	case gli::FORMAT_RGBA_DXT3_SRGB_BLOCK16:
	case gli::FORMAT_RGBA_DXT5_SRGB_BLOCK16:
	case gli::FORMAT_RGBA_BP_SRGB_BLOCK16:
	case gli::FORMAT_RGB_ETC2_SRGB_BLOCK8:
	case gli::FORMAT_RGBA_ETC2_SRGB_BLOCK8:
	case gli::FORMAT_RGBA_ETC2_SRGB_BLOCK16:
	case gli::FORMAT_RGBA_ASTC_4X4_SRGB_BLOCK16:
	case gli::FORMAT_RGBA_ASTC_5X4_SRGB_BLOCK16:
	case gli::FORMAT_RGBA_ASTC_5X5_SRGB_BLOCK16:
	case gli::FORMAT_RGBA_ASTC_6X5_SRGB_BLOCK16:
	case gli::FORMAT_RGBA_ASTC_6X6_SRGB_BLOCK16:
	case gli::FORMAT_RGBA_ASTC_8X5_SRGB_BLOCK16:
	case gli::FORMAT_RGBA_ASTC_8X6_SRGB_BLOCK16:
	case gli::FORMAT_RGBA_ASTC_8X8_SRGB_BLOCK16:
	case gli::FORMAT_RGBA_ASTC_10X5_SRGB_BLOCK16:
	case gli::FORMAT_RGBA_ASTC_10X6_SRGB_BLOCK16:
	case gli::FORMAT_RGBA_ASTC_10X8_SRGB_BLOCK16:
	case gli::FORMAT_RGBA_ASTC_10X10_SRGB_BLOCK16:
	case gli::FORMAT_RGBA_ASTC_12X10_SRGB_BLOCK16:
	case gli::FORMAT_RGBA_ASTC_12X12_SRGB_BLOCK16:
	case gli::FORMAT_RGB_PVRTC1_8X8_SRGB_BLOCK32:
	case gli::FORMAT_RGB_PVRTC1_16X8_SRGB_BLOCK32:
	case gli::FORMAT_RGBA_PVRTC1_8X8_SRGB_BLOCK32:
	case gli::FORMAT_RGBA_PVRTC1_16X8_SRGB_BLOCK32:
	case gli::FORMAT_RGBA_PVRTC2_4X4_SRGB_BLOCK8:
	case gli::FORMAT_RGBA_PVRTC2_8X4_SRGB_BLOCK8:
		return gli::FORMAT_RGBA8_SRGB_PACK8;

		// everything else => float target
	case gli::FORMAT_RGB10A2_UNORM_PACK32:
	case gli::FORMAT_BGR10A2_UNORM_PACK32:
	case gli::FORMAT_R16_UNORM_PACK16:
	case gli::FORMAT_RG16_UNORM_PACK16:
	case gli::FORMAT_RGB16_UNORM_PACK16:
	case gli::FORMAT_R8_USCALED_PACK8: 
	case gli::FORMAT_R8_SSCALED_PACK8: 
	case gli::FORMAT_R8_UINT_PACK8: 
	case gli::FORMAT_R8_SINT_PACK8: 
	case gli::FORMAT_RG8_USCALED_PACK8: 
	case gli::FORMAT_RG8_SSCALED_PACK8: 
	case gli::FORMAT_RG8_UINT_PACK8: 
	case gli::FORMAT_RG8_SINT_PACK8: 
	case gli::FORMAT_RGB8_USCALED_PACK8: 
	case gli::FORMAT_RGB8_SSCALED_PACK8: 
	case gli::FORMAT_RGB8_UINT_PACK8: 
	case gli::FORMAT_RGB8_SINT_PACK8: 
	case gli::FORMAT_BGR8_USCALED_PACK8: 
	case gli::FORMAT_BGR8_SSCALED_PACK8: 
	case gli::FORMAT_BGR8_UINT_PACK8: 
	case gli::FORMAT_BGR8_SINT_PACK8: 
	case gli::FORMAT_RGBA8_USCALED_PACK8: 
	case gli::FORMAT_RGBA8_SSCALED_PACK8: 
	case gli::FORMAT_RGBA8_UINT_PACK8: 
	case gli::FORMAT_RGBA8_SINT_PACK8: 
	case gli::FORMAT_BGRA8_USCALED_PACK8: 
	case gli::FORMAT_BGRA8_SSCALED_PACK8: 
	case gli::FORMAT_BGRA8_UINT_PACK8: 
	case gli::FORMAT_BGRA8_SINT_PACK8: 
	case gli::FORMAT_RGBA8_USCALED_PACK32: 
	case gli::FORMAT_RGBA8_SSCALED_PACK32: 
	case gli::FORMAT_RGBA8_UINT_PACK32: 
	case gli::FORMAT_RGBA8_SINT_PACK32: 
	case gli::FORMAT_RGB10A2_SNORM_PACK32: 
	case gli::FORMAT_RGB10A2_USCALED_PACK32: 
	case gli::FORMAT_RGB10A2_SSCALED_PACK32: 
	case gli::FORMAT_RGB10A2_UINT_PACK32: 
	case gli::FORMAT_RGB10A2_SINT_PACK32: 
	case gli::FORMAT_BGR10A2_SNORM_PACK32: 
	case gli::FORMAT_BGR10A2_USCALED_PACK32: 
	case gli::FORMAT_BGR10A2_SSCALED_PACK32: 
	case gli::FORMAT_BGR10A2_UINT_PACK32: 
	case gli::FORMAT_BGR10A2_SINT_PACK32: 
	case gli::FORMAT_R16_SNORM_PACK16: 
	case gli::FORMAT_R16_USCALED_PACK16: 
	case gli::FORMAT_R16_SSCALED_PACK16: 
	case gli::FORMAT_R16_UINT_PACK16: 
	case gli::FORMAT_R16_SINT_PACK16: 
	case gli::FORMAT_R16_SFLOAT_PACK16: 
	case gli::FORMAT_RG16_SNORM_PACK16: 
	case gli::FORMAT_RG16_USCALED_PACK16: 
	case gli::FORMAT_RG16_SSCALED_PACK16: 
	case gli::FORMAT_RG16_UINT_PACK16: 
	case gli::FORMAT_RG16_SINT_PACK16: 
	case gli::FORMAT_RG16_SFLOAT_PACK16: 
	case gli::FORMAT_RGB16_SNORM_PACK16: 
	case gli::FORMAT_RGB16_USCALED_PACK16: 
	case gli::FORMAT_RGB16_SSCALED_PACK16: 
	case gli::FORMAT_RGB16_UINT_PACK16: 
	case gli::FORMAT_RGB16_SINT_PACK16: 
	case gli::FORMAT_RGB16_SFLOAT_PACK16: 
	case gli::FORMAT_RGBA16_UNORM_PACK16:
	case gli::FORMAT_RGBA16_SNORM_PACK16: 
	case gli::FORMAT_RGBA16_USCALED_PACK16: 
	case gli::FORMAT_RGBA16_SSCALED_PACK16: 
	case gli::FORMAT_RGBA16_UINT_PACK16: 
	case gli::FORMAT_RGBA16_SINT_PACK16: 
	case gli::FORMAT_RGBA16_SFLOAT_PACK16: 
	case gli::FORMAT_R32_UINT_PACK32: 
	case gli::FORMAT_R32_SINT_PACK32: 
	case gli::FORMAT_R32_SFLOAT_PACK32: 
	case gli::FORMAT_RG32_UINT_PACK32: 
	case gli::FORMAT_RG32_SINT_PACK32: 
	case gli::FORMAT_RG32_SFLOAT_PACK32: 
	case gli::FORMAT_RGB32_UINT_PACK32: 
	case gli::FORMAT_RGB32_SINT_PACK32: 
	case gli::FORMAT_RGB32_SFLOAT_PACK32: 
	case gli::FORMAT_RGBA32_UINT_PACK32: 
	case gli::FORMAT_RGBA32_SINT_PACK32: 
	case gli::FORMAT_RGBA32_SFLOAT_PACK32: 
	case gli::FORMAT_R64_UINT_PACK64: 
	case gli::FORMAT_R64_SINT_PACK64: 
	case gli::FORMAT_R64_SFLOAT_PACK64: 
	case gli::FORMAT_RG64_UINT_PACK64: 
	case gli::FORMAT_RG64_SINT_PACK64: 
	case gli::FORMAT_RG64_SFLOAT_PACK64: 
	case gli::FORMAT_RGB64_UINT_PACK64: 
	case gli::FORMAT_RGB64_SINT_PACK64: 
	case gli::FORMAT_RGB64_SFLOAT_PACK64: 
	case gli::FORMAT_RGBA64_UINT_PACK64: 
	case gli::FORMAT_RGBA64_SINT_PACK64: 
	case gli::FORMAT_RGBA64_SFLOAT_PACK64: 
	case gli::FORMAT_RG11B10_UFLOAT_PACK32: 
	case gli::FORMAT_RGB9E5_UFLOAT_PACK32: 
	case gli::FORMAT_D16_UNORM_PACK16: 
	case gli::FORMAT_D24_UNORM_PACK32: 
	case gli::FORMAT_D32_SFLOAT_PACK32: 
	case gli::FORMAT_S8_UINT_PACK8: 
	case gli::FORMAT_D16_UNORM_S8_UINT_PACK32: 
	case gli::FORMAT_D24_UNORM_S8_UINT_PACK32: 
	case gli::FORMAT_D32_SFLOAT_S8_UINT_PACK64: 
	case gli::FORMAT_RGB_BP_UFLOAT_BLOCK16: 
	case gli::FORMAT_RGB_BP_SFLOAT_BLOCK16: 
	case gli::FORMAT_L16_UNORM_PACK16: 
	case gli::FORMAT_A16_UNORM_PACK16: 
	case gli::FORMAT_LA16_UNORM_PACK16: 
		return gli::FORMAT_RGBA32_SFLOAT_PACK32;
	}

	return gli::FORMAT_UNDEFINED;
}
