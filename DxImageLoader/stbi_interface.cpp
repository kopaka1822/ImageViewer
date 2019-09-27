#include "pch.h"
#include "stbi_interface.h"

#define STB_IMAGE_IMPLEMENTATION
#define STB_IMAGE_WRITE_IMPLEMENTATION
#include "../dependencies/stb_image.h"
#include "../dependencies/stb_image_write.h"
#include <fstream>

gli::format getFloatFormat(int numComponents)
{
	switch(numComponents)
	{
	case 1: return gli::FORMAT_R32_SFLOAT_PACK32;
	case 2: return gli::FORMAT_RG32_SFLOAT_PACK32;
	case 3: return gli::FORMAT_RGB32_SFLOAT_PACK32;
	case 4: return gli::FORMAT_RGBA32_SFLOAT_PACK32;
	}
	return gli::FORMAT_UNDEFINED;
}

gli::format getSrgbFormat(int numComponents)
{
	switch(numComponents)
	{
	case 1: return gli::FORMAT_R8_SRGB_PACK8;
	case 2: return gli::FORMAT_RG8_SRGB_PACK8;
	case 3: return gli::FORMAT_RGB8_SRGB_PACK8;
	case 4: return gli::FORMAT_RGBA8_SRGB_PACK8;
	}
	return gli::FORMAT_UNDEFINED;
}

std::unique_ptr<image::Image> stb_image_load(const char* filename)
{
	// create resource with one layer
	auto res = std::make_unique<image::Image>();
	res->layer.emplace_back();
	res->layer[0].mipmaps.emplace_back();
	auto& mipmap = res->layer[0].mipmaps[0];

	//stbi_set_flip_vertically_on_load(true);
	if (stbi_is_hdr(filename))
	{
		// load hdr file
		int nComponents = 0;
		float* data = stbi_loadf(filename, reinterpret_cast<int*>(&mipmap.width),
			reinterpret_cast<int*>(&mipmap.height), &nComponents, 4);
		if (!data)
			throw std::exception("error during reading file");

		res->original = getFloatFormat(nComponents);
		res->format = gli::format::FORMAT_RGBA32_SFLOAT_PACK32;
		size_t mipSize = mipmap.width * mipmap.height * 4 * 4;
		mipmap.bytes.resize(mipSize);
		memcpy(mipmap.bytes.data(), data, mipSize);

		stbi_image_free(data);
	}
	else
	{
		// load ldr file
		int nComponents = 0;
		stbi_uc* data = stbi_load(filename, reinterpret_cast<int*>(&mipmap.width),
			reinterpret_cast<int*>(&mipmap.height), &nComponents, 4);
		if (!data)
			throw std::exception("error during reading file");

		res->original = getSrgbFormat(nComponents);
		res->format = gli::format::FORMAT_RGBA8_SRGB_PACK8;

		size_t mipSize = mipmap.width * mipmap.height * 4;
		mipmap.bytes.resize(mipSize);
		memcpy(mipmap.bytes.data(), data, mipSize);

		stbi_image_free(data);
	}

	return res;
}

std::vector<uint32_t> stb_image_get_export_formats(const char* extension)
{
	auto ext = std::string(extension);
	if (ext == "bmp" || ext == "jpg")
		return std::vector<uint32_t>{
			gli::format::FORMAT_RGB8_SRGB_PACK8,
			gli::format::FORMAT_R8_SRGB_PACK8
		};
	if (ext == "png")
		return std::vector<uint32_t>{
			gli::format::FORMAT_RGBA8_SRGB_PACK8,
			gli::format::FORMAT_RGB8_SRGB_PACK8,
			gli::format::FORMAT_R8_SRGB_PACK8
		};
	if (ext == "hdr")
		return std::vector<uint32_t>{
			gli::format::FORMAT_RGB32_SFLOAT_PACK32,
			gli::format::FORMAT_R32_SFLOAT_PACK32
		};
	return {};
}

void stb_save_png(const char* filename, int width, int height, int components, const void* data)
{
	stbi_write_png_compression_level = 16;
	//stbi_flip_vertically_on_write(1);
	auto res = stbi_write_png(filename, width, height, components, data, width * components);
	if (!res)
		throw std::exception("could not save file");
}

void stb_save_bmp(const char* filename, int width, int height, int components, const void* data)
{
	//stbi_flip_vertically_on_write(1);
	auto res = stbi_write_bmp(filename, width, height, components, data);
	if (!res)
		throw std::exception("could not save file");
}

void stb_save_hdr(const char* filename, int width, int height, int components, const void* data)
{
	//stbi_flip_vertically_on_write(1);
	auto res = stbi_write_hdr(filename, width, height, components, reinterpret_cast<const float*>(data));
	if (!res)
		throw std::exception("could not save file");
}

void stb_save_jpg(const char* filename, int width, int height, int components, const void* data, int quality)
{
	if (quality < 1 || quality > 100)
		throw std::out_of_range("quality must be between 1 and 100");
	auto res = stbi_write_jpg(filename, width, height, components, data, quality);
	if (!res)
		throw std::exception("could not save file");
}
