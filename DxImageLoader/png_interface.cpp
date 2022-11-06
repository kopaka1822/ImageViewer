#include "pch.h"
#include "png_interface.h"
#include "../dependencies/libpng/png.h"
#include <stdexcept>
#include <unordered_map>
#include <string>
#include "convert.h"
#include <algorithm>
#include "interface.h"

struct ImportFormatInfo
{
	uint32_t width;
	uint32_t height;
	int bitDepth;
	int colorType;
	bool isSrgb = false;
	gli::format original = gli::format::FORMAT_UNDEFINED;
	gli::format staging = gli::format::FORMAT_UNDEFINED;
	uint32_t pixelSize = 0; // in bytes
};

void complete_import_info(ImportFormatInfo& info)
{
	const uint32_t nonPalettedType = info.colorType & ~PNG_COLOR_MASK_PALETTE;
	if(info.bitDepth <= 8)
	{
		if (info.isSrgb) info.staging = gli::format::FORMAT_RGBA8_SRGB_PACK8;
		else info.staging = gli::format::FORMAT_RGBA8_UNORM_PACK8;

		switch (nonPalettedType)
		{
		case PNG_COLOR_TYPE_GRAY:
			if (info.isSrgb) info.original = gli::format::FORMAT_R8_SRGB_PACK8;
			else info.original = gli::format::FORMAT_R8_UNORM_PACK8;
			info.pixelSize = 1;
			break;
		case PNG_COLOR_TYPE_GRAY_ALPHA:
			if (info.isSrgb) info.original = gli::format::FORMAT_RA8_SRGB_PACK8;
			else info.original = gli::format::FORMAT_RA8_UNORM_PACK8;
			info.pixelSize = 2;
			break;
		case PNG_COLOR_TYPE_RGB:
			if (info.isSrgb) info.original = gli::format::FORMAT_RGB8_SRGB_PACK8;
			else info.original = gli::format::FORMAT_RGB8_UNORM_PACK8;
			info.pixelSize = 3;
			break;
		case PNG_COLOR_TYPE_RGBA:
			if (info.isSrgb) info.original = gli::format::FORMAT_RGBA8_SRGB_PACK8;
			else info.original = gli::format::FORMAT_RGBA8_UNORM_PACK8;
			info.pixelSize = 4;
			break;
		default: throw std::runtime_error("unknown color type");
		}
		return;
	}
	else if(info.bitDepth == 16)
	{
		info.staging = gli::format::FORMAT_RGBA32_SFLOAT_PACK32;
		switch(nonPalettedType)
		{
		case PNG_COLOR_TYPE_GRAY:
			info.original = gli::format::FORMAT_R16_UNORM_PACK16;
			info.pixelSize = 2;
			break;
		case PNG_COLOR_TYPE_GRAY_ALPHA:
			info.original = gli::format::FORMAT_RG16_UNORM_PACK16;
			info.pixelSize = 4;
			break;
		case PNG_COLOR_TYPE_RGB:
			info.original = gli::format::FORMAT_RGB16_UNORM_PACK16;
			info.pixelSize = 6;
			break;
		case PNG_COLOR_TYPE_RGBA:
			info.original = gli::format::FORMAT_RGBA16_UNORM_PACK16;
			info.pixelSize = 8;
			break;
		default: throw std::runtime_error("unknown color type");
		}
		return;
	}
	throw std::runtime_error("invalid bit depth");
}

struct ExportFormatInfo
{
	uint32_t format;
	uint32_t bitDepth;
	uint32_t colorType;
	uint32_t pixelSize; // in bytes
	uint32_t bitmask; // component bit mask => 0b1 = 1 bit gray, 0b1001 => 1 bit gray and alpha
	png_fixed_point gamma;
};

