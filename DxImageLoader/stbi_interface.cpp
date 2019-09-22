#include "pch.h"
#include "stbi_interface.h"

#define STB_IMAGE_IMPLEMENTATION
#define STB_IMAGE_WRITE_IMPLEMENTATION
#include "../dependencies/stb_image.h"
#include "../dependencies/stb_image_write.h"
#include <fstream>

DXGI_FORMAT getDxFormat(int nComponents, bool isFloat, bool& isSrgb, bool& hasAlpha)
{
	if (isFloat)
	{
		isSrgb = false;
		hasAlpha = false;
		switch (nComponents)
		{
		case 1:
			return DXGI_FORMAT_R32_FLOAT;
		case 2:
			return DXGI_FORMAT_R32G32_FLOAT;
		case 3:
			return DXGI_FORMAT_R32G32B32_FLOAT;
		case 4:
			hasAlpha = true;
			return DXGI_FORMAT_R32G32B32A32_FLOAT;
		}
	}
	else
	{
		// TODO let the user decide if srgb conversion should be done
		isSrgb = true;
		hasAlpha = false;

		switch (nComponents)
		{
		case 1:			
			return DXGI_FORMAT_R8_UNORM;
		case 2:
			return DXGI_FORMAT_R8G8_UNORM;
		case 3:
			return DXGI_FORMAT_R8G8B8A8_UNORM;
		case 4:
			hasAlpha = true;
			return DXGI_FORMAT_R8G8B8A8_UNORM;
		}
	}

	return DXGI_FORMAT_UNKNOWN;
}

std::unique_ptr<image::Image> stb_image_load(const char* filename)
{
	// create resource with one layer
	image::Mipmap mipmap;
	image::Format format;

	//stbi_set_flip_vertically_on_load(true);
	if (stbi_is_hdr(filename))
	{
		// load hdr file
		int nComponents = 0;
		float* data = stbi_loadf(filename, reinterpret_cast<int*>(&mipmap.width),
			reinterpret_cast<int*>(&mipmap.height), &nComponents, 0);
		if (!data)
			throw std::exception("error during reading file");

		format.dxgi = getDxFormat(nComponents, true, format.isSrgb, format.hasAlpha);

		size_t mipSize = mipmap.width * mipmap.height * nComponents * 4;
		mipmap.bytes.resize(mipSize);
		memcpy(mipmap.bytes.data(), data, mipSize);

		stbi_image_free(data);
	}
	else
	{
		// obtain information about number of components
		int nComponents = 0;
		if (!stbi_info(filename, reinterpret_cast<int*>(&mipmap.height), reinterpret_cast<int*>(&mipmap.width), &nComponents))
			throw std::exception("error during reading file");

		// 3 component 8 bit textures are not supported by DXGI => round up to 4 bit
		int reqComponents = nComponents;
		if (reqComponents == 3)
			reqComponents = 4;

		// load ldr file
		stbi_uc* data = stbi_load(filename, reinterpret_cast<int*>(&mipmap.width),
			reinterpret_cast<int*>(&mipmap.height), &nComponents, reqComponents);
		if (!data)
			throw std::exception("error during reading file");

		format.dxgi = getDxFormat(nComponents, false, format.isSrgb, format.hasAlpha);

		size_t mipSize = mipmap.width * mipmap.height * reqComponents;
		mipmap.bytes.resize(mipSize);
		memcpy(mipmap.bytes.data(), data, mipSize);

		stbi_image_free(data);
	}

	// make the resource
	auto res = std::make_unique<image::Image>();
	res->format = format;
	res->layer.emplace_back();
	res->layer[0].mipmaps.push_back(std::move(mipmap));

	return res;
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
