#include "stb_loader.h"
#define STB_IMAGE_IMPLEMENTATION
#define STB_IMAGE_WRITE_IMPLEMENTATION
#include "../dependencies/stb/stb_image.h"
#include <fstream>

#include <gli/gl.hpp>
#include "../dependencies/stb_image_write.h"

uint32_t getDefaultExternalFormat(int nComponents)
{
	switch (nComponents)
	{
	case 1:
		return gli::gl::external_format::EXTERNAL_RED;
	case 2:
		return gli::gl::external_format::EXTERNAL_RG;
	case 3:
		return gli::gl::external_format::EXTERNAL_RGB;
		//return gli::gl::EXTERNAL_SRGB_EXT;
	case 4:
		return gli::gl::external_format::EXTERNAL_RGBA;
		//return gli::gl::external_format::EXTERNAL_SRGB_ALPHA_EXT;
	}
	return uint32_t(-1);
}

uint32_t getSizedInternalFormat(int nComponents, bool isFloat)
{
	if(isFloat)
	{
		switch (nComponents)
		{
		case 1:
			return gli::gl::internal_format::INTERNAL_R32F;
		case 2:
			return gli::gl::internal_format::INTERNAL_RG32F;
		case 3:
			return gli::gl::internal_format::INTERNAL_RGB32F;
		case 4:
			return gli::gl::internal_format::INTERNAL_RGBA32F;
		}
	}
	else
	{
		// TODO let the user decide if srgb conversion should be done
		switch (nComponents)
		{
		case 1:
			return gli::gl::internal_format::INTERNAL_SR8;
			//return gli::gl::internal_format::INTERNAL_R8_UNORM;
		case 2:
			return gli::gl::internal_format::INTERNAL_SRG8;
			//return gli::gl::internal_format::INTERNAL_RG8_UNORM;
		case 3:
			return gli::gl::internal_format::INTERNAL_SRGB8;
			//return gli::gl::internal_format::INTERNAL_RGB8_UNORM;
		case 4:
			return gli::gl::internal_format::INTERNAL_SRGB8_ALPHA8;
			//return gli::gl::internal_format::INTERNAL_RGBA8_UNORM;
		}
	}
	
	return uint32_t(-1);
}

std::unique_ptr<ImageResource> stb_load(const char* filename)
{
	// create resource with one layer
	ImageMipmap mipmap;
	ImageFormat format;

	stbi_set_flip_vertically_on_load(true);
	if (stbi_is_hdr(filename))
	{
		// load hdr file
		int nComponents = 0;
		float* data = stbi_loadf(filename, reinterpret_cast<int*>(&mipmap.width),
			reinterpret_cast<int*>(&mipmap.height), &nComponents, 0);
		if (!data)
			throw std::exception("error during reading file");

		format.openglInternalFormat = getSizedInternalFormat(nComponents, true);
		format.openglExternalFormat = getDefaultExternalFormat(nComponents);
		format.openglType = gli::gl::type_format::TYPE_F32;
		format.isCompressed = false;
		format.isSrgb = false;

		size_t mipSize = mipmap.width * mipmap.height * nComponents * 4;
		mipmap.bytes.resize(mipSize);
		memcpy(mipmap.bytes.data(), data, mipSize);

		stbi_image_free(data);
	}
	else
	{
		// load ldr file
		int nComponents = 0;
		stbi_uc* data = stbi_load(filename, reinterpret_cast<int*>(&mipmap.width), 
			reinterpret_cast<int*>(&mipmap.height), &nComponents, 0);
		if (!data)
			throw std::exception("error during reading file");

		format.openglInternalFormat = getSizedInternalFormat(nComponents, false);
		format.openglExternalFormat = getDefaultExternalFormat(nComponents);
		format.openglType = gli::gl::type_format::TYPE_U8;
		format.isCompressed = false;
		format.isSrgb = false; // srbb conversion is done by choosing the correct internal format (e.g. srgb8)

		size_t mipSize = mipmap.width * mipmap.height * nComponents;
		mipmap.bytes.resize(mipSize);
		memcpy(mipmap.bytes.data(), data, mipSize);

		stbi_image_free(data);
	}

	// make the resource
	auto res = std::make_unique<ImageResource>();
	res->format = format;
	res->layer.emplace_back();
	res->layer[0].faces.emplace_back();
	res->layer[0].faces[0].mipmaps.push_back(std::move(mipmap));

	return res;
}

void stb_save_png(const char* filename, int width, int height, int components, const void* data)
{
	stbi_flip_vertically_on_write(1);
	auto res = stbi_write_png(filename, width, height, components, data, width * components);
	if (!res)
		throw std::exception("could not save file");
}

void stb_save_bmp(const char* filename, int width, int height, int components, const void* data)
{
	stbi_flip_vertically_on_write(1);
	auto res = stbi_write_bmp(filename, width, height, components, data);
	if (!res)
		throw std::exception("could not save file");
}

void stb_save_hdr(const char* filename, int width, int height, int components, const void* data)
{
	stbi_flip_vertically_on_write(1);
	auto res = stbi_write_hdr(filename, width, height, components, reinterpret_cast<const float*>(data));
	if (!res)
		throw std::exception("could not save file");
}