static ExportFormatInfo get_export_info(gli::format format)
{
	ExportFormatInfo i = {};
	const png_fixed_point noGamma = 100000;
	const png_fixed_point srgbGamma = 45455;
	switch (format)
	{
	case gli::format::FORMAT_R8_SRGB_PACK8:
		i.format = PNG_FORMAT_GRAY;
		i.colorType = PNG_COLOR_TYPE_GRAY;
		i.bitDepth = 8;
		i.pixelSize = 1;
		i.bitmask = 1;
		i.gamma = srgbGamma;
		return i;
	case gli::format::FORMAT_RA8_SRGB_PACK8:
		i.format = PNG_FORMAT_GA;
		i.colorType = PNG_COLOR_TYPE_GA;
		i.bitDepth = 8;
		i.pixelSize = 2;
		i.bitmask = 0b1001;
		i.gamma = srgbGamma;
		return i;
	case gli::format::FORMAT_RGB8_SRGB_PACK8:
		i.format = PNG_FORMAT_RGB;
		i.colorType = PNG_COLOR_TYPE_RGB;
		i.bitDepth = 8;
		i.pixelSize = 3;
		i.bitmask = 0b111;
		i.gamma = srgbGamma;
		return i;
	case gli::format::FORMAT_RGBA8_SRGB_PACK8:
		i.format = PNG_FORMAT_RGBA;
		i.colorType = PNG_COLOR_TYPE_RGBA;
		i.bitDepth = 8;
		i.pixelSize = 4;
		i.bitmask = 0b1111;
		i.gamma = srgbGamma;
		return i;

	case gli::format::FORMAT_R8_UNORM_PACK8:
		i.format = PNG_FORMAT_GRAY;
		i.colorType = PNG_COLOR_TYPE_GRAY;
		i.bitDepth = 8;
		i.pixelSize = 1;
		i.bitmask = 0b1;
		i.gamma = noGamma;
		return i;
	case gli::format::FORMAT_RA8_UNORM_PACK8:
		i.format = PNG_FORMAT_GA;
		i.colorType = PNG_COLOR_TYPE_GA;
		i.bitDepth = 8;
		i.pixelSize = 2;
		i.bitmask = 0b1001;
		i.gamma = noGamma;
		return i;
		break;
	case gli::format::FORMAT_RGB8_UNORM_PACK8:
		i.format = PNG_FORMAT_RGB;
		i.colorType = PNG_COLOR_TYPE_RGB;
		i.bitDepth = 8;
		i.pixelSize = 3;
		i.bitmask = 0b111;
		i.gamma = noGamma;
		return i;
	case gli::format::FORMAT_RGBA8_UNORM_PACK8:
		i.format = PNG_FORMAT_RGBA;
		i.colorType = PNG_COLOR_TYPE_RGBA;
		i.bitDepth = 8;
		i.pixelSize = 4;
		i.bitmask = 0b1111;
		i.gamma = noGamma;
		return i;

	case gli::format::FORMAT_R16_UNORM_PACK16:
		i.format = PNG_FORMAT_LINEAR_Y;
		i.colorType = PNG_COLOR_TYPE_GRAY;
		i.bitDepth = 16;
		i.pixelSize = 2;
		i.bitmask = 0b11;
		i.gamma = noGamma;
		return i;
	case gli::format::FORMAT_RA16_UNORM_PACK16:
		i.format = PNG_FORMAT_LINEAR_Y_ALPHA;
		i.colorType = PNG_COLOR_TYPE_GA;
		i.bitDepth = 16;
		i.pixelSize = 4;
		i.bitmask = 0b11000011;
		i.gamma = noGamma;
		return i;
	case gli::format::FORMAT_RGB16_UNORM_PACK16:
		i.format = PNG_FORMAT_LINEAR_RGB;
		i.colorType = PNG_COLOR_TYPE_RGB;
		i.bitDepth = 16;
		i.pixelSize = 6;
		i.bitmask = 0b111111;
		i.gamma = noGamma;
		return i;
	case gli::format::FORMAT_RGBA16_UNORM_PACK16:
		i.format = PNG_FORMAT_LINEAR_RGB_ALPHA;
		i.colorType = PNG_COLOR_TYPE_RGBA;
		i.bitDepth = 16;
		i.pixelSize = 8;
		i.bitmask = 0b11111111;
		i.gamma = noGamma;
		return i;
	}

	throw std::runtime_error("gli export format not supported");
}

void png_error(png_structp pPng, png_const_charp message)
{
	throw std::runtime_error(message);
}

std::vector<uint32_t> png_get_export_formats()
{
	return  {
		// most useful srgb formats
		gli::format::FORMAT_R8_SRGB_PACK8,
		gli::format::FORMAT_RA8_SRGB_PACK8,
		gli::format::FORMAT_RGB8_SRGB_PACK8,
		gli::format::FORMAT_RGBA8_SRGB_PACK8,

		// 8 bit unorm formats (no gamma)
		gli::format::FORMAT_R8_UNORM_PACK8,
		gli::format::FORMAT_RA8_UNORM_PACK8,
		gli::format::FORMAT_RGB8_UNORM_PACK8,
		gli::format::FORMAT_RGBA8_UNORM_PACK8,

		// useful linear formats
		gli::format::FORMAT_R16_UNORM_PACK16,
		gli::format::FORMAT_RA16_UNORM_PACK16,
		gli::format::FORMAT_RGB16_UNORM_PACK16,
		gli::format::FORMAT_RGBA16_UNORM_PACK16
	};
}

