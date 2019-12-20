#include "pch.h"
#include "png_interface.h"
#include "../dependencies/libpng/png.h"
#include <stdexcept>
#include <unordered_map>
#include <string>

static const std::unordered_map<uint32_t, gli::format>& get_png_format_lookup()
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
	const auto& m = get_png_format_lookup();
	auto it = m.find(format);
	if (it == m.end())
		throw std::runtime_error("could not convert png format " + std::to_string(format));

	return it->second;
}

static uint32_t get_png_format(gli::format format)
{
	return 0;
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
