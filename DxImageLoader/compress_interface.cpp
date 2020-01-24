#include "pch.h"
#include "compress_interface.h"

#include "../dependencies/compressonator/Compressonator/Header/Compressonator.h"
#include <thread>
#include <stdexcept>
#include "interface.h"
#include <algorithm>

struct ExFormatInfo
{
	bool useDxt1Alpha = false;
	bool isCompressed = true;
	bool swizzleRGB = false;
	CMP_BYTE bx = 4;
	CMP_BYTE by = 4;
	CMP_BYTE bz = 1;
	CMP_DWORD widthMultiplier = 0; // 0 for compressed formats. width multiplier to get pitch
};

struct CompressInfo
{
	bool isCompress;
	// progress tracking
	size_t curSteps; // number of steps before this compression
	size_t curStepWeight; // weight of this compression
	size_t numSteps; // total number of steps
};

bool cmp_feedback_proc(float fProgress, CMP_DWORD_PTR pUser1, CMP_DWORD_PTR pUser2)
{
	const CompressInfo* info = reinterpret_cast<CompressInfo*>(pUser1);
	const char* desc = "compressing";
	if (!info->isCompress) desc = "decompressing";

	try
	{
		set_progress(uint32_t((info->curSteps + size_t(fProgress * 0.01f * float(info->curStepWeight))) / info->numSteps), desc);
	}
	catch (const std::exception& e)
	{
		return true;
	}
	
	return false; // don't abort compression
}

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
		exInfo.bx = 10;
		exInfo.by = 10;
		return CMP_FORMAT_ASTC;
	case gli::format::FORMAT_RGBA_ASTC_10X5_SRGB_BLOCK16:
	case gli::format::FORMAT_RGBA_ASTC_10X5_UNORM_BLOCK16:
		exInfo.bx = 10;
		exInfo.by = 5;
		return CMP_FORMAT_ASTC;
	case gli::format::FORMAT_RGBA_ASTC_10X6_SRGB_BLOCK16:
	case gli::format::FORMAT_RGBA_ASTC_10X6_UNORM_BLOCK16:
		exInfo.bx = 10;
		exInfo.by = 6;
		return CMP_FORMAT_ASTC;
	case gli::format::FORMAT_RGBA_ASTC_10X8_SRGB_BLOCK16:
	case gli::format::FORMAT_RGBA_ASTC_10X8_UNORM_BLOCK16:
		exInfo.bx = 10;
		exInfo.by = 8;
		return CMP_FORMAT_ASTC;
	case gli::format::FORMAT_RGBA_ASTC_12X10_SRGB_BLOCK16:
	case gli::format::FORMAT_RGBA_ASTC_12X10_UNORM_BLOCK16:
		exInfo.bx = 12;
		exInfo.by = 10;
		return CMP_FORMAT_ASTC;
	case gli::format::FORMAT_RGBA_ASTC_12X12_SRGB_BLOCK16:
	case gli::format::FORMAT_RGBA_ASTC_12X12_UNORM_BLOCK16:
		exInfo.bx = 12;
		exInfo.by = 12;
		return CMP_FORMAT_ASTC;
	case gli::format::FORMAT_RGBA_ASTC_4X4_SRGB_BLOCK16:
	case gli::format::FORMAT_RGBA_ASTC_4X4_UNORM_BLOCK16:
		exInfo.bx = 4;
		exInfo.by = 4;
		return CMP_FORMAT_ASTC;
	case gli::format::FORMAT_RGBA_ASTC_5X4_SRGB_BLOCK16:
	case gli::format::FORMAT_RGBA_ASTC_5X4_UNORM_BLOCK16:
		exInfo.bx = 5;
		exInfo.by = 4;
		return CMP_FORMAT_ASTC;
	case gli::format::FORMAT_RGBA_ASTC_5X5_SRGB_BLOCK16:
	case gli::format::FORMAT_RGBA_ASTC_5X5_UNORM_BLOCK16:
		exInfo.bx = 5;
		exInfo.by = 5;
		return CMP_FORMAT_ASTC;
	case gli::format::FORMAT_RGBA_ASTC_6X5_SRGB_BLOCK16:
	case gli::format::FORMAT_RGBA_ASTC_6X5_UNORM_BLOCK16:
		exInfo.bx = 6;
		exInfo.by = 5;
		return CMP_FORMAT_ASTC;
	case gli::format::FORMAT_RGBA_ASTC_6X6_SRGB_BLOCK16:
	case gli::format::FORMAT_RGBA_ASTC_6X6_UNORM_BLOCK16:
		exInfo.bx = 6;
		exInfo.by = 6;
		return CMP_FORMAT_ASTC;
	case gli::format::FORMAT_RGBA_ASTC_8X5_SRGB_BLOCK16:
	case gli::format::FORMAT_RGBA_ASTC_8X5_UNORM_BLOCK16:
		exInfo.bx = 8;
		exInfo.by = 5;
		return CMP_FORMAT_ASTC;
	case gli::format::FORMAT_RGBA_ASTC_8X6_SRGB_BLOCK16:
	case gli::format::FORMAT_RGBA_ASTC_8X6_UNORM_BLOCK16:
		exInfo.bx = 8;
		exInfo.by = 6;
		return CMP_FORMAT_ASTC;
	case gli::format::FORMAT_RGBA_ASTC_8X8_SRGB_BLOCK16:
	case gli::format::FORMAT_RGBA_ASTC_8X8_UNORM_BLOCK16:
		exInfo.bx = 8;
		exInfo.by = 8;
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
		if (isSource) exInfo.swizzleRGB = true;
		return CMP_FORMAT_ETC_RGB;

	case gli::format::FORMAT_RGB_ETC2_SRGB_BLOCK8:
		if (isSource) exInfo.swizzleRGB = true;
		return CMP_FORMAT_ETC2_SRGB;
	case gli::format::FORMAT_RGB_ETC2_UNORM_BLOCK8:
		if (isSource) exInfo.swizzleRGB = true;
		return CMP_FORMAT_ETC2_RGB;
	case gli::format::FORMAT_RGBA_ETC2_SRGB_BLOCK8:
		if (isSource) exInfo.swizzleRGB = true;
		return CMP_FORMAT_ETC2_RGBA;
	case gli::format::FORMAT_RGBA_ETC2_UNORM_BLOCK8:
		if (isSource) exInfo.swizzleRGB = true;
		return CMP_FORMAT_ETC2_SRGBA;
	}

	exInfo.isCompressed = false;
	return CMP_FORMAT_Unknown;
}