static uint32_t s_num_rows = 0;
void png_progress(png_structp pPng, png_uint_32 row, int pass)
{
	set_progress(row * 100 / s_num_rows);
}

std::unique_ptr<image::IImage> png_load(const char* filename)
{
	FILE* fp = fopen(filename, "rb");
	if (!fp)
		throw std::runtime_error("could not open file");

	png_structp pPng = nullptr;
	png_infop pInfo = nullptr;
	std::unique_ptr<image::IImage> res;

	try
	{
		pPng = png_create_read_struct(PNG_LIBPNG_VER_STRING,
			nullptr, png_error, nullptr);
		if (!pPng)
			throw std::runtime_error("could not create read struct");

		pInfo = png_create_info_struct(pPng);
		if (!pInfo)
			throw std::runtime_error("could not create info struct");

		png_init_io(pPng, fp);

		png_set_read_status_fn(pPng, png_progress);

		png_read_info(pPng, pInfo);

		ImportFormatInfo info;
		int interlace, compression, filterMethod;
		png_get_IHDR(pPng, pInfo, &info.width, &info.height, &info.bitDepth, &info.colorType, &interlace, &compression, &filterMethod);

		// set bit depth to at least 8 bit
		png_set_packing(pPng);

		// convert palette to rgb
		if (info.colorType == PNG_COLOR_TYPE_PALETTE)
			png_set_palette_to_rgb(pPng);
		// Expand grayscale images to the full 8 bits from 1, 2 or 4 bits/pixel.
		if (info.colorType == PNG_COLOR_TYPE_GRAY && info.bitDepth < 8)
			png_set_expand_gray_1_2_4_to_8(pPng);
		// Expand paletted or RGB images with transparency to full alpha channels
		// so the data will be available as RGBA quartets.
		if (png_get_valid(pPng, pInfo, PNG_INFO_tRNS) != 0)
			png_set_tRNS_to_alpha(pPng);
		
		// gamma handling
		int srgbIntent;
		double gamma;
		if(png_get_sRGB(pPng, pInfo, &srgbIntent))
		{
			// srgb available
			if (info.bitDepth <= 8)
				info.isSrgb = true; // use srgb as is
			else // convert to linear
				png_set_gamma(pPng, 1.0, PNG_DEFAULT_sRGB);
		}
		else if(png_get_gAMA(pPng, pInfo, &gamma))
		{
			// gamma available
			if (std::abs(gamma - 1.0) < 0.01) {} // keep it linear
			else if (std::abs(gamma - 0.45455) < 0.1 && info.bitDepth <= 8) // keep srgb as is
				info.isSrgb = true;
			else
			{
				// do color conversion
				if(info.bitDepth > 8 || gamma > 0.727275)
				{
					// convert to linear
					png_set_gamma(pPng, 1.0, gamma);
				}
				else // convert to srgb (closer)
				{
					info.isSrgb = true;
					png_set_gamma(pPng, 0.45455, gamma);
				}
			}
		}
		else if(info.bitDepth < 16)
		{
			// assume srgb
			info.isSrgb = true;
		}

		if(info.bitDepth == 16 && image::littleendian())
			png_set_swap(pPng);

		// convert gray to rgb
		if (info.colorType == PNG_COLOR_TYPE_GRAY || info.colorType == PNG_COLOR_TYPE_GRAY_ALPHA)
			png_set_gray_to_rgb(pPng);

		// fill with alpha
		if((info.colorType & PNG_COLOR_MASK_ALPHA) == 0)
			png_set_filler(pPng, 0xFFFF, PNG_FILLER_AFTER);

		png_read_update_info(pPng, pInfo);
		complete_import_info(info);

		// allocate storage
		res.reset(new image::SimpleImage(
			info.original, info.staging,
			info.width, info.height,
			info.bitDepth <= 8 ? 4 : 4 * 4
		));

		std::vector<png_bytep> rows;
		rows.resize(info.height);
		s_num_rows = info.height;
		size_t dataSize;
		auto data = res->getData(0, 0, dataSize);
		auto rowStride = info.width * (info.bitDepth <= 8 ? 4 : 2 * 4);
		for(auto& r:  rows)
		{
			r = data;
			data += rowStride;
		}

		png_read_image(pPng, rows.data());
		png_read_end(pPng, pInfo);

		// fix image stride
		if(info.bitDepth == 16)
		{
			// do conversion to 32 bit (from back to front to do this inplace)
			auto buffer = res->getData(0, 0, dataSize);
			const uint16_t* src = reinterpret_cast<uint16_t*>(buffer) + info.width * info.height * 4 - 1;
			const uint16_t* end = reinterpret_cast<uint16_t*>(buffer) - 1;
			float* dst = reinterpret_cast<float*>(buffer) + info.width * info.height * 4 - 1;
			float invMax = 1.0f / float(std::numeric_limits<uint16_t>::max());

			for (; src != end; --src, --dst)
			{
				*dst = float(*src) * invMax;
			}
		}
	}
	catch (...)
	{
		fclose(fp);
		if(pPng)
		{
			if (pInfo)
				png_destroy_read_struct(&pPng, &pInfo, nullptr);
			else
				png_destroy_read_struct(&pPng, nullptr, nullptr);
		}
		throw;
	}

	png_destroy_read_struct(&pPng, &pInfo, nullptr);
	fclose(fp);

	return res;
}

