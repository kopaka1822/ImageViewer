#include "pch.h"
#include "ktx_interface.h"
#define KTX_OPENGL
#include "../dependencies/ktx/include/ktx.h"
#include <stdexcept>
#include <algorithm>

std::unique_ptr<image::Image> ktx_load(const char* filename)
{
	ktxTexture2* ktex;
	auto err = ktxTexture2_CreateFromNamedFile(filename, 
		KTX_TEXTURE_CREATE_ALLOC_STORAGE | KTX_TEXTURE_CREATE_LOAD_IMAGE_DATA_BIT | KTX_TEXTURE_CREATE_RAW_KVDATA_BIT
		, &ktex);

	if (err != KTX_SUCCESS)
		throw std::runtime_error("error opening file");

	auto ktxThis = reinterpret_cast<ktxTexture*>(ktex);

	auto res = std::make_unique<image::Image>();
	// TODO conversion from VK_FORMAT to gli format
	res->original = gli::format(ktex->vkFormat);
	// TODO conversion to one of the supported formats (see image.h). gli can probably be used to do the conversion
	res->format = gli::format(ktex->vkFormat);
	res->layer.resize(ktex->numFaces * ktex->numLayers);

	size_t dstLayer = 0;
	for(size_t srcLayer = 0; srcLayer < ktex->numLayers; ++srcLayer)
	{
		for(size_t srcFace = 0; srcFace < ktex->numFaces; ++srcFace)
		{
			res->layer[dstLayer].mipmaps.resize(ktex->numLevels);
			size_t width = ktex->baseWidth;
			size_t height = ktex->baseHeight;
			size_t depth = ktex->baseDepth;
			assert(depth == 1);

			for(size_t mip = 0; mip < ktex->numLevels; ++mip)
			{
				auto& dstMip = res->layer[dstLayer].mipmaps[mip];
				dstMip.width = width;
				dstMip.height = height;
				auto mipSize = ktex->vtbl->GetImageSize(ktxThis, mip);
				dstMip.bytes.resize(mipSize);
				ktx_size_t offset = 0;
				ktex->vtbl->GetImageOffset(ktxThis, mip, srcLayer, srcFace, &offset);

				memcpy(dstMip.bytes.data(), ktex->pData + offset, mipSize);

				width = std::max(width / 2, size_t(1));
				height = std::max(height / 2, size_t(1));
			}
			++dstLayer;
		}
	}

	ktxTexture_Destroy(ktxThis);

	return res;
}
