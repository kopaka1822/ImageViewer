#include "pch.h"
#include "gli_interface.h"
#include <gli/gli.hpp>
#include <gli/gl.hpp>


std::unique_ptr<image::Image> gli_load(const char* filename)
{
	gli::texture tex(gli::load(filename));
	if (tex.empty())
		throw std::exception("error opening file");
	auto originalFormat = tex.format();

	// convert image format if not compatible
	if(!image::isSupported(tex.format()))
	{
		if (gli::is_compressed(tex.format()))
			throw std::runtime_error("compressed texture formats are not supported yet");

		const auto newFormat = image::getSupportedFormat(tex.format());
		if(tex.base_face() > 1)
		{
			tex = gli::convert(gli::texture2d_array(tex), newFormat);
		}
		else
		{
			tex = gli::convert(gli::texture_cube_array(tex), newFormat);
		}
	}

	std::unique_ptr<image::Image> res = std::make_unique<image::Image>();

	// determine image format
	res->format = tex.format();
	res->original = originalFormat;

	// put layers and faces together (only case that has layers and faces would be of type CUBE_ARRAY)
	res->layer.assign(tex.layers() * tex.faces(), image::Layer());
	size_t outputLayer = 0;
	for (size_t layer = 0; layer < tex.layers(); ++layer)
	{
		for (size_t face = 0; face < tex.faces(); ++face)
		{
			res->layer.at(outputLayer).mipmaps.assign(tex.levels(), image::Mipmap());
			for (size_t mip = 0; mip < tex.levels(); ++mip)
			{
				// fill mipmap
				image::Mipmap& mipmap = res->layer.at(outputLayer).mipmaps.at(mip);
				mipmap.width = tex.extent(mip).x;
				mipmap.height = tex.extent(mip).y;
				mipmap.depth = 1;

				auto data = tex.data(layer, face, mip);
				auto size = tex.size(mip);
				mipmap.bytes.reserve(size);
				mipmap.bytes.assign(reinterpret_cast<char*>(data), reinterpret_cast<char*>(data) + size);
			}
			outputLayer++;
		}
	}

	return res;
}