void png_write(image::IImage& image, const char* filename, gli::format format, int quality)
{
	// bit depth info etc.
	const auto info = get_export_info(format);

	if (image.getHeight(0) > PNG_SIZE_MAX / (image.getWidth(0) * info.pixelSize))
		throw std::runtime_error("image too large");

	// write to file
	FILE* fp = fopen(filename, "wb");
	png_structp pPng = nullptr;
	png_infop pInfo = nullptr;
	if (!fp) throw std::runtime_error("cannot open file");

	try
	{
		pPng = png_create_write_struct(PNG_LIBPNG_VER_STRING,
			nullptr, png_error, nullptr);
		if (!pPng)
			throw std::runtime_error("could not create png write struct");

		pInfo = png_create_info_struct(pPng);
		if (!pInfo)
			throw std::runtime_error("could not create info struct");

		s_num_rows = image.getHeight(0);
		png_set_write_status_fn(pPng, png_progress);

		png_init_io(pPng, fp);

		png_set_IHDR(pPng, pInfo, 
			image.getWidth(0), image.getHeight(0),
			info.bitDepth, info.colorType, 
			PNG_INTERLACE_NONE,
			PNG_COMPRESSION_TYPE_BASE, PNG_FILTER_TYPE_BASE
		);
	
		// write gama chunk (if srgb only is not supported)
		png_set_gAMA_fixed(pPng, pInfo, info.gamma);
		if(info.gamma == 45455)
		{
			// explicit srgb chunk (higher precedence than gamma chunk)
			png_set_sRGB(pPng, pInfo, PNG_sRGB_INTENT_PERCEPTUAL);
		}
		
		png_write_info(pPng, pInfo);

		if(info.bitDepth == 16 && image::littleendian())
			png_set_swap(pPng);

		size_t dataSize;
		auto data = image.getData(0, 0, dataSize);

		if(info.bitDepth == 16)
		{
			// transform to 16 bit unorm
			assert(image.getFormat() == gli::format::FORMAT_RGBA32_SFLOAT_PACK32);

			const float* src = reinterpret_cast<float*>(data);
			const float* end = reinterpret_cast<float*>(data + dataSize);
			uint16_t* dst = reinterpret_cast<uint16_t*>(data);

			uint32_t progress = 0;
			uint32_t divisor = std::max<uint32_t>(4 * image.getWidth(0) * image.getHeight(0) / 100, 1);
			for (; src != end; ++src, ++dst)
			{
				*dst = uint16_t(glm::round(glm::clamp(*src, 0.0f, 1.0f) * 65535.0f));
				++progress;

				if (progress % 256 == 0) set_progress(progress / divisor, "converting float to unorm");
			}
				
			// change stride if required
			if (info.bitmask != 0b11111111)
				image::changeStrideEx(data, dataSize / 2, 4 * 2, info.bitmask);
		}
		else if(info.bitmask != 0b1111)
		{
			// change stride
			image::changeStrideEx(data, dataSize, 4, info.bitmask);
		}

		// set row pointers
		std::vector<png_bytep> rows;
		rows.resize(image.getHeight(0));
		const auto rowStride = info.pixelSize * image.getWidth(0);
		for (auto& r : rows)
		{
			r = data;
			data += rowStride;
		}

		png_write_image(pPng, rows.data());
		png_write_end(pPng, pInfo);
	}
	catch(...) // error handling
	{
		fclose(fp);
		if(pPng)
		{
			if(pInfo)
			{
				png_free(pPng, nullptr);
				png_destroy_write_struct(&pPng, &pInfo);
			}
			else
				png_destroy_write_struct(&pPng, nullptr);
		}
		throw;
	}

	// cleanup
	png_free(pPng, nullptr);
	png_destroy_write_struct(&pPng, &pInfo);
	fclose(fp);
}
