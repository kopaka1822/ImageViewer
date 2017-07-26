#include "stb_loader.h"
#define STB_IMAGE_IMPLEMENTATION
#define STB_IMAGE_WRITE_IMPLEMENTATION
#include "../dependencies/stb/stb_image.h"

#include <gli/gl.hpp>

uint32_t getDefaultInternalFormat(int nComponents)
{
	switch (nComponents)
	{
	case 1:
		return gli::gl::external_format::EXTERNAL_RED;
	case 2:
		return gli::gl::external_format::EXTERNAL_RG;
	case 3:
		return gli::gl::external_format::EXTERNAL_RGB;
	case 4:
		return gli::gl::external_format::EXTERNAL_RGBA;
	}
	return uint32_t(-1);
}

std::unique_ptr<ImageResource> stb_load(const char* filename)
{
	// create resource with one layer
	ImageMipmap mipmap;
	ImageFormat format;

	if (stbi_is_hdr(filename))
	{
		// load hdr file
		int nComponents = 0;
		//format.componentType = ImageFormat::COMPONENT_TYPE_FLOAT;
		float* data = stbi_loadf(filename, reinterpret_cast<int*>(&mipmap.width),
			reinterpret_cast<int*>(&mipmap.height), &nComponents, 0);
		if (!data)
			return nullptr;

		format.openglInternalFormat = getDefaultInternalFormat(nComponents);
		format.openglExternalFormat = format.openglInternalFormat;
		format.openglType = gli::gl::type_format::TYPE_F32;
		format.isCompressed = false;

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
			return nullptr;

		format.openglInternalFormat = getDefaultInternalFormat(nComponents);
		format.openglExternalFormat = format.openglInternalFormat;
		format.openglType = gli::gl::type_format::TYPE_U8;
		format.isCompressed = false;

		size_t mipSize = mipmap.width * mipmap.height * nComponents;
		mipmap.bytes.resize(mipSize);
		memcpy(mipmap.bytes.data(), data, mipSize);

		stbi_image_free(data);
	}

	// make the resource
	std::unique_ptr<ImageResource> res = std::make_unique<ImageResource>();
	res->format = format;
	res->layer.push_back(ImageLayer());
	res->layer[0].mipmaps.push_back(std::move(mipmap));

	return res;
}