std::vector<uint32_t> dds_get_export_formats()
{
	return std::vector<uint32_t>{

	// uniform
	gli::format::FORMAT_RG3B2_UNORM_PACK8,
	gli::format::FORMAT_RGBA4_UNORM_PACK16,
	gli::format::FORMAT_BGRA4_UNORM_PACK16,
	gli::format::FORMAT_R5G6B5_UNORM_PACK16,
	gli::format::FORMAT_B5G6R5_UNORM_PACK16,
	gli::format::FORMAT_RGB5A1_UNORM_PACK16,
	gli::format::FORMAT_BGR5A1_UNORM_PACK16,
	gli::format::FORMAT_R8_UNORM_PACK8,
	gli::format::FORMAT_R8_SNORM_PACK8,
	gli::format::FORMAT_RG8_UNORM_PACK8,
	gli::format::FORMAT_RG8_SNORM_PACK8,
	gli::format::FORMAT_RGB8_UNORM_PACK8,
	gli::format::FORMAT_RGB8_SNORM_PACK8,
	gli::format::FORMAT_RGB8_SRGB_PACK8,
	gli::format::FORMAT_BGR8_UNORM_PACK8,
	gli::format::FORMAT_BGR8_SNORM_PACK8,
	gli::format::FORMAT_BGR8_SRGB_PACK8,
	gli::format::FORMAT_RGBA8_UNORM_PACK8,
	gli::format::FORMAT_RGBA8_SNORM_PACK8,
	gli::format::FORMAT_RGBA8_SRGB_PACK8,
	gli::format::FORMAT_BGRA8_UNORM_PACK8,
	gli::format::FORMAT_BGRA8_SNORM_PACK8,
	gli::format::FORMAT_BGRA8_SRGB_PACK8,
	gli::format::FORMAT_RGBA8_UNORM_PACK32,
	gli::format::FORMAT_RGBA8_SNORM_PACK32,
	gli::format::FORMAT_RGBA8_SRGB_PACK32,
	gli::format::FORMAT_RGB10A2_UNORM_PACK32,
	gli::format::FORMAT_BGR10A2_UNORM_PACK32,
	gli::format::FORMAT_BGR10A2_SNORM_PACK32,
	gli::format::FORMAT_A8_UNORM_PACK8,
	gli::format::FORMAT_A16_UNORM_PACK16,
	gli::format::FORMAT_BGR8_UNORM_PACK32,
	gli::format::FORMAT_BGR8_SRGB_PACK32,

	// float formats
	gli::format::FORMAT_R16_UNORM_PACK16,
	gli::format::FORMAT_R16_SNORM_PACK16,
	gli::format::FORMAT_R16_SFLOAT_PACK16,
	gli::format::FORMAT_RG16_UNORM_PACK16,
	gli::format::FORMAT_RG16_SNORM_PACK16,
	gli::format::FORMAT_RG16_SFLOAT_PACK16,
	gli::format::FORMAT_RGB16_UNORM_PACK16,
	gli::format::FORMAT_RGB16_SNORM_PACK16,
	gli::format::FORMAT_RGB16_SFLOAT_PACK16,
	gli::format::FORMAT_RGBA16_UNORM_PACK16,
	gli::format::FORMAT_RGBA16_SNORM_PACK16,
	gli::format::FORMAT_RGBA16_SFLOAT_PACK16,
	gli::format::FORMAT_R32_SFLOAT_PACK32,
	gli::format::FORMAT_RG32_SFLOAT_PACK32,
	gli::format::FORMAT_RGB32_SFLOAT_PACK32,
	gli::format::FORMAT_RGBA32_SFLOAT_PACK32,
	gli::format::FORMAT_RG11B10_UFLOAT_PACK32,
	gli::format::FORMAT_RGB9E5_UFLOAT_PACK32,

	// dds compressed
	/*gli::format::FORMAT_RGB_DXT1_UNORM_BLOCK8,
	gli::format::FORMAT_RGB_DXT1_SRGB_BLOCK8,
	gli::format::FORMAT_RGBA_DXT1_UNORM_BLOCK8,
	gli::format::FORMAT_RGBA_DXT1_SRGB_BLOCK8,
	gli::format::FORMAT_RGBA_DXT3_UNORM_BLOCK16,
	gli::format::FORMAT_RGBA_DXT3_SRGB_BLOCK16,
	gli::format::FORMAT_RGBA_DXT5_UNORM_BLOCK16,
	gli::format::FORMAT_RGBA_DXT5_SRGB_BLOCK16,
	gli::format::FORMAT_R_ATI1N_UNORM_BLOCK8,
	gli::format::FORMAT_R_ATI1N_SNORM_BLOCK8,
	gli::format::FORMAT_RG_ATI2N_UNORM_BLOCK16,
	gli::format::FORMAT_RG_ATI2N_SNORM_BLOCK16,
	gli::format::FORMAT_RGB_BP_UFLOAT_BLOCK16,
	gli::format::FORMAT_RGB_BP_SFLOAT_BLOCK16,
	gli::format::FORMAT_RGBA_BP_UNORM_BLOCK16,
	gli::format::FORMAT_RGBA_BP_SRGB_BLOCK16,
	gli::format::FORMAT_RGB_ETC2_UNORM_BLOCK8,
	gli::format::FORMAT_RGB_ETC2_SRGB_BLOCK8,
	gli::format::FORMAT_RGBA_ETC2_UNORM_BLOCK8,
	gli::format::FORMAT_RGBA_ETC2_SRGB_BLOCK8,
	gli::format::FORMAT_RGBA_ETC2_UNORM_BLOCK16,
	gli::format::FORMAT_RGBA_ETC2_SRGB_BLOCK16,
	gli::format::FORMAT_R_EAC_UNORM_BLOCK8,
	gli::format::FORMAT_R_EAC_SNORM_BLOCK8,
	gli::format::FORMAT_RG_EAC_UNORM_BLOCK16,
	gli::format::FORMAT_RG_EAC_SNORM_BLOCK16,
	gli::format::FORMAT_RGBA_ASTC_4X4_UNORM_BLOCK16,
	gli::format::FORMAT_RGBA_ASTC_4X4_SRGB_BLOCK16,
	gli::format::FORMAT_RGBA_ASTC_5X4_UNORM_BLOCK16,
	gli::format::FORMAT_RGBA_ASTC_5X4_SRGB_BLOCK16,
	gli::format::FORMAT_RGBA_ASTC_5X5_UNORM_BLOCK16,
	gli::format::FORMAT_RGBA_ASTC_5X5_SRGB_BLOCK16,
	gli::format::FORMAT_RGBA_ASTC_6X5_UNORM_BLOCK16,
	gli::format::FORMAT_RGBA_ASTC_6X5_SRGB_BLOCK16,
	gli::format::FORMAT_RGBA_ASTC_6X6_UNORM_BLOCK16,
	gli::format::FORMAT_RGBA_ASTC_6X6_SRGB_BLOCK16,
	gli::format::FORMAT_RGBA_ASTC_8X5_UNORM_BLOCK16,
	gli::format::FORMAT_RGBA_ASTC_8X5_SRGB_BLOCK16,
	gli::format::FORMAT_RGBA_ASTC_8X6_UNORM_BLOCK16,
	gli::format::FORMAT_RGBA_ASTC_8X6_SRGB_BLOCK16,
	gli::format::FORMAT_RGBA_ASTC_8X8_UNORM_BLOCK16,
	gli::format::FORMAT_RGBA_ASTC_8X8_SRGB_BLOCK16,
	gli::format::FORMAT_RGBA_ASTC_10X5_UNORM_BLOCK16,
	gli::format::FORMAT_RGBA_ASTC_10X5_SRGB_BLOCK16,
	gli::format::FORMAT_RGBA_ASTC_10X6_UNORM_BLOCK16,
	gli::format::FORMAT_RGBA_ASTC_10X6_SRGB_BLOCK16,
	gli::format::FORMAT_RGBA_ASTC_10X8_UNORM_BLOCK16,
	gli::format::FORMAT_RGBA_ASTC_10X8_SRGB_BLOCK16,
	gli::format::FORMAT_RGBA_ASTC_10X10_UNORM_BLOCK16,
	gli::format::FORMAT_RGBA_ASTC_10X10_SRGB_BLOCK16,
	gli::format::FORMAT_RGBA_ASTC_12X10_UNORM_BLOCK16,
	gli::format::FORMAT_RGBA_ASTC_12X10_SRGB_BLOCK16,
	gli::format::FORMAT_RGBA_ASTC_12X12_UNORM_BLOCK16,
	gli::format::FORMAT_RGBA_ASTC_12X12_SRGB_BLOCK16,
	gli::format::FORMAT_RGB_PVRTC1_8X8_UNORM_BLOCK32,
	gli::format::FORMAT_RGB_PVRTC1_8X8_SRGB_BLOCK32,
	gli::format::FORMAT_RGB_PVRTC1_16X8_UNORM_BLOCK32,
	gli::format::FORMAT_RGB_PVRTC1_16X8_SRGB_BLOCK32,
	gli::format::FORMAT_RGBA_PVRTC1_8X8_UNORM_BLOCK32,
	gli::format::FORMAT_RGBA_PVRTC1_8X8_SRGB_BLOCK32,
	gli::format::FORMAT_RGBA_PVRTC1_16X8_UNORM_BLOCK32,
	gli::format::FORMAT_RGBA_PVRTC1_16X8_SRGB_BLOCK32,
	gli::format::FORMAT_RGBA_PVRTC2_4X4_UNORM_BLOCK8,
	gli::format::FORMAT_RGBA_PVRTC2_4X4_SRGB_BLOCK8,
	gli::format::FORMAT_RGBA_PVRTC2_8X4_UNORM_BLOCK8,
	gli::format::FORMAT_RGBA_PVRTC2_8X4_SRGB_BLOCK8,
	gli::format::FORMAT_RGB_ETC_UNORM_BLOCK8,
	gli::format::FORMAT_RGB_ATC_UNORM_BLOCK8,
	gli::format::FORMAT_RGBA_ATCA_UNORM_BLOCK16,
	gli::format::FORMAT_RGBA_ATCI_UNORM_BLOCK16,
	gli::format::FORMAT_L8_UNORM_PACK8,
	
	gli::format::FORMAT_LA8_UNORM_PACK8,
	gli::format::FORMAT_L16_UNORM_PACK16,
	gli::format::FORMAT_LA16_UNORM_PACK16,
	*/
	};
}

