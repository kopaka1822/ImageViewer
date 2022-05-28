#include "pch.h"
#include "hdr_interface.h"
#include <stdexcept>

#include "convert.h"
#include "../dependencies/hdr/rgbe.h"

std::unique_ptr<image::IImage> hdr_load(const char* filename)
{
	std::unique_ptr<image::IImage> res;
	FILE* fp = fopen(filename, "rb");
	if (!fp)
		throw std::runtime_error("could not open file");

	int width, heigth;
	rgbe_header_info header;
	try
	{
		RGBE_ReadHeader(fp, &width, &heigth, &header);

		// create file (add alpha channel for staging purposes)
		res.reset(new image::SimpleImage(
			gli::format::FORMAT_RGB32_SFLOAT_PACK32,
			gli::format::FORMAT_RGBA32_SFLOAT_PACK32,
			width, heigth, 4 * 4
		));

		
		uint32_t dataSize = 0;
		auto dataPtr = res->getData(0, 0, dataSize);
		float* floatPtr = reinterpret_cast<float*>(dataPtr);
		RGBE_ReadPixels_RLE(fp, floatPtr, width, heigth);

		// fix alignment
		image::expandRGBtoRGBA(floatPtr, width * heigth, 1.0f);

		// TODO handle gamma and exposure parameters
	}
	catch(...)
	{
		fclose(fp);
		throw;
	}
	fclose(fp);

	return res;
}

std::vector<uint32_t> hdr_get_export_formats()
{
	return {
		gli::format::FORMAT_RGB8E8_UFLOAT_PACK32
	};
}

void hdr_write(image::IImage& image, const char* filename)
{
	if(image.getFormat() != gli::FORMAT_RGBA32_SFLOAT_PACK32)
		throw std::runtime_error("expected RGBA32F image format for hdr export");

	uint32_t dataSize = 0;
	auto dataPtr = image.getData(0, 0, dataSize);

	// adjust stride of RGBA float data to RGB float
	image::changeStride(dataPtr, dataSize, 16, 12);

	FILE* fp = fopen(filename, "wb");
	if (!fp)
		throw std::runtime_error("could not open file");

	try
	{
		RGBE_WriteHeader(fp, image.getWidth(0), image.getHeight(0), nullptr);

		auto floatPtr = reinterpret_cast<float*>(dataPtr);
		RGBE_WritePixels_RLE(fp, floatPtr, image.getWidth(0), image.getHeight(0));
	}
	catch(...)
	{
		fclose(fp);
		throw;
	}
	fclose(fp);
}