// exchanges R and B channels
void swizzleMipmap(uint8_t* data, uint32_t size, CMP_FORMAT format)
{
	assert(format == CMP_FORMAT_RGBA_8888);

	for(auto i = data, end = data + size; i < end; i += 4)
	{
		// change R and B
		std::swap(*i, *(i + 2));
	}
}

void copy_level(uint8_t* srcDat, uint8_t* dstDat, uint32_t width, uint32_t height, uint32_t srcSize, uint32_t dstSize,
	CMP_FORMAT srcFormat, CMP_FORMAT dstFormat, 
	const ExFormatInfo& srcInfo, const ExFormatInfo& dstInfo, float quality, CompressInfo& curCompressInfo)
{
	// fill out src texture
	CMP_Texture srcTex;
	srcTex.dwSize = sizeof(srcTex);
	srcTex.dwWidth = width;
	srcTex.dwHeight = height;
	srcTex.dwPitch = srcInfo.widthMultiplier * srcTex.dwWidth;
	srcTex.dwDataSize = CMP_DWORD(srcSize);
	srcTex.format = srcFormat;
	srcTex.pData = srcDat;
	srcTex.nBlockWidth = srcInfo.bx;
	srcTex.nBlockHeight = srcInfo.by;
	srcTex.nBlockDepth = srcInfo.bz;
	if (!srcInfo.isCompressed && (srcInfo.swizzleRGB || dstInfo.swizzleRGB))
		swizzleMipmap(srcDat, srcSize, srcFormat);

	// fill out dst texture
	CMP_Texture dstTex;
	dstTex.dwSize = sizeof(srcTex);
	dstTex.dwWidth = width;
	dstTex.dwHeight = height;
	dstTex.dwPitch = dstInfo.widthMultiplier * dstTex.dwWidth;
	dstTex.format = dstFormat;
	dstTex.nBlockWidth = dstInfo.bx;
	dstTex.nBlockHeight = dstInfo.by;
	dstTex.nBlockDepth = dstInfo.bz;
	if (dstInfo.isCompressed) dstTex.dwDataSize = CMP_CalculateBufferSize(&dstTex);
	else dstTex.dwDataSize = dstTex.dwPitch * dstTex.dwHeight;
	//dst.bytes.resize(dstTex.dwDataSize);
	assert(dstSize >= dstTex.dwDataSize);
	dstTex.pData = dstDat;

	// set compress options
	CMP_CompressOptions options = {};
	options.dwSize = sizeof(options);
	options.fquality = quality;
	static const size_t nThreads = std::thread::hardware_concurrency();
	options.dwnumThreads = CMP_DWORD(nThreads);
	options.bDXT1UseAlpha = srcInfo.useDxt1Alpha || dstInfo.useDxt1Alpha;
	options.nAlphaThreshold = 127;
	options.bUseGPUCompress = true;
	options.bUseGPUDecompress = true;
	options.nComputeWith = CMP_Compute_type::Compute_OPENCL;
	options.nGPUDecode = CMP_GPUDecode::GPUDecode_DIRECTX;

	// compress texture
	auto status = CMP_ConvertTexture(&srcTex, &dstTex, &options, cmp_feedback_proc, reinterpret_cast<size_t>(&curCompressInfo), 0);
	if (status != CMP_OK)
		throw std::runtime_error("texture compression failed");

	if (!dstInfo.isCompressed && (srcInfo.swizzleRGB || dstInfo.swizzleRGB))
		swizzleMipmap(dstDat, dstSize, dstFormat);
}

