#include "openexr.h"
#define TINYEXR_IMPLEMENTATION
#include "../dependencies/tinyexr/tinyexr.h"
#include <gli/gl.hpp>

std::unique_ptr<ImageResource> openexr_load(const char* filename)
{
	auto res = std::make_unique<ImageResource>();

	float* out = nullptr; // width * height * RGBA
	int width = 0;
	int height = 0;
	const char* err = nullptr;

	int ret = LoadEXR(&out, &width, &height, filename, &err);
	if (ret != TINYEXR_SUCCESS)
		throw std::runtime_error(err);

	// create single image mipmap
	ImageMipmap mipmap;
	size_t imgSize = width * height * 16; // float * rgba
	mipmap.bytes.resize(imgSize);
	mipmap.depth = 1;
	mipmap.width = width;
	mipmap.height = height;
	// image is upside down => revert
	
	size_t lineBytes = width * 16;
	for(size_t y = 0; y < height; ++y)
	{
		memcpy(mipmap.bytes.data() + y * lineBytes, reinterpret_cast<char*>(out) + (height - y - 1) * lineBytes, lineBytes);
	}

	res->format.isCompressed = false;
	res->format.isSrgb = false;
	res->format.openglType = gli::gl::type_format::TYPE_F32;
	res->format.openglExternalFormat = gli::gl::external_format::EXTERNAL_RGBA;
	res->format.openglInternalFormat = gli::gl::internal_format::INTERNAL_RGBA32F;
	res->layer.emplace_back();
	res->layer[0].faces.emplace_back();
	res->layer[0].faces[0].mipmaps.push_back(std::move(mipmap));

	return res;
}
