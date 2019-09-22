#include "pch.h"
#include "exr_interface.h"
#define TINYEXR_IMPLEMENTATION
#include "../dependencies/tinyexr/tinyexr.h"

std::unique_ptr<image::Image> openexr_load(const char* filename)
{
	auto res = std::make_unique<image::Image>();

	float* out = nullptr; // width * height * RGBA
	int width = 0;
	int height = 0;
	const char* err = nullptr;

	int ret = LoadEXR(&out, &width, &height, filename, &err);
	if (ret != TINYEXR_SUCCESS)
		throw std::runtime_error(err);

	// create single image mipmap
	image::Mipmap mipmap;
	size_t imgSize = width * height * 16; // float * rgba
	mipmap.bytes.resize(imgSize);
	mipmap.depth = 1;
	mipmap.width = width;
	mipmap.height = height;

	memcpy(mipmap.bytes.data(), out, imgSize);

	res->format.isSrgb = false;
	res->format.dxgi = DXGI_FORMAT_R32G32B32A32_FLOAT;
	res->format.hasAlpha = true;

	res->layer.emplace_back();
	res->layer[0].mipmaps.push_back(std::move(mipmap));

	return res;
}
