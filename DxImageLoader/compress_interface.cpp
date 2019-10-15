#include "pch.h"
#include "compress_interface.h"

#include "../dependencies/compressonator/Compressonator/Header/Compressonator.h"
#include <thread>
#include <stdexcept>

CMP_FORMAT get_cmp_format(gli::format format)
{ 
	switch (format) { 
		// formats used by the exporter
	case gli::format::FORMAT_RGBA8_SRGB_PACK8:
	case gli::format::FORMAT_RGBA8_UNORM_PACK8:
	case gli::format::FORMAT_RGBA8_SNORM_PACK8:
		return CMP_FORMAT_RGBA_8888;
	//case gli::format::FORMAT_RGBA32_SFLOAT_PACK32:
		//return CMP_FORMAT_RGBA_32F; // not supported by converter

		// compressed formats
	case gli::format::FORMAT_RGBA_ASTC_10X10_SRGB_BLOCK16:
	case gli::format::FORMAT_RGBA_ASTC_10X10_UNORM_BLOCK16:
	case gli::format::FORMAT_RGBA_ASTC_10X5_SRGB_BLOCK16:
	case gli::format::FORMAT_RGBA_ASTC_10X5_UNORM_BLOCK16:
	case gli::format::FORMAT_RGBA_ASTC_10X6_SRGB_BLOCK16:
	case gli::format::FORMAT_RGBA_ASTC_10X6_UNORM_BLOCK16:
	case gli::format::FORMAT_RGBA_ASTC_10X8_SRGB_BLOCK16:
	case gli::format::FORMAT_RGBA_ASTC_10X8_UNORM_BLOCK16:
	case gli::format::FORMAT_RGBA_ASTC_12X10_SRGB_BLOCK16:
	case gli::format::FORMAT_RGBA_ASTC_12X10_UNORM_BLOCK16:
	case gli::format::FORMAT_RGBA_ASTC_12X12_SRGB_BLOCK16:
	case gli::format::FORMAT_RGBA_ASTC_12X12_UNORM_BLOCK16:
	case gli::format::FORMAT_RGBA_ASTC_4X4_SRGB_BLOCK16:
	case gli::format::FORMAT_RGBA_ASTC_4X4_UNORM_BLOCK16:
	case gli::format::FORMAT_RGBA_ASTC_5X4_SRGB_BLOCK16:
	case gli::format::FORMAT_RGBA_ASTC_5X4_UNORM_BLOCK16:
	case gli::format::FORMAT_RGBA_ASTC_5X5_SRGB_BLOCK16:
	case gli::format::FORMAT_RGBA_ASTC_5X5_UNORM_BLOCK16:
	case gli::format::FORMAT_RGBA_ASTC_6X5_SRGB_BLOCK16:
	case gli::format::FORMAT_RGBA_ASTC_6X5_UNORM_BLOCK16:
	case gli::format::FORMAT_RGBA_ASTC_6X6_SRGB_BLOCK16:
	case gli::format::FORMAT_RGBA_ASTC_6X6_UNORM_BLOCK16:
	case gli::format::FORMAT_RGBA_ASTC_8X5_SRGB_BLOCK16:
	case gli::format::FORMAT_RGBA_ASTC_8X5_UNORM_BLOCK16:
	case gli::format::FORMAT_RGBA_ASTC_8X6_SRGB_BLOCK16:
	case gli::format::FORMAT_RGBA_ASTC_8X6_UNORM_BLOCK16:
	case gli::format::FORMAT_RGBA_ASTC_8X8_SRGB_BLOCK16:
	case gli::format::FORMAT_RGBA_ASTC_8X8_UNORM_BLOCK16:
		// TODO remember block dimensions
		return CMP_FORMAT_ASTC;

	case gli::format::FORMAT_R_ATI1N_UNORM_BLOCK8:
		return CMP_FORMAT_ATI1N;
	case gli::format::FORMAT_RG_ATI2N_UNORM_BLOCK16:
		return CMP_FORMAT_ATI2N;

	case gli::format::FORMAT_RGB_ATC_UNORM_BLOCK8:
		return CMP_FORMAT_ATC_RGB;
	case gli::format::FORMAT_RGBA_ATCI_UNORM_BLOCK16:
		return CMP_FORMAT_ATC_RGBA_Interpolated;
	case gli::format::FORMAT_RGBA_ATCA_UNORM_BLOCK16:
		return CMP_FORMAT_ATC_RGBA_Explicit;

	case gli::format::FORMAT_RGBA_DXT1_UNORM_BLOCK8:
	case gli::format::FORMAT_RGBA_DXT1_SRGB_BLOCK8:
		return CMP_FORMAT_DXT1;

	case gli::format::FORMAT_RGBA_DXT3_SRGB_BLOCK16:
	case gli::format::FORMAT_RGBA_DXT3_UNORM_BLOCK16:
		return CMP_FORMAT_DXT3;

	case gli::format::FORMAT_RGBA_DXT5_SRGB_BLOCK16:
	case gli::format::FORMAT_RGBA_DXT5_UNORM_BLOCK16:
		return CMP_FORMAT_DXT5;

	case gli::format::FORMAT_RGB_ETC_UNORM_BLOCK8:
		return CMP_FORMAT_ETC_RGB;

	case gli::format::FORMAT_RGB_ETC2_SRGB_BLOCK8:
		return CMP_FORMAT_ETC2_SRGB;
	case gli::format::FORMAT_RGB_ETC2_UNORM_BLOCK8:
		return CMP_FORMAT_ETC2_RGB;

	case gli::format::FORMAT_RGBA_ETC2_SRGB_BLOCK8:
		return CMP_FORMAT_ETC2_RGBA;

	case gli::format::FORMAT_RGBA_ETC2_UNORM_BLOCK8:
		return CMP_FORMAT_ETC2_SRGBA;
	}

	return CMP_FORMAT_Unknown;
}

