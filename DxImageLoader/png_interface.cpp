#include "pch.h"
#include "png_interface.h"
#include "../dependencies/libpng/png.h"
#include <stdexcept>
#include <unordered_map>
#include <string>
#include "convert.h"
#include <algorithm>

static const std::unordered_map<uint32_t, gli::format>& get_png_import_lookup()
{
	static std::unordered_map<uint32_t, gli::format> s_map =
	{
		// single byte sRGB formats TODO add more gli formats
		{PNG_FORMAT_GRAY, gli::format::FORMAT_R8_SRGB_PACK8},
		{PNG_FORMAT_GA, gli::format::FORMAT_RA8_SRGB_PACK8},
		{PNG_FORMAT_AG, gli::format::FORMAT_AR8_SRGB_PACK8},
		{PNG_FORMAT_RGB, gli::format::FORMAT_RGB8_SRGB_PACK8},
		{PNG_FORMAT_BGR, gli::format::FORMAT_BGR8_SRGB_PACK8},
		{PNG_FORMAT_RGBA, gli::format::FORMAT_RGBA8_SRGB_PACK8},
		{PNG_FORMAT_ARGB, gli::format::FORMAT_ARGB8_SRGB_PACK8},
		{PNG_FORMAT_BGRA, gli::format::FORMAT_BGRA8_SRGB_PACK8},
		{PNG_FORMAT_ABGR, gli::format::FORMAT_ABGR8_SRGB_PACK8},

		// linear 2 byte formats
		{PNG_FORMAT_LINEAR_Y, gli::format::FORMAT_R16_UNORM_PACK16},
		{PNG_FORMAT_LINEAR_Y_ALPHA, gli::format::FORMAT_RA16_UNORM_PACK16},
		{PNG_FORMAT_LINEAR_RGB, gli::format::FORMAT_RGB16_UNORM_PACK16},
		{PNG_FORMAT_LINEAR_RGB_ALPHA, gli::format::FORMAT_RGBA16_UNORM_PACK16},

		// colormapped formats
		{PNG_FORMAT_RGB_COLORMAP, gli::format::FORMAT_RGB8_SRGB_PACK8},
		{PNG_FORMAT_BGR_COLORMAP, gli::format::FORMAT_BGR8_SRGB_PACK8},
		{PNG_FORMAT_RGBA_COLORMAP, gli::format::FORMAT_RGBA8_SRGB_PACK8},
		{PNG_FORMAT_ARGB_COLORMAP, gli::format::FORMAT_ARGB8_SRGB_PACK8},
		{PNG_FORMAT_BGRA_COLORMAP, gli::format::FORMAT_BGRA8_SRGB_PACK8},
		{PNG_FORMAT_ABGR_COLORMAP, gli::format::FORMAT_ABGR8_SRGB_PACK8},
	};

	return s_map;
}

static gli::format get_gli_format(uint32_t format)
{
	const auto& m = get_png_import_lookup();
	auto it = m.find(format);
	if (it == m.end())
		throw std::runtime_error("could not convert png format " + std::to_string(format));

	return it->second;
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

static ExportFormatInfo get_png_format(gli::format format)
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

std::unique_ptr<image::IImage> png_load(const char* filename)
{
	png_image img = {};
	img.version = PNG_IMAGE_VERSION;

	if (!png_image_begin_read_from_file(&img, filename))
		throw std::runtime_error(img.message);

	gli::format original = get_gli_format(img.format);
	gli::format gliStageFormat = gli::format::FORMAT_RGBA8_SRGB_PACK8;
	uint32_t stageFormat = PNG_FORMAT_RGBA;
	uint32_t pixelSize = 4;
	const auto isLinear = img.format & PNG_FORMAT_FLAG_LINEAR;
	if(isLinear)
	{
		// take bigger stage target
		gliStageFormat = gli::format::FORMAT_RGBA32_SFLOAT_PACK32;
		stageFormat = PNG_FORMAT_LINEAR_RGB_ALPHA;
		pixelSize = 4 * 4;
	}

	auto res = std::make_unique<image::SimpleImage>(
		original,
		gliStageFormat,
		img.width, img.height, pixelSize);

	img.format = stageFormat;
	auto bufSize = PNG_IMAGE_SIZE(img);
	uint32_t actualBufSize;
	auto buffer = res->getData(0, 0, actualBufSize);
	if (bufSize > actualBufSize)
		throw std::runtime_error("buffer to small");

	if (!png_image_finish_read(&img, nullptr, buffer, 0, nullptr))
		throw std::runtime_error(img.message);

	if(isLinear) // unorm16 => float32
	{
		// do conversion to 32 bit (from back to front to do this inplace)
		const uint16_t* src = reinterpret_cast<uint16_t*>(buffer) + img.width * img.height * 4 - 1;
		const uint16_t* end = reinterpret_cast<uint16_t*>(buffer) - 1;
		float* dst = reinterpret_cast<float*>(buffer) + img.width * img.height * 4 - 1;
		float invMax = 1.0f / float(std::numeric_limits<uint16_t>::max());

		for(;src != end; --src, --dst)
		{
			*dst = float(*src) * invMax;
		}
	}

	return res;
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
		//gli::format::FORMAT_RA8_UNORM_PACK8,
		gli::format::FORMAT_RGB8_UNORM_PACK8,
		gli::format::FORMAT_RGBA8_UNORM_PACK8,

		// useful linear formats
		gli::format::FORMAT_R16_UNORM_PACK16,
		gli::format::FORMAT_RA16_UNORM_PACK16,
		gli::format::FORMAT_RGB16_UNORM_PACK16,
		gli::format::FORMAT_RGBA16_UNORM_PACK16
	};
}

void png_error(png_structp pPng, png_const_charp message)
{
	throw std::runtime_error(message);
}

void png_write(image::IImage& image, const char* filename, gli::format format, int quality)
{
	// bit depth info etc.
	auto info = get_png_format(format);

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

		uint32_t dataSize;
		auto data = image.getData(0, 0, dataSize);

		if(info.bitDepth == 16)
		{
			// transform to 16 bit unorm
			assert(image.getFormat() == gli::format::FORMAT_RGBA32_SFLOAT_PACK32);

			const float* src = reinterpret_cast<float*>(data);
			const float* end = reinterpret_cast<float*>(data + dataSize);
			uint16_t* dst = reinterpret_cast<uint16_t*>(data);

			for (; src != end; ++src, ++dst)
			{
				*dst = uint16_t(glm::round(glm::clamp(*src, 0.0f, 1.0f) * 65535.0f));
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
