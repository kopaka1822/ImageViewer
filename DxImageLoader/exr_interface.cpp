#include "pch.h"
#include "exr_interface.h"
#define TINYEXR_IMPLEMENTATION
#include "../dependencies/tinyexr/tinyexr.h"

std::unique_ptr<image::IImage> openexr_load(const char* filename)
{
	float* out = nullptr; // width * height * RGBA
	int width = 0;
	int height = 0;
	const char* err = nullptr;

	int ret = LoadEXR(&out, &width, &height, filename, &err);
	if (ret != TINYEXR_SUCCESS)
		throw std::runtime_error(err);

	auto res = std::make_unique<image::SimpleImage>(
		gli::format::FORMAT_RGBA32_SFLOAT_PACK32, 
		gli::format::FORMAT_RGBA32_SFLOAT_PACK32,
		width, height, 4 * 4
	);

	size_t imgSize;
	auto dst = res->getData(0, 0, imgSize);

	memcpy(dst, out, imgSize);

	free(out);

	return res;
}