void copy_level(const image::Mipmap& src, image::Mipmap& dst, CMP_FORMAT srcFormat, CMP_FORMAT dstFormat, float quality)
{
	dst.width = src.width;
	dst.height = src.height;

	// fill out src texture
	CMP_Texture srcTex;
	srcTex.dwSize = sizeof(srcTex);
	srcTex.dwWidth = src.width;
	srcTex.dwHeight = src.height;
	srcTex.dwPitch = src.bytes.size() / src.height;
	srcTex.dwDataSize = src.bytes.size();
	srcTex.format = srcFormat;
	srcTex.pData = const_cast<CMP_BYTE*>(src.bytes.data());
	srcTex.nBlockWidth = 0;
	srcTex.nBlockHeight = 0;
	srcTex.nBlockDepth = 0;

	// fill out dst texture
	CMP_Texture dstTex;
	dstTex.dwSize = sizeof(srcTex);
	dstTex.dwWidth = src.width;
	dstTex.dwHeight = src.height;
	dstTex.dwPitch = 0; // should not be set for compressed formats
	dstTex.format = dstFormat;
	dstTex.nBlockWidth = 0;
	dstTex.nBlockHeight = 0;
	dstTex.nBlockDepth = 0;
	dstTex.dwDataSize = CMP_CalculateBufferSize(&dstTex);
	dst.bytes.resize(dstTex.dwDataSize);
	dstTex.pData = dst.bytes.data();

	// set compress options
	CMP_CompressOptions options = {};
	options.dwSize = sizeof(options);
	options.fquality = quality;
	static const size_t nThreads = std::thread::hardware_concurrency();
	options.dwnumThreads = nThreads;
	if(dstFormat == CMP_FORMAT_DXT1 || dstFormat == CMP_FORMAT_BC1)
		options.bDXT1UseAlpha = true;

	// compress texture
	auto status = CMP_ConvertTexture(&srcTex, &dstTex, &options, nullptr, 0, 0);
	if (status != CMP_OK)
		throw std::runtime_error("texture compression failed");
}

image::Image compressonator_convert_image(const image::Image& image, gli::format format, int quality)
{
	image::Image res;
	res.format = format;
	res.original = format;
	// create layer
	res.layer.resize(image.layer.size());

	const auto srcFormat = get_cmp_format(image.format);
	const auto dstFormat = get_cmp_format(format);
	const float fquality = quality / 100.0f;

	for(size_t layer = 0; layer < image.layer.size(); ++layer)
	{
		const auto& srcMips = image.layer[layer].mipmaps;

		// create mipmap levels
		res.layer[layer].mipmaps.resize(srcMips.size());
		auto& dstMips = res.layer[layer].mipmaps;

		// copy mipmap levels
		for(size_t mipmap = 0; mipmap < srcMips.size(); ++mipmap)
		{
			copy_level(srcMips[mipmap], dstMips[mipmap], srcFormat, dstFormat, fquality);
		}
	}

	return res;
}

bool is_compressonator_format(gli::format format)
{
	auto conv = get_cmp_format(format);
	if (conv == CMP_FORMAT_Unknown || conv == CMP_FORMAT_RGBA_8888 || conv == CMP_FORMAT_RGBA_32F)
		return false; // only compressed formats should be processed

	return true;
}
