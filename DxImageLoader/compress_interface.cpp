#include "pch.h"
#include "compress_interface.h"

#include "../dependencies/compressonator/Compressonator/Header/Compressonator.h"
#include <thread>
#include <stdexcept>

struct ExFormatInfo
{
	bool useDxt1Alpha = false;
	bool isCompressed = true;
	bool swizzleRGB = false;
	CMP_DWORD widthMultiplier = 0; // 0 for compressed formats. width multiplier to get pitch
};

CMP_FORMAT get_cmp_format(gli::format format, ExFormatInfo& exInfo, bool isSource)
{ 
	switch (format) { 
		// formats used by the exporter
	case gli::format::FORMAT_RGBA8_SRGB_PACK8:
	case gli::format::FORMAT_RGBA8_UNORM_PACK8:
	case gli::format::FORMAT_RGBA8_SNORM_PACK8:
		exInfo.isCompressed = false;
		exInfo.widthMultiplier = 4;
		//if (isSource) return CMP_FORMAT_ARGB_8888; // for some reason channels get swizzled
		//if (!isSource) return CMP_FORMAT_BGRA_8888;
		return CMP_FORMAT_RGBA_8888;
	case gli::format::FORMAT_RGBA32_SFLOAT_PACK32:
		exInfo.isCompressed = false;
		exInfo.widthMultiplier = 16;
		return CMP_FORMAT_RGBA_32F; // not supported by converter

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

	case gli::format::FORMAT_RGB_DXT1_UNORM_BLOCK8: // BC 1
	case gli::format::FORMAT_RGB_DXT1_SRGB_BLOCK8:
		if (isSource) exInfo.swizzleRGB = true;
		return CMP_FORMAT_DXT1;

	case gli::format::FORMAT_RGBA_DXT1_UNORM_BLOCK8: // BC 1
	case gli::format::FORMAT_RGBA_DXT1_SRGB_BLOCK8:
		exInfo.useDxt1Alpha = true;
		if (isSource) exInfo.swizzleRGB = true;
		return CMP_FORMAT_DXT1;

	case gli::format::FORMAT_RGBA_DXT3_SRGB_BLOCK16: // BC 2
	case gli::format::FORMAT_RGBA_DXT3_UNORM_BLOCK16:
		if (isSource) exInfo.swizzleRGB = true;
		return CMP_FORMAT_DXT3;

	case gli::format::FORMAT_RGBA_DXT5_SRGB_BLOCK16: // BC 3
	case gli::format::FORMAT_RGBA_DXT5_UNORM_BLOCK16:
		if (isSource) exInfo.swizzleRGB = true;
		return CMP_FORMAT_DXT5;

	case gli::format::FORMAT_R_ATI1N_UNORM_BLOCK8: // BC 4
	case gli::format::FORMAT_R_ATI1N_SNORM_BLOCK8:
		exInfo.swizzleRGB = true;
		return CMP_FORMAT_ATI1N;

	case gli::format::FORMAT_RG_ATI2N_UNORM_BLOCK16: // BC 5
	case gli::format::FORMAT_RG_ATI2N_SNORM_BLOCK16:
		if (isSource) exInfo.swizzleRGB = true;
		//exInfo.swizzleRGB = true;
		//return CMP_FORMAT_ATI2N;
		return CMP_FORMAT_ATI2N_XY;

	case gli::format::FORMAT_RGB_BP_UFLOAT_BLOCK16: // BC 6
		return CMP_FORMAT_BC6H;

	case gli::format::FORMAT_RGB_BP_SFLOAT_BLOCK16: // BC 6
		return CMP_FORMAT_BC6H_SF;

	case gli::format::FORMAT_RGBA_BP_UNORM_BLOCK16: // BC 7
	case gli::format::FORMAT_RGBA_BP_SRGB_BLOCK16:
		return CMP_FORMAT_BC7;

	case gli::format::FORMAT_RGB_ATC_UNORM_BLOCK8:
		return CMP_FORMAT_ATC_RGB;
	case gli::format::FORMAT_RGBA_ATCI_UNORM_BLOCK16:
		return CMP_FORMAT_ATC_RGBA_Interpolated;
	case gli::format::FORMAT_RGBA_ATCA_UNORM_BLOCK16:
		return CMP_FORMAT_ATC_RGBA_Explicit;

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

	exInfo.isCompressed = false;
	return CMP_FORMAT_Unknown;
}

// exchanges R and B channels
void swizzleMipmap(image::Mipmap& mip, CMP_FORMAT format)
{
	assert(format == CMP_FORMAT_RGBA_8888);

	for(auto i = mip.bytes.begin(), end = mip.bytes.end(); i < end; i += 4)
	{
		// change R and B
		std::swap(*i, *(i + 2));
	}
}

void copy_level(image::Mipmap& src, image::Mipmap& dst,
	CMP_FORMAT srcFormat, CMP_FORMAT dstFormat, 
	const ExFormatInfo& srcInfo, const ExFormatInfo& dstInfo, float quality)
{
	dst.width = src.width;
	dst.height = src.height;

	// fill out src texture
	CMP_Texture srcTex;
	srcTex.dwSize = sizeof(srcTex);
	srcTex.dwWidth = src.width;
	srcTex.dwHeight = src.height;
	srcTex.dwPitch = srcInfo.widthMultiplier * srcTex.dwWidth;
	srcTex.dwDataSize = CMP_DWORD(src.bytes.size());
	srcTex.format = srcFormat;
	srcTex.pData = src.bytes.data();
	srcTex.nBlockWidth = 0;
	srcTex.nBlockHeight = 0;
	srcTex.nBlockDepth = 0;
	if (!srcInfo.isCompressed && (srcInfo.swizzleRGB || dstInfo.swizzleRGB))
		swizzleMipmap(src, srcFormat);

	// fill out dst texture
	CMP_Texture dstTex;
	dstTex.dwSize = sizeof(srcTex);
	dstTex.dwWidth = src.width;
	dstTex.dwHeight = src.height;
	dstTex.dwPitch = dstInfo.widthMultiplier * dstTex.dwWidth;
	dstTex.format = dstFormat;
	dstTex.nBlockWidth = 0;
	dstTex.nBlockHeight = 0;
	dstTex.nBlockDepth = 0;
	if (dstInfo.isCompressed) dstTex.dwDataSize = CMP_CalculateBufferSize(&dstTex);
	else dstTex.dwDataSize = dstTex.dwPitch * dstTex.dwHeight;
	dst.bytes.resize(dstTex.dwDataSize);
	dstTex.pData = dst.bytes.data();

	// set compress options
	CMP_CompressOptions options = {};
	options.dwSize = sizeof(options);
	options.fquality = quality;
	static const size_t nThreads = std::thread::hardware_concurrency();
	options.dwnumThreads = CMP_DWORD(nThreads);
	options.bDXT1UseAlpha = srcInfo.useDxt1Alpha || dstInfo.useDxt1Alpha;

	options.bUseGPUCompress = true;
	options.nGPUDecode = CMP_GPUDecode::GPUDecode_DIRECTX;

	// compress texture
	auto status = CMP_ConvertTexture(&srcTex, &dstTex, &options, nullptr, 0, 0);
	if (status != CMP_OK)
		throw std::runtime_error("texture compression failed");

	if (!dstInfo.isCompressed && (srcInfo.swizzleRGB || dstInfo.swizzleRGB))
		swizzleMipmap(dst, dstFormat);
}

std::unique_ptr<image::Image> compressonator_convert_image(image::Image& image, gli::format format, int quality)
{
	auto resPtr = std::make_unique<image::Image>();
	image::Image& res = *resPtr;
	res.format = format;
	res.original = format;
	// create layer
	res.layer.resize(image.layer.size());

	ExFormatInfo srcFormatInfo;
	const auto srcFormat = get_cmp_format(image.format, srcFormatInfo, true);
	ExFormatInfo dstFormatInfo;
	const auto dstFormat = get_cmp_format(format, dstFormatInfo, false);
	const float fquality = quality / 100.0f;

	for(size_t layer = 0; layer < image.layer.size(); ++layer)
	{
		auto& srcMips = image.layer[layer].mipmaps;

		// create mipmap levels
		res.layer[layer].mipmaps.resize(srcMips.size());
		auto& dstMips = res.layer[layer].mipmaps;

		// copy mipmap levels
		for(size_t mipmap = 0; mipmap < srcMips.size(); ++mipmap)
		{
			copy_level(srcMips[mipmap], dstMips[mipmap], srcFormat, dstFormat, srcFormatInfo, dstFormatInfo, fquality);
		}
	}

	return resPtr;
}

bool is_compressonator_format(gli::format format)
{
	ExFormatInfo i;
	auto conv = get_cmp_format(format, i, true);
	return i.isCompressed;
}
