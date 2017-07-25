#include "stb_loader.h"
#define STB_IMAGE_IMPLEMENTATION
#define STB_IMAGE_WRITE_IMPLEMENTATION
#include "../dependencies/stb/stb_image.h"

std::unique_ptr<ImageResource> stb_load(const char* filename)
{
	// create resource with one layer
	ImageMipmap mipmap;
	ImageFormat format;

	if (stbi_is_hdr(filename))
	{
		// load hdr file
		int nComponents = 0;
		format.componentType = ImageFormat::COMPONENT_TYPE_FLOAT;
		float* data = stbi_loadf(filename, reinterpret_cast<int*>(&mipmap.width),
			reinterpret_cast<int*>(&mipmap.height), &nComponents, 0);
		if (!data)
			return nullptr;

		format.componentCount = nComponents;
		format.componentSize = 4;

		size_t mipSize = mipmap.width * mipmap.height * nComponents * format.componentSize;
		mipmap.bytes.resize(mipSize);
		memcpy(mipmap.bytes.data(), data, mipSize);

		stbi_image_free(data);
	}
	else
	{
		// load ldr file
		int nComponents = 0;
		format.componentType = ImageFormat::COMPONENT_TYPE_INT;
		stbi_uc* data = stbi_load(filename, reinterpret_cast<int*>(&mipmap.width), 
			reinterpret_cast<int*>(&mipmap.height), &nComponents, 0);
		if (!data)
			return nullptr;

		format.componentCount = nComponents;
		format.componentSize = 1; // 0-255 (1 byte)

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