void compressonator_convert_image(image::IImage& src, image::IImage& dst, int quality)
{
	assert(src.getNumLayers() == dst.getNumLayers());
	assert(src.getNumMipmaps() == dst.getNumMipmaps());
	assert(src.getWidth(0) == dst.getWidth(0));
	assert(src.getHeight(0  ) == dst.getHeight(0));

	ExFormatInfo srcFormatInfo;
	const auto srcFormat = get_cmp_format(src.getFormat(), srcFormatInfo, true);
	ExFormatInfo dstFormatInfo;
	const auto dstFormat = get_cmp_format(dst.getFormat(), dstFormatInfo, false);
	const float fquality = quality / 100.0f;

	CompressInfo info;
	info.isCompress = dstFormatInfo.isCompressed;
	info.numSteps = std::max<size_t>(src.getNumPixels() / 100, 1); // progress range [0, 100]
	info.curSteps = 0;
	for(uint32_t layer = 0; layer < src.getNumLayers(); ++layer)
	{
		// copy mipmap levels
		for(uint32_t mipmap = 0; mipmap < src.getNumMipmaps(); ++mipmap)
		{
			const auto depth = src.getDepth(mipmap);
			const auto width = src.getWidth(mipmap);
			const auto height = src.getHeight(mipmap);
			info.curStepWeight = width * height;

			uint32_t srcSize;
			auto srcDat = src.getData(layer, mipmap, srcSize);
			uint32_t dstSize;
			auto dstDat = dst.getData(layer, mipmap, dstSize);

			auto srcPlaneSize = srcSize / depth;
			auto dstPlaneSize = dstSize / depth;

			for (uint32_t z = 0; z < depth; ++z)
			{
				copy_level(
					srcDat + srcPlaneSize * z,
					dstDat + dstPlaneSize * z,
					width,
					height,
					srcPlaneSize, dstPlaneSize,
					srcFormat, dstFormat,
					srcFormatInfo, dstFormatInfo,
					fquality,
					info
				);

				info.curSteps += info.curStepWeight;;
			}
		}
	}
}

bool is_compressonator_format(gli::format format)
{
	ExFormatInfo i;
	auto conv = get_cmp_format(format, i, true);
	return i.isCompressed;
}
