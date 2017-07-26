#include "gli_loader.h"
#include <gli/gli.hpp>

bool getImageFormat(ImageFormat& dst, gli::texture& tex)
{
	switch(tex.format())
	{
		// 8 bit types
	case gli::FORMAT_L8_UNORM_PACK8:
	case gli::FORMAT_A8_UNORM_PACK8:
	case gli::FORMAT_R8_UINT_PACK8:
	case gli::FORMAT_R8_UNORM_PACK8: 
		dst.componentSize = 1;
		dst.componentCount = 1;
		dst.componentType = ImageFormat::COMPONENT_TYPE_INT;
		break;
	case gli::FORMAT_RG8_UINT_PACK8:
	case gli::FORMAT_RG8_UNORM_PACK8:
		dst.componentSize = 1;
		dst.componentCount = 2;
		dst.componentType = ImageFormat::COMPONENT_TYPE_INT;
		break;
	case gli::FORMAT_RGB8_UINT_PACK8:
	case gli::FORMAT_RGB8_UNORM_PACK8:
		dst.componentSize = 1;
		dst.componentCount = 3;
		dst.componentType = ImageFormat::COMPONENT_TYPE_INT;
		break;
	case gli::FORMAT_RGBA8_UNORM_PACK8:
	case gli::FORMAT_RGBA8_UINT_PACK8:
		dst.componentSize = 1;
		dst.componentCount = 4;
		dst.componentType = ImageFormat::COMPONENT_TYPE_INT;
		break;

		// 16 bit types
	case gli::FORMAT_R16_UINT_PACK16:
	case gli::FORMAT_R16_UNORM_PACK16:
		dst.componentSize = 2;
		dst.componentCount = 1;
		dst.componentType = ImageFormat::COMPONENT_TYPE_INT;
		break;
	case gli::FORMAT_RG16_UINT_PACK16:
	case gli::FORMAT_RG16_UNORM_PACK16:
		dst.componentSize = 2;
		dst.componentCount = 2;
		dst.componentType = ImageFormat::COMPONENT_TYPE_INT;
		break;
	case gli::FORMAT_RGB16_UINT_PACK16:
	case gli::FORMAT_RGB16_UNORM_PACK16:
		dst.componentSize = 2;
		dst.componentCount = 3;
		dst.componentType = ImageFormat::COMPONENT_TYPE_INT;
		break;
	case gli::FORMAT_RGBA16_UNORM_PACK16:
	case gli::FORMAT_RGBA16_UINT_PACK16:
		dst.componentSize = 2;
		dst.componentCount = 4;
		dst.componentType = ImageFormat::COMPONENT_TYPE_INT;
		break;

		// 32 bit types
	case gli::FORMAT_R32_UINT_PACK32:
		dst.componentSize = 4;
		dst.componentCount = 1;
		dst.componentType = ImageFormat::COMPONENT_TYPE_INT;
		break;
	case gli::FORMAT_RG32_UINT_PACK32:
		dst.componentSize = 4;
		dst.componentCount = 2;
		dst.componentType = ImageFormat::COMPONENT_TYPE_INT;
		break;
	case gli::FORMAT_RGB32_UINT_PACK32:
		dst.componentSize = 4;
		dst.componentCount = 3;
		dst.componentType = ImageFormat::COMPONENT_TYPE_INT;
		break;
	case gli::FORMAT_RGBA32_UINT_PACK32:
		dst.componentSize = 4;
		dst.componentCount = 4;
		dst.componentType = ImageFormat::COMPONENT_TYPE_INT;
		break;

		// floating types
	case gli::FORMAT_R32_SFLOAT_PACK32:
		dst.componentSize = 4;
		dst.componentCount = 1;
		dst.componentType = ImageFormat::COMPONENT_TYPE_FLOAT;
		break;
	case gli::FORMAT_RG32_SFLOAT_PACK32:
		dst.componentSize = 4;
		dst.componentCount = 2;
		dst.componentType = ImageFormat::COMPONENT_TYPE_FLOAT;
		break;
	case gli::FORMAT_RGB32_SFLOAT_PACK32:
		dst.componentSize = 4;
		dst.componentCount = 3;
		dst.componentType = ImageFormat::COMPONENT_TYPE_FLOAT;
		break;
	case gli::FORMAT_RGBA32_SFLOAT_PACK32:
		dst.componentSize = 4;
		dst.componentCount = 4;
		dst.componentType = ImageFormat::COMPONENT_TYPE_FLOAT;
		break;
	case gli::FORMAT_RG4_UNORM_PACK8:
	case gli::FORMAT_RGBA4_UNORM_PACK16:
	case gli::FORMAT_BGRA4_UNORM_PACK16:
	case gli::FORMAT_R5G6B5_UNORM_PACK16:
	case gli::FORMAT_B5G6R5_UNORM_PACK16:
	case gli::FORMAT_RGB5A1_UNORM_PACK16:
	case gli::FORMAT_BGR5A1_UNORM_PACK16:
	case gli::FORMAT_A1RGB5_UNORM_PACK16:
	case gli::FORMAT_R8_SNORM_PACK8: 
	case gli::FORMAT_R8_USCALED_PACK8: 
	case gli::FORMAT_R8_SSCALED_PACK8: 
	case gli::FORMAT_R8_SINT_PACK8: 
	case gli::FORMAT_R8_SRGB_PACK8: 
	case gli::FORMAT_RG8_SNORM_PACK8: 
	case gli::FORMAT_RG8_USCALED_PACK8: 
	case gli::FORMAT_RG8_SSCALED_PACK8: 
	case gli::FORMAT_RG8_SINT_PACK8: 
	case gli::FORMAT_RG8_SRGB_PACK8: 
	case gli::FORMAT_RGB8_SNORM_PACK8: 
	case gli::FORMAT_RGB8_USCALED_PACK8: 
	case gli::FORMAT_RGB8_SSCALED_PACK8: 
	case gli::FORMAT_RGB8_SINT_PACK8: 
	case gli::FORMAT_RGB8_SRGB_PACK8: 
	case gli::FORMAT_BGR8_UNORM_PACK8: 
	case gli::FORMAT_BGR8_SNORM_PACK8: 
	case gli::FORMAT_BGR8_USCALED_PACK8: 
	case gli::FORMAT_BGR8_SSCALED_PACK8: 
	case gli::FORMAT_BGR8_UINT_PACK8: 
	case gli::FORMAT_BGR8_SINT_PACK8: 
	case gli::FORMAT_BGR8_SRGB_PACK8:  
	case gli::FORMAT_RGBA8_SNORM_PACK8: 
	case gli::FORMAT_RGBA8_USCALED_PACK8: 
	case gli::FORMAT_RGBA8_SSCALED_PACK8:  
	case gli::FORMAT_RGBA8_SINT_PACK8: 
	case gli::FORMAT_RGBA8_SRGB_PACK8: 
	case gli::FORMAT_BGRA8_UNORM_PACK8: 
	case gli::FORMAT_BGRA8_SNORM_PACK8: 
	case gli::FORMAT_BGRA8_USCALED_PACK8: 
	case gli::FORMAT_BGRA8_SSCALED_PACK8: 
	case gli::FORMAT_BGRA8_UINT_PACK8: 
	case gli::FORMAT_BGRA8_SINT_PACK8: 
	case gli::FORMAT_BGRA8_SRGB_PACK8: 
	case gli::FORMAT_RGBA8_UNORM_PACK32: 
	case gli::FORMAT_RGBA8_SNORM_PACK32: 
	case gli::FORMAT_RGBA8_USCALED_PACK32: 
	case gli::FORMAT_RGBA8_SSCALED_PACK32: 
	case gli::FORMAT_RGBA8_UINT_PACK32: 
	case gli::FORMAT_RGBA8_SINT_PACK32: 
	case gli::FORMAT_RGBA8_SRGB_PACK32: 
	case gli::FORMAT_RGB10A2_UNORM_PACK32: 
	case gli::FORMAT_RGB10A2_SNORM_PACK32: 
	case gli::FORMAT_RGB10A2_USCALED_PACK32: 
	case gli::FORMAT_RGB10A2_SSCALED_PACK32: 
	case gli::FORMAT_RGB10A2_UINT_PACK32: 
	case gli::FORMAT_RGB10A2_SINT_PACK32: 
	case gli::FORMAT_BGR10A2_UNORM_PACK32: 
	case gli::FORMAT_BGR10A2_SNORM_PACK32: 
	case gli::FORMAT_BGR10A2_USCALED_PACK32: 
	case gli::FORMAT_BGR10A2_SSCALED_PACK32: 
	case gli::FORMAT_BGR10A2_UINT_PACK32: 
	case gli::FORMAT_BGR10A2_SINT_PACK32: 
	case gli::FORMAT_R16_SNORM_PACK16: 
	case gli::FORMAT_R16_USCALED_PACK16: 
	case gli::FORMAT_R16_SSCALED_PACK16: 
	case gli::FORMAT_R16_SINT_PACK16: 
	case gli::FORMAT_R16_SFLOAT_PACK16:  
	case gli::FORMAT_RG16_SNORM_PACK16: 
	case gli::FORMAT_RG16_USCALED_PACK16: 
	case gli::FORMAT_RG16_SSCALED_PACK16: 
	case gli::FORMAT_RG16_SINT_PACK16: 
	case gli::FORMAT_RG16_SFLOAT_PACK16: 
	case gli::FORMAT_RGB16_SNORM_PACK16: 
	case gli::FORMAT_RGB16_USCALED_PACK16: 
	case gli::FORMAT_RGB16_SSCALED_PACK16: 
	case gli::FORMAT_RGB16_SINT_PACK16: 
	case gli::FORMAT_RGB16_SFLOAT_PACK16: 
	case gli::FORMAT_RGBA16_SNORM_PACK16: 
	case gli::FORMAT_RGBA16_USCALED_PACK16: 
	case gli::FORMAT_RGBA16_SSCALED_PACK16: 
	case gli::FORMAT_RGBA16_SINT_PACK16: 
	case gli::FORMAT_RGBA16_SFLOAT_PACK16: 
	case gli::FORMAT_R32_SINT_PACK32: 
	case gli::FORMAT_RG32_SINT_PACK32: 
	case gli::FORMAT_RGB32_SINT_PACK32: 
	case gli::FORMAT_RGBA32_SINT_PACK32: 
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
	case gli::FORMAT_RGB_DXT1_UNORM_BLOCK8: 
	case gli::FORMAT_RGB_DXT1_SRGB_BLOCK8: 
	case gli::FORMAT_RGBA_DXT1_UNORM_BLOCK8: 
	case gli::FORMAT_RGBA_DXT1_SRGB_BLOCK8: 
	case gli::FORMAT_RGBA_DXT3_UNORM_BLOCK16: 
	case gli::FORMAT_RGBA_DXT3_SRGB_BLOCK16: 
	case gli::FORMAT_RGBA_DXT5_UNORM_BLOCK16: 
	case gli::FORMAT_RGBA_DXT5_SRGB_BLOCK16: 
	case gli::FORMAT_R_ATI1N_UNORM_BLOCK8: 
	case gli::FORMAT_R_ATI1N_SNORM_BLOCK8: 
	case gli::FORMAT_RG_ATI2N_UNORM_BLOCK16: 
	case gli::FORMAT_RG_ATI2N_SNORM_BLOCK16: 
	case gli::FORMAT_RGB_BP_UFLOAT_BLOCK16: 
	case gli::FORMAT_RGB_BP_SFLOAT_BLOCK16: 
	case gli::FORMAT_RGBA_BP_UNORM_BLOCK16: 
	case gli::FORMAT_RGBA_BP_SRGB_BLOCK16: 
	case gli::FORMAT_RGB_ETC2_UNORM_BLOCK8: 
	case gli::FORMAT_RGB_ETC2_SRGB_BLOCK8: 
	case gli::FORMAT_RGBA_ETC2_UNORM_BLOCK8: 
	case gli::FORMAT_RGBA_ETC2_SRGB_BLOCK8: 
	case gli::FORMAT_RGBA_ETC2_UNORM_BLOCK16: 
	case gli::FORMAT_RGBA_ETC2_SRGB_BLOCK16: 
	case gli::FORMAT_R_EAC_UNORM_BLOCK8: 
	case gli::FORMAT_R_EAC_SNORM_BLOCK8: 
	case gli::FORMAT_RG_EAC_UNORM_BLOCK16: 
	case gli::FORMAT_RG_EAC_SNORM_BLOCK16: 
	case gli::FORMAT_RGBA_ASTC_4X4_UNORM_BLOCK16: 
	case gli::FORMAT_RGBA_ASTC_4X4_SRGB_BLOCK16: 
	case gli::FORMAT_RGBA_ASTC_5X4_UNORM_BLOCK16: 
	case gli::FORMAT_RGBA_ASTC_5X4_SRGB_BLOCK16: 
	case gli::FORMAT_RGBA_ASTC_5X5_UNORM_BLOCK16: 
	case gli::FORMAT_RGBA_ASTC_5X5_SRGB_BLOCK16: 
	case gli::FORMAT_RGBA_ASTC_6X5_UNORM_BLOCK16: 
	case gli::FORMAT_RGBA_ASTC_6X5_SRGB_BLOCK16: 
	case gli::FORMAT_RGBA_ASTC_6X6_UNORM_BLOCK16: 
	case gli::FORMAT_RGBA_ASTC_6X6_SRGB_BLOCK16: 
	case gli::FORMAT_RGBA_ASTC_8X5_UNORM_BLOCK16: 
	case gli::FORMAT_RGBA_ASTC_8X5_SRGB_BLOCK16: 
	case gli::FORMAT_RGBA_ASTC_8X6_UNORM_BLOCK16: 
	case gli::FORMAT_RGBA_ASTC_8X6_SRGB_BLOCK16: 
	case gli::FORMAT_RGBA_ASTC_8X8_UNORM_BLOCK16: 
	case gli::FORMAT_RGBA_ASTC_8X8_SRGB_BLOCK16: 
	case gli::FORMAT_RGBA_ASTC_10X5_UNORM_BLOCK16: 
	case gli::FORMAT_RGBA_ASTC_10X5_SRGB_BLOCK16: 
	case gli::FORMAT_RGBA_ASTC_10X6_UNORM_BLOCK16: 
	case gli::FORMAT_RGBA_ASTC_10X6_SRGB_BLOCK16: 
	case gli::FORMAT_RGBA_ASTC_10X8_UNORM_BLOCK16: 
	case gli::FORMAT_RGBA_ASTC_10X8_SRGB_BLOCK16: 
	case gli::FORMAT_RGBA_ASTC_10X10_UNORM_BLOCK16: 
	case gli::FORMAT_RGBA_ASTC_10X10_SRGB_BLOCK16: 
	case gli::FORMAT_RGBA_ASTC_12X10_UNORM_BLOCK16: 
	case gli::FORMAT_RGBA_ASTC_12X10_SRGB_BLOCK16: 
	case gli::FORMAT_RGBA_ASTC_12X12_UNORM_BLOCK16: 
	case gli::FORMAT_RGBA_ASTC_12X12_SRGB_BLOCK16: 
	case gli::FORMAT_RGB_PVRTC1_8X8_UNORM_BLOCK32: 
	case gli::FORMAT_RGB_PVRTC1_8X8_SRGB_BLOCK32: 
	case gli::FORMAT_RGB_PVRTC1_16X8_UNORM_BLOCK32: 
	case gli::FORMAT_RGB_PVRTC1_16X8_SRGB_BLOCK32: 
	case gli::FORMAT_RGBA_PVRTC1_8X8_UNORM_BLOCK32: 
	case gli::FORMAT_RGBA_PVRTC1_8X8_SRGB_BLOCK32: 
	case gli::FORMAT_RGBA_PVRTC1_16X8_UNORM_BLOCK32: 
	case gli::FORMAT_RGBA_PVRTC1_16X8_SRGB_BLOCK32: 
	case gli::FORMAT_RGBA_PVRTC2_4X4_UNORM_BLOCK8: 
	case gli::FORMAT_RGBA_PVRTC2_4X4_SRGB_BLOCK8: 
	case gli::FORMAT_RGBA_PVRTC2_8X4_UNORM_BLOCK8: 
	case gli::FORMAT_RGBA_PVRTC2_8X4_SRGB_BLOCK8: 
	case gli::FORMAT_RGB_ETC_UNORM_BLOCK8: 
	case gli::FORMAT_RGB_ATC_UNORM_BLOCK8: 
	case gli::FORMAT_RGBA_ATCA_UNORM_BLOCK16: 
	case gli::FORMAT_RGBA_ATCI_UNORM_BLOCK16: 
	case gli::FORMAT_LA8_UNORM_PACK8: 
	case gli::FORMAT_L16_UNORM_PACK16: 
	case gli::FORMAT_A16_UNORM_PACK16: 
	case gli::FORMAT_LA16_UNORM_PACK16: 
	case gli::FORMAT_BGR8_UNORM_PACK32: 
	case gli::FORMAT_BGR8_SRGB_PACK32: 
	case gli::FORMAT_RG3B2_UNORM_PACK8:
	default:
		return false;
	}
	return true;
}

std::unique_ptr<ImageResource> gli_load(const char* filename)
{
	gli::texture tex = gli::load(filename);
	if (tex.empty())
		return nullptr;

	std::unique_ptr<ImageResource> res = std::make_unique<ImageResource>();
	
	// determine image format
	if (!getImageFormat(res->format, tex))
		return nullptr;

	// add layer
	res->layer.assign(tex.faces(), ImageLayer());
	for(size_t level = 0; level < tex.faces(); ++level)
	{
		// load layer (faces)
		res->layer[level].mipmaps.assign(tex.levels(), ImageMipmap());
		for(size_t mip = 0; mip < tex.levels(); ++mip)
		{
			// fill mipmap
			res->layer[level].mipmaps[mip].width = tex.extent(mip).x;
			res->layer[level].mipmaps[mip].height = tex.extent(mip).y;
			auto data = tex.data(0, level, mip);
			auto size = tex.size(mip);
			res->layer[level].mipmaps[mip].bytes.assign(reinterpret_cast<char*>(data), reinterpret_cast<char*>(data) + size);
		}
	}

	// get data
	return res;
}