std::vector<uint32_t> ktx_get_export_formats()
{
	return dds_get_export_formats();
}

void gli_save_image(const char* filename, const image::Image& image, gli::format format, bool ktx)
{
	gli::texture* tex = nullptr;
	gli::texture2d_array tex2d;
	gli::texture_cube texCube;
	bool isCube = image.layer.size() == 6;
	// create texture storage
	if(isCube)
	{
		texCube = gli::texture_cube(image.format,
			gli::extent2d(image.layer[0].mipmaps[0].width, image.layer[0].mipmaps[0].height),
			image.layer[0].mipmaps.size()
		);
		tex = &texCube;
	}
	else
	{
		tex2d = gli::texture2d_array(image.format,
			gli::extent2d(image.layer[0].mipmaps[0].width, image.layer[0].mipmaps[0].height),
			image.layer.size(),
			image.layer[0].mipmaps.size());
		tex = &tex2d;
	}

	// transfer texture data
	for(size_t curLayer = 0; curLayer < image.layer.size(); ++curLayer)
	{
		for(size_t curMip = 0; curMip < image.layer[curLayer].mipmaps.size(); ++curMip)
		{
			auto* dst = tex->data(
				isCube ? 0 : curLayer,
				isCube ? curLayer : 0,
				curMip
			);
			const auto& mip = image.layer[curLayer].mipmaps[curMip];
			if (mip.bytes.size() != tex->size(curMip))
				throw std::runtime_error("mipmap size mipmatch. Expected "
					+ std::to_string(tex->size(curMip)) + " but got " + std::to_string(mip.bytes.size()));

			memcpy(dst, mip.bytes.data(), mip.bytes.size());
		}
	}

	// convert texture if necessary
	if(tex->format() != format)
	{
		if (isCube)
			texCube = gli::convert(texCube, format);
		else
			tex2d = gli::convert(tex2d, format);
	}

	// save
	if (ktx)
		gli::save_ktx(*tex, filename);
	else
		gli::save_dds(*tex, filename);
}