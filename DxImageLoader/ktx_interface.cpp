#include "pch.h"
#include "ktx_interface.h"

#include "../dependencies/ktx/include/ktx.h"
#include <stdexcept>
#include <algorithm>
#include "VkFormat.h"
#include <unordered_map>
#include <string>
#include <thread>

#include "GliImage.h"
#include "interface.h"
#include "gli_interface.h"

gli::format convertFormat(VkFormat format);
VkFormat convertFormat(gli::format);

void set_ktx_image_data(ktxTexture* ktex, GliImage& image)
{
	// set image data for all layers, faces and levels
	for (uint32_t layer = 0; layer < image.getNumNonFaceLayers(); ++layer)
	{
		for (uint32_t face = 0; face < image.getNumFaces(); ++face)
		{
			for (uint32_t level = 0; level < image.getNumMipmaps(); ++level)
			{
				size_t byteSize;
				const uint8_t* data = image.getData(layer * image.getNumFaces() + face, level, byteSize);
				auto curDepth = image.getDepth(level);

				if (curDepth == 1)
				{
					auto err = ktxTexture_SetImageFromMemory(ktex, level, layer, face, data, byteSize);
					if (err != KTX_SUCCESS)
						throw std::runtime_error(std::string("could not set image data: ") + ktxErrorString(err));
				}
				else // for multiple depth slices => split as if it has multiple faces
				{
					auto sliceSize = byteSize / curDepth;
					for (uint32_t z = 0; z < curDepth; ++z)
					{
						auto err = ktxTexture_SetImageFromMemory(ktex, level, layer, z, data + z * sliceSize, sliceSize);
						if (err != KTX_SUCCESS)
							throw std::runtime_error(std::string("could not set image data: ") + ktxErrorString(err));
					}
				}
			}
		}
	}
}

void ktx1_save_image(const char* filename, GliImage& image, gli::format format, int quality)
{
	// convert format if it does not match
	if (image.getFormat() != format)
	{
		auto tmp = image.convert(format, quality);
		ktx1_save_image(filename, *tmp, format, quality);
		return;
	}

	ktxTexture1* ktex;
	ktxTextureCreateInfo i;
	i.glInternalformat = get_gl_format(format);
	i.vkFormat = convertFormat(format); // it is okay if this is undefined for ktx1
	i.baseWidth = image.getWidth(0);
	i.baseHeight = image.getHeight(0);
	i.baseDepth = image.getDepth(0);
	if (i.baseDepth > 1) i.numDimensions = 3;
	else if (i.baseHeight > 1 || quality < 100) i.numDimensions = 2;
	else i.numDimensions = 1;
	i.numLevels = image.getNumMipmaps();
	i.numLayers = image.getNumNonFaceLayers();
	i.numFaces = image.getNumFaces();
	i.isArray = i.numLayers > 1;
	i.generateMipmaps = false; // TODO let the user select

	auto err = ktxTexture1_Create(&i, KTX_TEXTURE_CREATE_ALLOC_STORAGE, &ktex);
	if (err != KTX_SUCCESS)
		throw std::runtime_error(std::string("failed create ktx texture storage: ") + ktxErrorString(err));

	set_ktx_image_data(ktxTexture(ktex), image);

	ktxTexture_WriteToNamedFile(ktxTexture(ktex), filename);
	ktxTexture_Destroy(ktxTexture(ktex));
}

void ktx2_save_image(const char* filename, GliImage& image, gli::format format, int quality)
{
	// convert format if it does not match
	if(image.getFormat() != format)
	{
		auto tmp = image.convert(format, quality);
		ktx2_save_image(filename, *tmp, format, quality);
		return;
	}
	
	ktxTexture2* ktex;
	ktxTextureCreateInfo i;
	i.glInternalformat = 0; // ignored for ktx2
	i.vkFormat = convertFormat(format);
	if(i.vkFormat == VK_FORMAT_UNDEFINED)
		throw std::runtime_error("Could not find a matching VK_FORMAT for the requested output format");
	i.baseWidth = image.getWidth(0);
	i.baseHeight = image.getHeight(0);
	i.baseDepth = image.getDepth(0);
	if(i.baseDepth > 1) i.numDimensions = 3;
	else if(i.baseHeight > 1 || quality < 100) i.numDimensions = 2;
	else i.numDimensions = 1;
	i.numLevels = image.getNumMipmaps();
	i.numLayers = image.getNumNonFaceLayers();
	i.numFaces = image.getNumFaces();
	i.isArray = i.numLayers > 1;
	i.generateMipmaps = false; // TODO let the user select

	auto err = ktxTexture2_Create(&i, KTX_TEXTURE_CREATE_ALLOC_STORAGE, &ktex);
	if(err != KTX_SUCCESS)
		throw std::runtime_error(std::string("failed create ktx texture storage: ") + ktxErrorString(err));

	set_ktx_image_data(ktxTexture(ktex), image);

	// optionally compress (if it was not already compressed)
	if(!is_compressed(format) && quality < 100)
	{
		set_progress(0, "basis compression");
		ktxBasisParams params = {};
		params.structSize = sizeof(params);
		params.threadCount = std::thread::hardware_concurrency();
		params.compressionLevel = KTX_ETC1S_DEFAULT_COMPRESSION_LEVEL;
		params.qualityLevel = std::max((quality * 254) / 99 + 1, 1); // scale quality [0, 99] between [1, 255]
		params.normalMap = KTX_FALSE;
		if (!gli::is_srgb(format)) // only valid for linear textures
	        params.normalMap = get_global_parameter_i("normalmap", 0) ? KTX_TRUE : KTX_FALSE;

	    // select uastc for everything that is not color (here: for everyhing that is not SRGB)
		// unless the "uastc srgb" flag is set => then use usastc as well
		if(!gli::is_srgb(format) || get_global_parameter_i("uastc srgb", 0))
		{
		    params.uastc = KTX_TRUE;
			params.uastcFlags = KTX_PACK_UASTC_MAX_LEVEL; // maximum supported quality
			params.uastcRDO = params.normalMap ? KTX_FALSE : KTX_TRUE;
		}

		// optional if compression
		err = ktxTexture2_CompressBasisEx(ktex, &params);
		if (err != KTX_SUCCESS)
			throw std::runtime_error(std::string("failed to compress ktx texture: ") + ktxErrorString(err));
	}
	
	ktxTexture_WriteToNamedFile(ktxTexture(ktex), filename);
	ktxTexture_Destroy(ktxTexture(ktex));
}

std::unique_ptr<image::IImage> ktx_load_base(ktxTexture* ktex, gli::format format, gli::format originalFormat)
{
	// store data in gli storage to be able to convert it easily
	auto res = std::make_unique<GliImage>(format, originalFormat,
		ktex->numLayers, ktex->numFaces, ktex->numLevels,
		ktex->baseWidth, ktex->baseHeight, ktex->baseDepth);


	ktx_uint32_t dstLayer = 0;
	for (ktx_uint32_t srcLayer = 0; srcLayer < ktex->numLayers; ++srcLayer)
	{
		for (ktx_uint32_t srcFace = 0; srcFace < ktex->numFaces; ++srcFace)
		{
			for (ktx_uint32_t mip = 0; mip < ktex->numLevels; ++mip)
			{
				ktx_size_t offset = 0;
				ktxTexture_GetImageOffset(ktex, ktx_uint32_t(mip), ktx_uint32_t(srcLayer), ktx_uint32_t(srcFace), &offset);
				//ktex2->vtbl->GetImageOffset(ktex, mip, srcLayer, srcFace, &offset);
				auto ktxLvlSize = ktxTexture_GetImageSize(ktex, ktx_uint32_t(mip));
				ktxLvlSize *= res->getDepth(mip); // is not multiplied with depth layer
				size_t size;
				auto dstData = res->getData(dstLayer, mip, size);
				if(ktxLvlSize == size)
				{
					memcpy(dstData, ktex->pData + offset, size); // alignment matches
				}
				else if(size < ktxLvlSize)
				{
					// calculate size with alignment after each row
					size_t rows = res->getHeight(mip) * res->getDepth(mip);
					size_t unalignedRow = size / rows;
					size_t alignedRow = ktxLvlSize / rows;
					auto srcData = ktex->pData;
					//std::vector<uint8_t> debugSrc(srcData, srcData + ktxLvlSize);
					// copy row by row
					for(size_t r = 0; r < rows; ++r)
					{
						memcpy(dstData, srcData, unalignedRow);
						dstData += unalignedRow;
						srcData += alignedRow;
					}
				}
				else throw std::runtime_error("suggested level size of gli does not match with ktx api");
			}
			++dstLayer;
		}
	}

	ktxTexture_Destroy(ktex);

	if (!image::isSupported(res->getFormat()))
	{
		res = res->convert(image::getSupportedFormat(res->getFormat()), 100);
	}

	if (ktex->orientation.y == KTX_ORIENT_Y_UP)
		res->flip();

	return res;
}

std::unique_ptr<image::IImage> ktx1_load(ktxTexture* ktex)
{
	assert(ktex->classId == ktxTexture1_c);
	ktxTexture1* ktex1 = reinterpret_cast<ktxTexture1*>(ktex);

	gli::format format = gli::FORMAT_UNDEFINED;
	gli::format originalFormat = format;
	assert(!ktxTexture1_NeedsTranscoding(ktex1)); // currently set to return false 
	format = originalFormat = get_format_from_GL(ktex1->glInternalformat, ktex1->glFormat, ktex1->glType);

	if (format == gli::FORMAT_UNDEFINED)
		throw std::runtime_error("could not interpret format id " + std::to_string(ktex1->glFormat));

	return ktx_load_base(ktex, format, originalFormat);
}

std::unique_ptr<image::IImage> ktx2_load(ktxTexture* ktex)
{
	assert(ktex->classId == ktxTexture2_c);
	ktxTexture2* ktex2 = reinterpret_cast<ktxTexture2*>(ktex);

	gli::format format = gli::FORMAT_UNDEFINED;
	gli::format originalFormat = format;
	if(ktxTexture2_NeedsTranscoding(ktex2)) // transcode from compressed format
	{
		const auto compressionSheme = ktex2->supercompressionScheme;
		auto numComponents = ktxTexture2_GetNumComponents(ktex2);
		// do transcoding
		auto err = ktxTexture2_TranscodeBasis(ktex2, KTX_TTF_RGBA32, 0);
		if (err != KTX_SUCCESS)
			throw std::runtime_error(std::string("failed to transcode file: ") + ktxErrorString(err));
		// set format and (previous) original format
		format = convertFormat(VkFormat(ktex2->vkFormat));
		if(compressionSheme == KTX_SS_BASIS_LZ) // ETC1S
            switch (numComponents)
            {
            case 1: // should not happen (no matching srgb format)
			case 2: // should not happen
			case 3: originalFormat = gli::FORMAT_RGB_ETC2_SRGB_BLOCK8; break;
			case 4: originalFormat = gli::FORMAT_RGBA_ETC2_SRGB_BLOCK8; break;
            }
		else // UASTC
			originalFormat = gli::FORMAT_RGBA_ASTC_4X4_UNORM_BLOCK16; // astc has only rgba formats in the enum
	}
	else format = originalFormat = convertFormat(VkFormat(ktex2->vkFormat)); // no transcoding needed => read format directly

	if (format == gli::FORMAT_UNDEFINED)
		throw std::runtime_error("could not translate format id from VK_FORMAT to Image Viewer format. VK_FORMAT: " + std::to_string(ktex2->vkFormat));

	return ktx_load_base(ktex, format, originalFormat);
}

std::unique_ptr<image::IImage> ktx_load(const char* filename)
{
	ktxTexture* ktex;
	auto err = ktxTexture_CreateFromNamedFile(filename, KTX_TEXTURE_CREATE_LOAD_IMAGE_DATA_BIT, &ktex);
	if (err != KTX_SUCCESS)
		throw std::runtime_error(std::string("failed to load file: ") + ktxErrorString(err));

	switch (ktex->classId)
	{
	case ktxTexture1_c:
		return ktx1_load(ktex);
	case ktxTexture2_c:
		return ktx2_load(ktex);
	}
	throw std::runtime_error("expected ktx2 texture or ktx1 texture class but got unknown class");
}

gli::format convertFormat(VkFormat format)
{
	static std::unordered_map<VkFormat, gli::format> lookup = {
	{VK_FORMAT_R4G4_UNORM_PACK8, gli::FORMAT_RG4_UNORM_PACK8},
	{VK_FORMAT_R4G4B4A4_UNORM_PACK16, gli::FORMAT_RGBA4_UNORM_PACK16},
	{VK_FORMAT_B4G4R4A4_UNORM_PACK16, gli::FORMAT_BGRA4_UNORM_PACK16},
	{VK_FORMAT_R5G6B5_UNORM_PACK16, gli::FORMAT_R5G6B5_UNORM_PACK16},
	{VK_FORMAT_B5G6R5_UNORM_PACK16, gli::FORMAT_B5G6R5_UNORM_PACK16},
	{VK_FORMAT_R5G5B5A1_UNORM_PACK16, gli::FORMAT_RGB5A1_UNORM_PACK16},
	{VK_FORMAT_B5G5R5A1_UNORM_PACK16, gli::FORMAT_BGR5A1_UNORM_PACK16},
	{VK_FORMAT_A1R5G5B5_UNORM_PACK16, gli::FORMAT_A1RGB5_UNORM_PACK16},
	{VK_FORMAT_R8_UNORM, gli::FORMAT_R8_UNORM_PACK8},
	{VK_FORMAT_R8_SNORM, gli::FORMAT_R8_SNORM_PACK8},
	{VK_FORMAT_R8_USCALED, gli::FORMAT_R8_USCALED_PACK8},
	{VK_FORMAT_R8_SSCALED, gli::FORMAT_R8_SSCALED_PACK8},
	{VK_FORMAT_R8_UINT, gli::FORMAT_R8_UINT_PACK8},
	{VK_FORMAT_R8_SINT, gli::FORMAT_R8_SINT_PACK8},
	{VK_FORMAT_R8_SRGB, gli::FORMAT_R8_SRGB_PACK8},
	{VK_FORMAT_R8G8_UNORM, gli::FORMAT_RG8_UNORM_PACK8},
	{VK_FORMAT_R8G8_SNORM, gli::FORMAT_RG8_SNORM_PACK8},
	{VK_FORMAT_R8G8_USCALED, gli::FORMAT_RG8_USCALED_PACK8},
	{VK_FORMAT_R8G8_SSCALED, gli::FORMAT_RG8_SSCALED_PACK8},
	{VK_FORMAT_R8G8_UINT, gli::FORMAT_RG8_UINT_PACK8},
	{VK_FORMAT_R8G8_SINT, gli::FORMAT_RG8_SINT_PACK8},
	{VK_FORMAT_R8G8_SRGB, gli::FORMAT_RG8_SRGB_PACK8},
	{VK_FORMAT_R8G8B8_UNORM, gli::FORMAT_RGB8_UNORM_PACK8},
	{VK_FORMAT_R8G8B8_SNORM, gli::FORMAT_RGB8_SNORM_PACK8},
	{VK_FORMAT_R8G8B8_USCALED, gli::FORMAT_RGB8_USCALED_PACK8},
	{VK_FORMAT_R8G8B8_SSCALED, gli::FORMAT_RGB8_SSCALED_PACK8},
	{VK_FORMAT_R8G8B8_UINT, gli::FORMAT_RGB8_UINT_PACK8},
	{VK_FORMAT_R8G8B8_SINT, gli::FORMAT_RGB8_SINT_PACK8},
	{VK_FORMAT_R8G8B8_SRGB, gli::FORMAT_RGB8_SRGB_PACK8},
	{VK_FORMAT_B8G8R8_UNORM, gli::FORMAT_BGR8_UNORM_PACK8},
	{VK_FORMAT_B8G8R8_SNORM, gli::FORMAT_BGR8_SNORM_PACK8},
	{VK_FORMAT_B8G8R8_USCALED, gli::FORMAT_BGR8_USCALED_PACK8},
	{VK_FORMAT_B8G8R8_SSCALED, gli::FORMAT_BGR8_SSCALED_PACK8},
	{VK_FORMAT_B8G8R8_UINT, gli::FORMAT_BGR8_UINT_PACK8},
	{VK_FORMAT_B8G8R8_SINT, gli::FORMAT_BGR8_SINT_PACK8},
	{VK_FORMAT_B8G8R8_SRGB, gli::FORMAT_BGR8_SRGB_PACK8},
	{VK_FORMAT_R8G8B8A8_UNORM, gli::FORMAT_RGBA8_UNORM_PACK8 },
	{VK_FORMAT_R8G8B8A8_SNORM, gli::FORMAT_RGBA8_SNORM_PACK8 },
	{VK_FORMAT_R8G8B8A8_USCALED, gli::FORMAT_RGBA8_USCALED_PACK8 },
	{VK_FORMAT_R8G8B8A8_SSCALED, gli::FORMAT_RGBA8_SSCALED_PACK8 },
	{VK_FORMAT_R8G8B8A8_UINT, gli::FORMAT_RGBA8_UINT_PACK8 },
	{VK_FORMAT_R8G8B8A8_SINT, gli::FORMAT_RGBA8_SINT_PACK8 },
	{VK_FORMAT_R8G8B8A8_SRGB, gli::FORMAT_RGBA8_SRGB_PACK8 },
	{VK_FORMAT_B8G8R8A8_UNORM, gli::FORMAT_RGBA8_UNORM_PACK8 },
	{VK_FORMAT_B8G8R8A8_SNORM, gli::FORMAT_RGBA8_SNORM_PACK8 },
	{VK_FORMAT_B8G8R8A8_USCALED, gli::FORMAT_BGRA8_USCALED_PACK8 },
	{VK_FORMAT_B8G8R8A8_SSCALED, gli::FORMAT_BGRA8_SSCALED_PACK8 },
	{VK_FORMAT_B8G8R8A8_UINT, gli::FORMAT_BGRA8_UINT_PACK8 },
	{VK_FORMAT_B8G8R8A8_SINT, gli::FORMAT_BGRA8_SINT_PACK8 },
	{VK_FORMAT_B8G8R8A8_SRGB, gli::FORMAT_BGRA8_SRGB_PACK8 },
	//{VK_FORMAT_A8B8G8R8_UNORM_PACK32, gli::FORMAT_ },
	//{VK_FORMAT_A8B8G8R8_SNORM_PACK32, gli::FORMAT_ },  
	//{VK_FORMAT_A8B8G8R8_USCALED_PACK32, gli::FORMAT_ },  
	//{VK_FORMAT_A8B8G8R8_SSCALED_PACK32, gli::FORMAT_ },  
	//{VK_FORMAT_A8B8G8R8_UINT_PACK32, gli::FORMAT_ },  
	//{VK_FORMAT_A8B8G8R8_SINT_PACK32, gli::FORMAT_ },  
	//{VK_FORMAT_A8B8G8R8_SRGB_PACK32, gli::FORMAT_ },  
	//{VK_FORMAT_A2R10G10B10_UNORM_PACK32, gli::FORMAT_ },  
	//{VK_FORMAT_A2R10G10B10_SNORM_PACK32, gli::FORMAT_ },  
	//{VK_FORMAT_A2R10G10B10_USCALED_PACK32, gli::FORMAT_ },  
	//{VK_FORMAT_A2R10G10B10_SSCALED_PACK32, gli::FORMAT_ },  
	{VK_FORMAT_A2R10G10B10_UINT_PACK32, gli::FORMAT_RGB10A2_UINT_PACK32 },  
	{VK_FORMAT_A2R10G10B10_SINT_PACK32, gli::FORMAT_RGB10A2_SINT_PACK32 }, 
	{VK_FORMAT_A2B10G10R10_UNORM_PACK32, gli::FORMAT_BGR10A2_SNORM_PACK32 },  
	{VK_FORMAT_A2B10G10R10_SNORM_PACK32, gli::FORMAT_BGR10A2_SNORM_PACK32 },  
	//{VK_FORMAT_A2B10G10R10_USCALED_PACK32, gli::FORMAT_ },  
	//{VK_FORMAT_A2B10G10R10_SSCALED_PACK32, gli::FORMAT_ },  
	{VK_FORMAT_A2B10G10R10_UINT_PACK32, gli::FORMAT_BGR10A2_UINT_PACK32 },  
	{VK_FORMAT_A2B10G10R10_SINT_PACK32, gli::FORMAT_BGR10A2_SINT_PACK32 },  
	{VK_FORMAT_R16_UNORM, gli::FORMAT_R16_UNORM_PACK16 },  
	{VK_FORMAT_R16_SNORM, gli::FORMAT_R16_SNORM_PACK16 },  
	{VK_FORMAT_R16_USCALED, gli::FORMAT_R16_USCALED_PACK16 },  
	{VK_FORMAT_R16_SSCALED, gli::FORMAT_R16_SSCALED_PACK16 },  
	{VK_FORMAT_R16_UINT, gli::FORMAT_R16_UINT_PACK16 },  
	{VK_FORMAT_R16_SINT, gli::FORMAT_R16_SINT_PACK16 },  
	{VK_FORMAT_R16_SFLOAT, gli::FORMAT_R16_SFLOAT_PACK16 },  
	{VK_FORMAT_R16G16_UNORM, gli::FORMAT_RG16_UNORM_PACK16 },  
	{VK_FORMAT_R16G16_SNORM, gli::FORMAT_RG16_SNORM_PACK16 },  
	{VK_FORMAT_R16G16_USCALED, gli::FORMAT_RG16_USCALED_PACK16 },  
	{VK_FORMAT_R16G16_SSCALED, gli::FORMAT_RG16_SSCALED_PACK16 },  
	{VK_FORMAT_R16G16_UINT, gli::FORMAT_RG16_UINT_PACK16 },  
	{VK_FORMAT_R16G16_SINT, gli::FORMAT_RG16_SINT_PACK16 },  
	{VK_FORMAT_R16G16_SFLOAT, gli::FORMAT_RG16_SFLOAT_PACK16 },  
	{VK_FORMAT_R16G16B16_UNORM, gli::FORMAT_RGB16_UNORM_PACK16 },  
	{VK_FORMAT_R16G16B16_SNORM, gli::FORMAT_RGB16_SNORM_PACK16 },
	{VK_FORMAT_R16G16B16_USCALED, gli::FORMAT_RGB16_USCALED_PACK16 },
	{VK_FORMAT_R16G16B16_SSCALED, gli::FORMAT_RGB16_SSCALED_PACK16 },
	{VK_FORMAT_R16G16B16_UINT, gli::FORMAT_RGB16_UINT_PACK16 },
	{VK_FORMAT_R16G16B16_SINT, gli::FORMAT_RGB16_SINT_PACK16 },
	{VK_FORMAT_R16G16B16_SFLOAT, gli::FORMAT_RGB16_SFLOAT_PACK16 },
	{VK_FORMAT_R16G16B16A16_UNORM, gli::FORMAT_RGBA16_UNORM_PACK16 },  
	{VK_FORMAT_R16G16B16A16_SNORM, gli::FORMAT_RGBA16_SNORM_PACK16 },
	{VK_FORMAT_R16G16B16A16_USCALED, gli::FORMAT_RGBA16_USCALED_PACK16 },
	{VK_FORMAT_R16G16B16A16_SSCALED, gli::FORMAT_RGBA16_SSCALED_PACK16 },
	{VK_FORMAT_R16G16B16A16_UINT, gli::FORMAT_RGBA16_UINT_PACK16 },
	{VK_FORMAT_R16G16B16A16_SINT, gli::FORMAT_RGBA16_SINT_PACK16 },
	{VK_FORMAT_R16G16B16A16_SFLOAT, gli::FORMAT_RGBA16_SFLOAT_PACK16 },
	{VK_FORMAT_R32_UINT, gli::FORMAT_R32_UINT_PACK32 },  
	{VK_FORMAT_R32_SINT, gli::FORMAT_R32_SINT_PACK32 },  
	{VK_FORMAT_R32_SFLOAT, gli::FORMAT_R32_SFLOAT_PACK32 },  
	{VK_FORMAT_R32G32_UINT, gli::FORMAT_RG32_UINT_PACK32 },  
	{VK_FORMAT_R32G32_SINT, gli::FORMAT_RG32_SINT_PACK32 },  
	{VK_FORMAT_R32G32_SFLOAT, gli::FORMAT_RG32_SFLOAT_PACK32 },  
	{VK_FORMAT_R32G32B32_UINT, gli::FORMAT_RGB32_UINT_PACK32 },  
	{VK_FORMAT_R32G32B32_SINT, gli::FORMAT_RGB32_SINT_PACK32 },  
	{VK_FORMAT_R32G32B32_SFLOAT, gli::FORMAT_RGB32_SFLOAT_PACK32 },  
	{VK_FORMAT_R32G32B32A32_UINT, gli::FORMAT_RGBA32_UINT_PACK32 },  
	{VK_FORMAT_R32G32B32A32_SINT, gli::FORMAT_RGBA32_SINT_PACK32 },
	{VK_FORMAT_R32G32B32A32_SFLOAT, gli::FORMAT_RGBA32_SFLOAT_PACK32 },
	{VK_FORMAT_R64_UINT, gli::FORMAT_R64_UINT_PACK64 },  
	{VK_FORMAT_R64_SINT, gli::FORMAT_R64_SINT_PACK64 },  
	{VK_FORMAT_R64_SFLOAT, gli::FORMAT_R64_SFLOAT_PACK64 },  
	{VK_FORMAT_R64G64_UINT, gli::FORMAT_RG64_UINT_PACK64 },  
	{VK_FORMAT_R64G64_SINT, gli::FORMAT_RG64_SINT_PACK64 },  
	{VK_FORMAT_R64G64_SFLOAT, gli::FORMAT_RG64_SFLOAT_PACK64 },  
	{VK_FORMAT_R64G64B64_UINT, gli::FORMAT_RGB64_UINT_PACK64 },  
	{VK_FORMAT_R64G64B64_SINT, gli::FORMAT_RGB64_SINT_PACK64 },  
	{VK_FORMAT_R64G64B64_SFLOAT, gli::FORMAT_RGB64_SFLOAT_PACK64 },  
	{VK_FORMAT_R64G64B64A64_UINT, gli::FORMAT_RGBA64_UINT_PACK64 },  
	{VK_FORMAT_R64G64B64A64_SINT, gli::FORMAT_RGBA64_SINT_PACK64 },  
	{VK_FORMAT_R64G64B64A64_SFLOAT, gli::FORMAT_RGBA64_SFLOAT_PACK64 },  
	{VK_FORMAT_B10G11R11_UFLOAT_PACK32, gli::FORMAT_RG11B10_UFLOAT_PACK32 },  
	//{VK_FORMAT_E5B9G9R9_UFLOAT_PACK32, gli::FORMAT_ },  
	{VK_FORMAT_D16_UNORM, gli::FORMAT_D16_UNORM_PACK16 },  
	{VK_FORMAT_X8_D24_UNORM_PACK32, gli::FORMAT_D24_UNORM_PACK32 },  
	{VK_FORMAT_D32_SFLOAT, gli::FORMAT_D32_SFLOAT_PACK32 },  
	{VK_FORMAT_S8_UINT, gli::FORMAT_S8_UINT_PACK8 },  
	{VK_FORMAT_D16_UNORM_S8_UINT, gli::FORMAT_D16_UNORM_S8_UINT_PACK32 },  
	{VK_FORMAT_D24_UNORM_S8_UINT, gli::FORMAT_D24_UNORM_S8_UINT_PACK32 },  
	{VK_FORMAT_D32_SFLOAT_S8_UINT, gli::FORMAT_D32_SFLOAT_S8_UINT_PACK64 },  
	{VK_FORMAT_BC1_RGB_UNORM_BLOCK, gli::FORMAT_RGB_DXT1_UNORM_BLOCK8 },  
	{VK_FORMAT_BC1_RGB_SRGB_BLOCK, gli::FORMAT_RGB_DXT1_SRGB_BLOCK8 },  
	{VK_FORMAT_BC1_RGBA_UNORM_BLOCK, gli::FORMAT_RGBA_DXT1_UNORM_BLOCK8 },
	{VK_FORMAT_BC1_RGBA_SRGB_BLOCK, gli::FORMAT_RGBA_DXT1_SRGB_BLOCK8 },
	{VK_FORMAT_BC2_UNORM_BLOCK, gli::FORMAT_RGBA_DXT3_UNORM_BLOCK16 },  
	{VK_FORMAT_BC2_SRGB_BLOCK, gli::FORMAT_RGBA_DXT3_SRGB_BLOCK16 },  
	{VK_FORMAT_BC3_UNORM_BLOCK, gli::FORMAT_RGBA_DXT5_UNORM_BLOCK16 },  
	{VK_FORMAT_BC3_SRGB_BLOCK, gli::FORMAT_RGBA_DXT5_SRGB_BLOCK16 },  
	{VK_FORMAT_BC4_UNORM_BLOCK, gli::FORMAT_R_ATI1N_UNORM_BLOCK8 },  
	{VK_FORMAT_BC4_SNORM_BLOCK, gli::FORMAT_R_ATI1N_SNORM_BLOCK8 },  
	{VK_FORMAT_BC5_UNORM_BLOCK, gli::FORMAT_RG_ATI2N_UNORM_BLOCK16 },  
	{VK_FORMAT_BC5_SNORM_BLOCK, gli::FORMAT_RG_ATI2N_SNORM_BLOCK16 },  
	{VK_FORMAT_BC6H_UFLOAT_BLOCK, gli::FORMAT_RGB_BP_UFLOAT_BLOCK16 },  
	{VK_FORMAT_BC6H_SFLOAT_BLOCK, gli::FORMAT_RGB_BP_SFLOAT_BLOCK16 },  
	{VK_FORMAT_BC7_UNORM_BLOCK, gli::FORMAT_RGBA_BP_UNORM_BLOCK16 },  
	{VK_FORMAT_BC7_SRGB_BLOCK, gli::FORMAT_RGBA_BP_SRGB_BLOCK16 },  
	{VK_FORMAT_ETC2_R8G8B8_UNORM_BLOCK, gli::FORMAT_RGB_ETC2_UNORM_BLOCK8 },  
	{VK_FORMAT_ETC2_R8G8B8_SRGB_BLOCK, gli::FORMAT_RGB_ETC2_SRGB_BLOCK8 },
	{VK_FORMAT_ETC2_R8G8B8A1_UNORM_BLOCK, gli::FORMAT_RGBA_ETC2_UNORM_BLOCK8 },  
	{VK_FORMAT_ETC2_R8G8B8A1_SRGB_BLOCK, gli::FORMAT_RGBA_ETC2_SRGB_BLOCK8 },
	{VK_FORMAT_ETC2_R8G8B8A8_UNORM_BLOCK, gli::FORMAT_RGBA_ETC2_UNORM_BLOCK16 },
	{VK_FORMAT_ETC2_R8G8B8A8_SRGB_BLOCK, gli::FORMAT_RGBA_ETC2_SRGB_BLOCK16 },
	{VK_FORMAT_EAC_R11_UNORM_BLOCK, gli::FORMAT_R_EAC_UNORM_BLOCK8 },  
	{VK_FORMAT_EAC_R11_SNORM_BLOCK, gli::FORMAT_R_EAC_SNORM_BLOCK8 },  
	{VK_FORMAT_EAC_R11G11_UNORM_BLOCK, gli::FORMAT_RG_EAC_UNORM_BLOCK16 },  
	{VK_FORMAT_EAC_R11G11_SNORM_BLOCK, gli::FORMAT_RG_EAC_SNORM_BLOCK16 },  
	{VK_FORMAT_ASTC_4x4_UNORM_BLOCK, gli::FORMAT_RGBA_ASTC_4X4_UNORM_BLOCK16 },
	{ VK_FORMAT_ASTC_5x4_UNORM_BLOCK, gli::FORMAT_RGBA_ASTC_5X4_UNORM_BLOCK16 },
	{ VK_FORMAT_ASTC_5x5_UNORM_BLOCK, gli::FORMAT_RGBA_ASTC_5X5_UNORM_BLOCK16 },
	{ VK_FORMAT_ASTC_6x5_UNORM_BLOCK, gli::FORMAT_RGBA_ASTC_6X5_UNORM_BLOCK16 },
	{ VK_FORMAT_ASTC_6x6_UNORM_BLOCK, gli::FORMAT_RGBA_ASTC_6X6_UNORM_BLOCK16 },
	{ VK_FORMAT_ASTC_8x5_UNORM_BLOCK, gli::FORMAT_RGBA_ASTC_8X5_UNORM_BLOCK16 },
	{ VK_FORMAT_ASTC_8x6_UNORM_BLOCK, gli::FORMAT_RGBA_ASTC_8X6_UNORM_BLOCK16 },
	{ VK_FORMAT_ASTC_8x8_UNORM_BLOCK, gli::FORMAT_RGBA_ASTC_8X8_UNORM_BLOCK16 },
	{ VK_FORMAT_ASTC_10x5_UNORM_BLOCK, gli::FORMAT_RGBA_ASTC_10X5_UNORM_BLOCK16 },
	{ VK_FORMAT_ASTC_10x6_UNORM_BLOCK, gli::FORMAT_RGBA_ASTC_10X6_UNORM_BLOCK16 },
	{ VK_FORMAT_ASTC_10x8_UNORM_BLOCK, gli::FORMAT_RGBA_ASTC_10X8_UNORM_BLOCK16 },
	{ VK_FORMAT_ASTC_10x10_UNORM_BLOCK, gli::FORMAT_RGBA_ASTC_10X10_UNORM_BLOCK16 },
	{ VK_FORMAT_ASTC_12x10_UNORM_BLOCK, gli::FORMAT_RGBA_ASTC_12X10_UNORM_BLOCK16 },
	{ VK_FORMAT_ASTC_12x12_UNORM_BLOCK, gli::FORMAT_RGBA_ASTC_12X12_UNORM_BLOCK16 },
	{ VK_FORMAT_ASTC_4x4_SRGB_BLOCK, gli::FORMAT_RGBA_ASTC_4X4_SRGB_BLOCK16 },
	{ VK_FORMAT_ASTC_5x4_SRGB_BLOCK, gli::FORMAT_RGBA_ASTC_5X4_SRGB_BLOCK16 },
	{ VK_FORMAT_ASTC_5x5_SRGB_BLOCK, gli::FORMAT_RGBA_ASTC_5X5_SRGB_BLOCK16 },
	{ VK_FORMAT_ASTC_6x5_SRGB_BLOCK, gli::FORMAT_RGBA_ASTC_6X5_SRGB_BLOCK16 },
	{ VK_FORMAT_ASTC_6x6_SRGB_BLOCK, gli::FORMAT_RGBA_ASTC_6X6_SRGB_BLOCK16 },
	{ VK_FORMAT_ASTC_8x5_SRGB_BLOCK, gli::FORMAT_RGBA_ASTC_8X5_SRGB_BLOCK16 },
	{ VK_FORMAT_ASTC_8x6_SRGB_BLOCK, gli::FORMAT_RGBA_ASTC_8X6_SRGB_BLOCK16 },
	{ VK_FORMAT_ASTC_8x8_SRGB_BLOCK, gli::FORMAT_RGBA_ASTC_8X8_SRGB_BLOCK16 },
	{ VK_FORMAT_ASTC_10x5_SRGB_BLOCK, gli::FORMAT_RGBA_ASTC_10X5_SRGB_BLOCK16 },
	{ VK_FORMAT_ASTC_10x6_SRGB_BLOCK, gli::FORMAT_RGBA_ASTC_10X6_SRGB_BLOCK16 },
	{ VK_FORMAT_ASTC_10x8_SRGB_BLOCK, gli::FORMAT_RGBA_ASTC_10X8_SRGB_BLOCK16 },
	{ VK_FORMAT_ASTC_10x10_SRGB_BLOCK, gli::FORMAT_RGBA_ASTC_10X10_SRGB_BLOCK16 },
	{ VK_FORMAT_ASTC_12x10_SRGB_BLOCK, gli::FORMAT_RGBA_ASTC_12X10_SRGB_BLOCK16 },
	{ VK_FORMAT_ASTC_12x12_SRGB_BLOCK, gli::FORMAT_RGBA_ASTC_12X12_SRGB_BLOCK16 },
	//{VK_FORMAT_G8B8G8R8_422_UNORM, gli::FORMAT_ },  
	//{VK_FORMAT_B8G8R8G8_422_UNORM, gli::FORMAT_ },  
	//{VK_FORMAT_G8_B8_R8_3PLANE_420_UNORM, gli::FORMAT_ },  
	//{VK_FORMAT_G8_B8R8_2PLANE_420_UNORM, gli::FORMAT_ },  
	//{VK_FORMAT_G8_B8_R8_3PLANE_422_UNORM, gli::FORMAT_ },  
	//{VK_FORMAT_G8_B8R8_2PLANE_422_UNORM, gli::FORMAT_ },  
	//{VK_FORMAT_G8_B8_R8_3PLANE_444_UNORM, gli::FORMAT_ },  
	//{VK_FORMAT_R10X6_UNORM_PACK16, gli::FORMAT_ },  
	//{VK_FORMAT_R10X6G10X6_UNORM_2PACK16, gli::FORMAT_ },  
	//{VK_FORMAT_R10X6G10X6B10X6A10X6_UNORM_4PACK16, gli::FORMAT_ },  
	//{VK_FORMAT_G10X6B10X6G10X6R10X6_422_UNORM_4PACK16, gli::FORMAT_ },  
	//{VK_FORMAT_B10X6G10X6R10X6G10X6_422_UNORM_4PACK16, gli::FORMAT_ },  
	//{VK_FORMAT_G10X6_B10X6_R10X6_3PLANE_420_UNORM_3PACK16, gli::FORMAT_ },  
	//{VK_FORMAT_G10X6_B10X6R10X6_2PLANE_420_UNORM_3PACK16, gli::FORMAT_ },  
	//{VK_FORMAT_G10X6_B10X6_R10X6_3PLANE_422_UNORM_3PACK16, gli::FORMAT_ },  
	//{VK_FORMAT_G10X6_B10X6R10X6_2PLANE_422_UNORM_3PACK16, gli::FORMAT_ },  
	//{VK_FORMAT_G10X6_B10X6_R10X6_3PLANE_444_UNORM_3PACK16, gli::FORMAT_ },  
	//{VK_FORMAT_R12X4_UNORM_PACK16, gli::FORMAT_ },  
	//{VK_FORMAT_R12X4G12X4_UNORM_2PACK16, gli::FORMAT_ },  
	//{VK_FORMAT_R12X4G12X4B12X4A12X4_UNORM_4PACK16, gli::FORMAT_ },  
	//{VK_FORMAT_G12X4B12X4G12X4R12X4_422_UNORM_4PACK16, gli::FORMAT_ },  
	//{VK_FORMAT_B12X4G12X4R12X4G12X4_422_UNORM_4PACK16, gli::FORMAT_ },  
	//{VK_FORMAT_G12X4_B12X4_R12X4_3PLANE_420_UNORM_3PACK16, gli::FORMAT_ },  
	//{VK_FORMAT_G12X4_B12X4R12X4_2PLANE_420_UNORM_3PACK16, gli::FORMAT_ },  
	//{VK_FORMAT_G12X4_B12X4_R12X4_3PLANE_422_UNORM_3PACK16, gli::FORMAT_ },  
	//{VK_FORMAT_G12X4_B12X4R12X4_2PLANE_422_UNORM_3PACK16, gli::FORMAT_ },  
	//{VK_FORMAT_G12X4_B12X4_R12X4_3PLANE_444_UNORM_3PACK16, gli::FORMAT_ },  
	//{VK_FORMAT_G16B16G16R16_422_UNORM, gli::FORMAT_ },  
	//{VK_FORMAT_B16G16R16G16_422_UNORM, gli::FORMAT_ },  
	//{VK_FORMAT_G16_B16_R16_3PLANE_420_UNORM, gli::FORMAT_ },  
	//{VK_FORMAT_G16_B16R16_2PLANE_420_UNORM, gli::FORMAT_ },  
	//{VK_FORMAT_G16_B16_R16_3PLANE_422_UNORM, gli::FORMAT_ },  
	//{VK_FORMAT_G16_B16R16_2PLANE_422_UNORM, gli::FORMAT_ },  
	//{VK_FORMAT_G16_B16_R16_3PLANE_444_UNORM, gli::FORMAT_ },  
	//{VK_FORMAT_PVRTC1_2BPP_UNORM_BLOCK_IMG, gli::FORMAT_ },  
	//{VK_FORMAT_PVRTC1_4BPP_UNORM_BLOCK_IMG, gli::FORMAT_ },  
	//{VK_FORMAT_PVRTC2_2BPP_UNORM_BLOCK_IMG, gli::FORMAT_ },  
	//{VK_FORMAT_PVRTC2_4BPP_UNORM_BLOCK_IMG, gli::FORMAT_ },  
	//{VK_FORMAT_PVRTC1_2BPP_SRGB_BLOCK_IMG, gli::FORMAT_ },  
	//{VK_FORMAT_PVRTC1_4BPP_SRGB_BLOCK_IMG, gli::FORMAT_ },  
	//{VK_FORMAT_PVRTC2_2BPP_SRGB_BLOCK_IMG, gli::FORMAT_ },  
	//{VK_FORMAT_PVRTC2_4BPP_SRGB_BLOCK_IMG, gli::FORMAT_ },  
	//{VK_FORMAT_ASTC_4x4_SFLOAT_BLOCK_EXT, gli::FORMAT_ },  
	//{VK_FORMAT_ASTC_5x4_SFLOAT_BLOCK_EXT, gli::FORMAT_ },  
	//{VK_FORMAT_ASTC_5x5_SFLOAT_BLOCK_EXT, gli::FORMAT_ },  
	//{VK_FORMAT_ASTC_6x5_SFLOAT_BLOCK_EXT, gli::FORMAT_ },  
	//{VK_FORMAT_ASTC_6x6_SFLOAT_BLOCK_EXT, gli::FORMAT_ },  
	//{VK_FORMAT_ASTC_8x5_SFLOAT_BLOCK_EXT, gli::FORMAT_ },  
	//{VK_FORMAT_ASTC_8x6_SFLOAT_BLOCK_EXT, gli::FORMAT_ },  
	//{VK_FORMAT_ASTC_8x8_SFLOAT_BLOCK_EXT, gli::FORMAT_ },  
	//{VK_FORMAT_ASTC_10x5_SFLOAT_BLOCK_EXT, gli::FORMAT_ },  
	//{VK_FORMAT_ASTC_10x6_SFLOAT_BLOCK_EXT, gli::FORMAT_ },  
	//{VK_FORMAT_ASTC_10x8_SFLOAT_BLOCK_EXT, gli::FORMAT_ },  
	//{VK_FORMAT_ASTC_10x10_SFLOAT_BLOCK_EXT, gli::FORMAT_ },  
	//{VK_FORMAT_ASTC_12x10_SFLOAT_BLOCK_EXT, gli::FORMAT_ },  
	//{VK_FORMAT_ASTC_12x12_SFLOAT_BLOCK_EXT, gli::FORMAT_ },  
	};

	auto it = lookup.find(format);
	if (it == lookup.end()) return gli::FORMAT_UNDEFINED;

	return it->second;
}



VkFormat convertFormat(gli::format format)
{
	switch (format) {
	case gli::FORMAT_RG4_UNORM_PACK8: return VK_FORMAT_R4G4_UNORM_PACK8;
	case gli::FORMAT_RGBA4_UNORM_PACK16: return VK_FORMAT_R4G4B4A4_UNORM_PACK16;
	case gli::FORMAT_BGRA4_UNORM_PACK16: return VK_FORMAT_B4G4R4A4_UNORM_PACK16;
	case gli::FORMAT_R5G6B5_UNORM_PACK16: return VK_FORMAT_R5G6B5_UNORM_PACK16;
	case gli::FORMAT_B5G6R5_UNORM_PACK16: return VK_FORMAT_B5G6R5_UNORM_PACK16;
	case gli::FORMAT_RGB5A1_UNORM_PACK16: return VK_FORMAT_R5G5B5A1_UNORM_PACK16;
	case gli::FORMAT_BGR5A1_UNORM_PACK16: return VK_FORMAT_B5G5R5A1_UNORM_PACK16;
	case gli::FORMAT_A1RGB5_UNORM_PACK16: return VK_FORMAT_A1R5G5B5_UNORM_PACK16;
	case gli::FORMAT_R8_UNORM_PACK8: return VK_FORMAT_R8_UNORM;
	case gli::FORMAT_R8_SNORM_PACK8: return VK_FORMAT_R8_SNORM;
	case gli::FORMAT_R8_USCALED_PACK8: return VK_FORMAT_R8_USCALED;
	case gli::FORMAT_R8_SSCALED_PACK8: return VK_FORMAT_R8_SSCALED;
	case gli::FORMAT_R8_UINT_PACK8: return VK_FORMAT_R8_UINT;
	case gli::FORMAT_R8_SINT_PACK8: return VK_FORMAT_R8_SINT;
	case gli::FORMAT_R8_SRGB_PACK8: return VK_FORMAT_R8_SRGB;
	case gli::FORMAT_RG8_UNORM_PACK8: return VK_FORMAT_R8G8_UNORM;
	case gli::FORMAT_RG8_SNORM_PACK8: return VK_FORMAT_R8G8_SNORM;
	case gli::FORMAT_RG8_USCALED_PACK8: return VK_FORMAT_R8G8_USCALED;
	case gli::FORMAT_RG8_SSCALED_PACK8: return VK_FORMAT_R8G8_SSCALED;
	case gli::FORMAT_RG8_UINT_PACK8: return VK_FORMAT_R8G8_UINT;
	case gli::FORMAT_RG8_SINT_PACK8: return VK_FORMAT_R8G8_SINT;
	case gli::FORMAT_RG8_SRGB_PACK8: return VK_FORMAT_R8G8_SRGB;
	case gli::FORMAT_RGB8_UNORM_PACK8: return VK_FORMAT_R8G8B8_UNORM;
	case gli::FORMAT_RGB8_SNORM_PACK8: return VK_FORMAT_R8G8B8_SNORM;
	case gli::FORMAT_RGB8_USCALED_PACK8: return VK_FORMAT_R8G8B8_USCALED;
	case gli::FORMAT_RGB8_SSCALED_PACK8: return VK_FORMAT_R8G8B8_SSCALED;
	case gli::FORMAT_RGB8_UINT_PACK8: return VK_FORMAT_R8G8B8_UINT;
	case gli::FORMAT_RGB8_SINT_PACK8: return VK_FORMAT_R8G8B8_SINT;
	case gli::FORMAT_RGB8_SRGB_PACK8: return VK_FORMAT_R8G8B8_SRGB;
	case gli::FORMAT_BGR8_UNORM_PACK8: return VK_FORMAT_B8G8R8_UNORM;
	case gli::FORMAT_BGR8_SNORM_PACK8: return VK_FORMAT_B8G8R8_SNORM;
	case gli::FORMAT_BGR8_USCALED_PACK8: return VK_FORMAT_B8G8R8_USCALED;
	case gli::FORMAT_BGR8_SSCALED_PACK8: return VK_FORMAT_B8G8R8_SSCALED;
	case gli::FORMAT_BGR8_UINT_PACK8: return VK_FORMAT_B8G8R8_UINT;
	case gli::FORMAT_BGR8_SINT_PACK8: return VK_FORMAT_B8G8R8_SINT;
	case gli::FORMAT_BGR8_SRGB_PACK8: return VK_FORMAT_B8G8R8_SRGB;
	case gli::FORMAT_RGBA8_UNORM_PACK8: return VK_FORMAT_R8G8B8A8_UNORM;
	case gli::FORMAT_RGBA8_SNORM_PACK8: return VK_FORMAT_R8G8B8A8_SNORM;
	case gli::FORMAT_RGBA8_USCALED_PACK8: return VK_FORMAT_R8G8B8A8_USCALED;
	case gli::FORMAT_RGBA8_SSCALED_PACK8: return VK_FORMAT_R8G8B8A8_SSCALED;
	case gli::FORMAT_RGBA8_UINT_PACK8: return VK_FORMAT_R8G8B8A8_UINT;
	case gli::FORMAT_RGBA8_SINT_PACK8: return VK_FORMAT_R8G8B8A8_SINT;
	case gli::FORMAT_RGBA8_SRGB_PACK8: return VK_FORMAT_R8G8B8A8_SRGB;
	case gli::FORMAT_BGRA8_UNORM_PACK8: return VK_FORMAT_B8G8R8A8_UNORM;
	case gli::FORMAT_BGRA8_SNORM_PACK8: return VK_FORMAT_B8G8R8A8_SNORM;
	case gli::FORMAT_BGRA8_USCALED_PACK8: return VK_FORMAT_B8G8R8A8_USCALED;
	case gli::FORMAT_BGRA8_SSCALED_PACK8: return VK_FORMAT_B8G8R8A8_SSCALED;
	case gli::FORMAT_BGRA8_UINT_PACK8: return VK_FORMAT_B8G8R8A8_UINT;
	case gli::FORMAT_BGRA8_SINT_PACK8: return VK_FORMAT_B8G8R8A8_SINT;
	case gli::FORMAT_BGRA8_SRGB_PACK8: return VK_FORMAT_B8G8R8A8_SRGB;
	case gli::FORMAT_RGBA8_UNORM_PACK32: return VK_FORMAT_R8G8B8A8_UNORM;
	case gli::FORMAT_RGBA8_SNORM_PACK32: return VK_FORMAT_R8G8B8A8_SNORM;
	case gli::FORMAT_RGBA8_USCALED_PACK32: return VK_FORMAT_R8G8B8A8_USCALED;
	case gli::FORMAT_RGBA8_SSCALED_PACK32: return VK_FORMAT_R8G8B8A8_SSCALED;
	case gli::FORMAT_RGBA8_UINT_PACK32: return VK_FORMAT_R8G8B8A8_UINT;
	case gli::FORMAT_RGBA8_SINT_PACK32: return VK_FORMAT_R8G8B8A8_SINT;
	case gli::FORMAT_RGBA8_SRGB_PACK32: return VK_FORMAT_R8G8B8A8_SRGB;
	case gli::FORMAT_R16_UNORM_PACK16: return VK_FORMAT_R16_UNORM;
	case gli::FORMAT_R16_SNORM_PACK16: return VK_FORMAT_R16_SNORM;
	case gli::FORMAT_R16_USCALED_PACK16: return VK_FORMAT_R16_USCALED;
	case gli::FORMAT_R16_SSCALED_PACK16: return VK_FORMAT_R16_SSCALED;
	case gli::FORMAT_R16_UINT_PACK16: return VK_FORMAT_R16_UINT;
	case gli::FORMAT_R16_SINT_PACK16: return VK_FORMAT_R16_SINT;
	case gli::FORMAT_R16_SFLOAT_PACK16: return VK_FORMAT_R16_SFLOAT;
	case gli::FORMAT_RG16_UNORM_PACK16: return VK_FORMAT_R16_UNORM;
	case gli::FORMAT_RG16_SNORM_PACK16: return VK_FORMAT_R16_SNORM;
	case gli::FORMAT_RG16_USCALED_PACK16: return VK_FORMAT_R16G16_USCALED;
	case gli::FORMAT_RG16_SSCALED_PACK16: return VK_FORMAT_R16G16_SSCALED;
	case gli::FORMAT_RG16_UINT_PACK16: return VK_FORMAT_R16G16_UINT;
	case gli::FORMAT_RG16_SINT_PACK16: return VK_FORMAT_R16G16_SINT;
	case gli::FORMAT_RG16_SFLOAT_PACK16: return VK_FORMAT_R16G16_SFLOAT;
	case gli::FORMAT_RGB16_UNORM_PACK16: return VK_FORMAT_R16G16B16_UNORM;
	case gli::FORMAT_RGB16_SNORM_PACK16: return VK_FORMAT_R16G16B16_SNORM;
	case gli::FORMAT_RGB16_USCALED_PACK16: return VK_FORMAT_R16G16B16_USCALED;
	case gli::FORMAT_RGB16_SSCALED_PACK16: return VK_FORMAT_R16G16B16_SSCALED;
	case gli::FORMAT_RGB16_UINT_PACK16: return VK_FORMAT_R16G16B16_UINT;
	case gli::FORMAT_RGB16_SINT_PACK16: return VK_FORMAT_R16G16B16_SINT;
	case gli::FORMAT_RGB16_SFLOAT_PACK16: return VK_FORMAT_R16G16B16_SFLOAT;
	case gli::FORMAT_RGBA16_UNORM_PACK16: return VK_FORMAT_R16G16B16A16_UNORM;
	case gli::FORMAT_RGBA16_SNORM_PACK16: return VK_FORMAT_R16G16B16_SNORM;
	case gli::FORMAT_RGBA16_USCALED_PACK16: return VK_FORMAT_R16G16B16A16_USCALED;
	case gli::FORMAT_RGBA16_SSCALED_PACK16: return VK_FORMAT_R16G16B16A16_SSCALED;
	case gli::FORMAT_RGBA16_UINT_PACK16: return VK_FORMAT_R16G16B16A16_UINT;
	case gli::FORMAT_RGBA16_SINT_PACK16: return VK_FORMAT_R16G16B16A16_SINT;
	case gli::FORMAT_RGBA16_SFLOAT_PACK16: return VK_FORMAT_R16G16B16A16_SFLOAT;
	case gli::FORMAT_R32_UINT_PACK32: return VK_FORMAT_R32_UINT;
	case gli::FORMAT_R32_SINT_PACK32: return VK_FORMAT_R32_SINT;
	case gli::FORMAT_R32_SFLOAT_PACK32: return VK_FORMAT_R32_SFLOAT;
	case gli::FORMAT_RG32_UINT_PACK32: return VK_FORMAT_R32G32_UINT;
	case gli::FORMAT_RG32_SINT_PACK32: return VK_FORMAT_R32G32_SINT;
	case gli::FORMAT_RG32_SFLOAT_PACK32: return VK_FORMAT_R32G32_SFLOAT;
	case gli::FORMAT_RGB32_UINT_PACK32: return VK_FORMAT_R32G32B32_UINT;
	case gli::FORMAT_RGB32_SINT_PACK32: return VK_FORMAT_R32G32B32_SINT;
	case gli::FORMAT_RGB32_SFLOAT_PACK32: return VK_FORMAT_R32G32B32_SFLOAT;
	case gli::FORMAT_RGBA32_UINT_PACK32: return VK_FORMAT_R32G32B32A32_UINT;
	case gli::FORMAT_RGBA32_SINT_PACK32: return VK_FORMAT_R32G32B32A32_SINT;
	case gli::FORMAT_RGBA32_SFLOAT_PACK32: return VK_FORMAT_R32G32B32A32_SFLOAT;
	case gli::FORMAT_R64_UINT_PACK64: return VK_FORMAT_R64_UINT;
	case gli::FORMAT_R64_SINT_PACK64: return VK_FORMAT_R64_SINT;
	case gli::FORMAT_R64_SFLOAT_PACK64: return VK_FORMAT_R64_SFLOAT;
	case gli::FORMAT_RG64_UINT_PACK64: return VK_FORMAT_R64G64_UINT;
	case gli::FORMAT_RG64_SINT_PACK64: return VK_FORMAT_R64G64_SINT;
	case gli::FORMAT_RG64_SFLOAT_PACK64: return VK_FORMAT_R64G64_SFLOAT;
	case gli::FORMAT_RGB64_UINT_PACK64: return VK_FORMAT_R64G64B64_UINT;
	case gli::FORMAT_RGB64_SINT_PACK64: return VK_FORMAT_R64G64B64_SINT;
	case gli::FORMAT_RGB64_SFLOAT_PACK64: return VK_FORMAT_R64G64B64_SFLOAT;
	case gli::FORMAT_RGBA64_UINT_PACK64: return VK_FORMAT_R64G64B64A64_UINT;
	case gli::FORMAT_RGBA64_SINT_PACK64: return VK_FORMAT_R64G64B64A64_SINT;
	case gli::FORMAT_RGBA64_SFLOAT_PACK64: return VK_FORMAT_R64G64B64A64_SFLOAT;
	case gli::FORMAT_D16_UNORM_PACK16: return VK_FORMAT_D16_UNORM;
	case gli::FORMAT_D32_SFLOAT_PACK32: return VK_FORMAT_D32_SFLOAT;
	case gli::FORMAT_S8_UINT_PACK8: return VK_FORMAT_S8_UINT;
	case gli::FORMAT_D16_UNORM_S8_UINT_PACK32: return VK_FORMAT_D16_UNORM_S8_UINT;
	case gli::FORMAT_D24_UNORM_S8_UINT_PACK32: return VK_FORMAT_D24_UNORM_S8_UINT;
	case gli::FORMAT_D32_SFLOAT_S8_UINT_PACK64: return VK_FORMAT_D32_SFLOAT_S8_UINT;
	case gli::FORMAT_RGB_DXT1_UNORM_BLOCK8: return VK_FORMAT_BC1_RGB_UNORM_BLOCK;
	case gli::FORMAT_RGB_DXT1_SRGB_BLOCK8: return VK_FORMAT_BC1_RGB_SRGB_BLOCK;
	case gli::FORMAT_RGBA_DXT1_UNORM_BLOCK8: return VK_FORMAT_BC1_RGBA_UNORM_BLOCK;
	case gli::FORMAT_RGBA_DXT1_SRGB_BLOCK8: return VK_FORMAT_BC1_RGBA_SRGB_BLOCK;
	case gli::FORMAT_RGBA_DXT3_UNORM_BLOCK16: return VK_FORMAT_BC2_UNORM_BLOCK;
	case gli::FORMAT_RGBA_DXT3_SRGB_BLOCK16: return VK_FORMAT_BC2_SRGB_BLOCK;
	case gli::FORMAT_RGBA_DXT5_UNORM_BLOCK16: return VK_FORMAT_BC3_UNORM_BLOCK;
	case gli::FORMAT_RGBA_DXT5_SRGB_BLOCK16: return VK_FORMAT_BC3_SRGB_BLOCK;
	case gli::FORMAT_RGB_ETC2_UNORM_BLOCK8: return VK_FORMAT_ETC2_R8G8B8_UNORM_BLOCK;
	case gli::FORMAT_RGB_ETC2_SRGB_BLOCK8: return VK_FORMAT_ETC2_R8G8B8_SRGB_BLOCK;
	case gli::FORMAT_RGBA_ETC2_UNORM_BLOCK8: return VK_FORMAT_ETC2_R8G8B8A8_UNORM_BLOCK;
	case gli::FORMAT_RGBA_ETC2_SRGB_BLOCK8: return VK_FORMAT_ETC2_R8G8B8A8_SRGB_BLOCK;
	case gli::FORMAT_R_EAC_UNORM_BLOCK8: return VK_FORMAT_EAC_R11_UNORM_BLOCK;
	case gli::FORMAT_R_EAC_SNORM_BLOCK8: return VK_FORMAT_EAC_R11_SNORM_BLOCK;
	case gli::FORMAT_RG_EAC_UNORM_BLOCK16: return VK_FORMAT_EAC_R11G11_UNORM_BLOCK;
	case gli::FORMAT_RG_EAC_SNORM_BLOCK16: return VK_FORMAT_EAC_R11G11_SNORM_BLOCK;
	case gli::FORMAT_RGBA_ASTC_4X4_UNORM_BLOCK16: return VK_FORMAT_ASTC_4x4_UNORM_BLOCK;
	case gli::FORMAT_RGBA_ASTC_4X4_SRGB_BLOCK16: return VK_FORMAT_ASTC_4x4_SRGB_BLOCK;
	case gli::FORMAT_RGBA_ASTC_5X4_UNORM_BLOCK16: return VK_FORMAT_ASTC_5x4_UNORM_BLOCK;
	case gli::FORMAT_RGBA_ASTC_5X4_SRGB_BLOCK16: return VK_FORMAT_ASTC_5x4_SRGB_BLOCK;
	case gli::FORMAT_RGBA_ASTC_5X5_UNORM_BLOCK16: return VK_FORMAT_ASTC_5x5_UNORM_BLOCK;
	case gli::FORMAT_RGBA_ASTC_5X5_SRGB_BLOCK16: return VK_FORMAT_ASTC_5x5_SRGB_BLOCK;
	case gli::FORMAT_RGBA_ASTC_6X5_UNORM_BLOCK16: return VK_FORMAT_ASTC_6x5_UNORM_BLOCK;
	case gli::FORMAT_RGBA_ASTC_6X5_SRGB_BLOCK16: return VK_FORMAT_ASTC_6x5_SRGB_BLOCK;
	case gli::FORMAT_RGBA_ASTC_6X6_UNORM_BLOCK16: return VK_FORMAT_ASTC_6x6_UNORM_BLOCK;
	case gli::FORMAT_RGBA_ASTC_6X6_SRGB_BLOCK16: return VK_FORMAT_ASTC_6x6_SRGB_BLOCK;
	case gli::FORMAT_RGBA_ASTC_8X5_UNORM_BLOCK16: return VK_FORMAT_ASTC_8x5_UNORM_BLOCK;
	case gli::FORMAT_RGBA_ASTC_8X5_SRGB_BLOCK16: return VK_FORMAT_ASTC_8x5_SRGB_BLOCK;
	case gli::FORMAT_RGBA_ASTC_8X6_UNORM_BLOCK16: return VK_FORMAT_ASTC_8x6_UNORM_BLOCK;
	case gli::FORMAT_RGBA_ASTC_8X6_SRGB_BLOCK16: return VK_FORMAT_ASTC_8x6_SRGB_BLOCK;
	case gli::FORMAT_RGBA_ASTC_8X8_UNORM_BLOCK16: return VK_FORMAT_ASTC_8x8_UNORM_BLOCK;
	case gli::FORMAT_RGBA_ASTC_8X8_SRGB_BLOCK16: return VK_FORMAT_ASTC_8x8_SRGB_BLOCK;
	case gli::FORMAT_RGBA_ASTC_10X5_UNORM_BLOCK16: return VK_FORMAT_ASTC_10x5_UNORM_BLOCK;
	case gli::FORMAT_RGBA_ASTC_10X5_SRGB_BLOCK16: return VK_FORMAT_ASTC_10x5_SRGB_BLOCK;
	case gli::FORMAT_RGBA_ASTC_10X6_UNORM_BLOCK16: return VK_FORMAT_ASTC_10x6_UNORM_BLOCK;
	case gli::FORMAT_RGBA_ASTC_10X6_SRGB_BLOCK16: return VK_FORMAT_ASTC_10x6_SRGB_BLOCK;
	case gli::FORMAT_RGBA_ASTC_10X8_UNORM_BLOCK16: return VK_FORMAT_ASTC_10x8_UNORM_BLOCK;
	case gli::FORMAT_RGBA_ASTC_10X8_SRGB_BLOCK16: return VK_FORMAT_ASTC_10x8_SRGB_BLOCK;
	case gli::FORMAT_RGBA_ASTC_10X10_UNORM_BLOCK16: return VK_FORMAT_ASTC_10x10_UNORM_BLOCK;
	case gli::FORMAT_RGBA_ASTC_10X10_SRGB_BLOCK16: return VK_FORMAT_ASTC_10x10_SRGB_BLOCK;
	case gli::FORMAT_RGBA_ASTC_12X10_UNORM_BLOCK16: return VK_FORMAT_ASTC_12x10_UNORM_BLOCK;
	case gli::FORMAT_RGBA_ASTC_12X10_SRGB_BLOCK16: return VK_FORMAT_ASTC_12x10_SRGB_BLOCK;
	case gli::FORMAT_RGBA_ASTC_12X12_UNORM_BLOCK16: return VK_FORMAT_ASTC_12x12_UNORM_BLOCK;
	case gli::FORMAT_RGBA_ASTC_12X12_SRGB_BLOCK16: return VK_FORMAT_ASTC_12x12_SRGB_BLOCK;
	case gli::FORMAT_RGB_PVRTC1_8X8_UNORM_BLOCK32: return VK_FORMAT_ASTC_8x8_UNORM_BLOCK;
	case gli::FORMAT_L8_UNORM_PACK8:	case gli::FORMAT_A8_UNORM_PACK8: return VK_FORMAT_R8_UNORM;
	case gli::FORMAT_LA8_UNORM_PACK8: return VK_FORMAT_R8G8_UNORM;
	case gli::FORMAT_L16_UNORM_PACK16:	case gli::FORMAT_A16_UNORM_PACK16: return VK_FORMAT_R16_UNORM;
	case gli::FORMAT_LA16_UNORM_PACK16: return VK_FORMAT_R16G16_UNORM;
	case gli::FORMAT_BGR8_UNORM_PACK32: return VK_FORMAT_B8G8R8_UNORM;
	case gli::FORMAT_BGR8_SRGB_PACK32: return VK_FORMAT_B8G8R8_SRGB;
	case gli::FORMAT_D24_UNORM_PACK32: return VK_FORMAT_X8_D24_UNORM_PACK32;
	case gli::FORMAT_R_ATI1N_UNORM_BLOCK8: return VK_FORMAT_BC4_UNORM_BLOCK;
	case gli::FORMAT_R_ATI1N_SNORM_BLOCK8: return VK_FORMAT_BC4_SNORM_BLOCK;
	case gli::FORMAT_RG_ATI2N_UNORM_BLOCK16: return VK_FORMAT_BC5_UNORM_BLOCK;
	case gli::FORMAT_RG_ATI2N_SNORM_BLOCK16: return VK_FORMAT_BC5_SNORM_BLOCK;
	case gli::FORMAT_RGB_BP_UFLOAT_BLOCK16: return VK_FORMAT_BC6H_UFLOAT_BLOCK;
	case gli::FORMAT_RGB_BP_SFLOAT_BLOCK16: return VK_FORMAT_BC6H_SFLOAT_BLOCK;
	case gli::FORMAT_RGBA_BP_UNORM_BLOCK16: return VK_FORMAT_BC7_UNORM_BLOCK;
	case gli::FORMAT_RGBA_BP_SRGB_BLOCK16: return VK_FORMAT_BC7_SRGB_BLOCK;
	case gli::FORMAT_RGBA_ETC2_UNORM_BLOCK16: return VK_FORMAT_ETC2_R8G8B8A8_UNORM_BLOCK;
	case gli::FORMAT_RGBA_ETC2_SRGB_BLOCK16: return VK_FORMAT_ETC2_R8G8B8A8_SRGB_BLOCK;


	// order different but same components (TODO experimental)
	//case gli::FORMAT_RGB10A2_UNORM_PACK32: 
	//	return VK_FORMAT_A2R10G10B10_UNORM_PACK32;
	//case gli::FORMAT_RGB10A2_SNORM_PACK32:
	//	return VK_FORMAT_A2R10G10B10_SNORM_PACK32;
	//case gli::FORMAT_RGB10A2_USCALED_PACK32:
	//	return VK_FORMAT_A2R10G10B10_USCALED_PACK32;
	//case gli::FORMAT_RGB10A2_SSCALED_PACK32:
	//	return VK_FORMAT_A2R10G10B10_SSCALED_PACK32;
	case gli::FORMAT_RGB10A2_UINT_PACK32:
		return VK_FORMAT_A2R10G10B10_UINT_PACK32;
	case gli::FORMAT_RGB10A2_SINT_PACK32:
		return VK_FORMAT_A2R10G10B10_SINT_PACK32;
	case gli::FORMAT_BGR10A2_UNORM_PACK32:
		return VK_FORMAT_A2B10G10R10_UNORM_PACK32;
	case gli::FORMAT_BGR10A2_SNORM_PACK32:
		return VK_FORMAT_A2B10G10R10_SNORM_PACK32;
	//case gli::FORMAT_BGR10A2_USCALED_PACK32:
	//	return VK_FORMAT_A2B10G10R10_USCALED_PACK32;
	//case gli::FORMAT_BGR10A2_SSCALED_PACK32:
	//	return VK_FORMAT_A2B10G10R10_SSCALED_PACK32;
	case gli::FORMAT_BGR10A2_UINT_PACK32:
		return VK_FORMAT_A2B10G10R10_UINT_PACK32;
	case gli::FORMAT_BGR10A2_SINT_PACK32:
		return VK_FORMAT_A2B10G10R10_SINT_PACK32;

	//case gli::FORMAT_RG11B10_UFLOAT_PACK32:
	//case gli::FORMAT_RGB9E5_UFLOAT_PACK32: 
	//	return VK_FORMAT_E5B9G9R9_UFLOAT_PACK32;
	
	case gli::FORMAT_RGB_PVRTC1_8X8_SRGB_BLOCK32: 
	case gli::FORMAT_RGB_PVRTC1_16X8_UNORM_BLOCK32: 
	case gli::FORMAT_RGB_PVRTC1_16X8_SRGB_BLOCK32: 
	case gli::FORMAT_RGBA_PVRTC1_8X8_UNORM_BLOCK32: 
	case gli::FORMAT_RGBA_PVRTC1_8X8_SRGB_BLOCK32: 
	case gli::FORMAT_RGBA_PVRTC1_16X8_UNORM_BLOCK32: 
	case gli::FORMAT_RGBA_PVRTC1_16X8_SRGB_BLOCK32: 
	case gli::FORMAT_RGBA_PVRTC2_4X4_UNORM_BLOCK8: 
	case gli::FORMAT_RGBA_PVRTC2_4X4_SRGB_BLOCK8: 
	case gli::FORMAT_RGBA_PVRTC2_8X4_UNORM_BLOCK8: 
	case gli::FORMAT_RGBA_PVRTC2_8X4_SRGB_BLOCK8: 
	case gli::FORMAT_RGB_ETC_UNORM_BLOCK8: 
	case gli::FORMAT_RGB_ATC_UNORM_BLOCK8: 
	case gli::FORMAT_RGBA_ATCA_UNORM_BLOCK16: 
	case gli::FORMAT_RGBA_ATCI_UNORM_BLOCK16: 
	case gli::FORMAT_RG3B2_UNORM_PACK8:
	default: break;
	}

	return VK_FORMAT_UNDEFINED;
}

std::vector<uint32_t> ktx_get_export_formats()
{
	return std::vector<uint32_t>{

		// uniform
		gli::format::FORMAT_RG3B2_UNORM_PACK8,
			gli::format::FORMAT_RGBA4_UNORM_PACK16,
			gli::format::FORMAT_BGRA4_UNORM_PACK16,
			gli::format::FORMAT_R5G6B5_UNORM_PACK16,
			gli::format::FORMAT_B5G6R5_UNORM_PACK16,
			//gli::format::FORMAT_RGB5A1_UNORM_PACK16,
			//gli::format::FORMAT_BGR5A1_UNORM_PACK16,
			gli::format::FORMAT_R8_UNORM_PACK8,
			gli::format::FORMAT_R8_SNORM_PACK8,
			gli::FORMAT_R8_UINT_PACK8,
			gli::FORMAT_R8_SINT_PACK8,
			gli::format::FORMAT_RG8_UNORM_PACK8,
			gli::format::FORMAT_RG8_SNORM_PACK8,
			gli::FORMAT_RG8_UINT_PACK8,
			gli::FORMAT_RG8_SINT_PACK8,
			gli::format::FORMAT_RGB8_UNORM_PACK8,
			gli::format::FORMAT_RGB8_SNORM_PACK8,
			gli::format::FORMAT_RGB8_SRGB_PACK8,
			gli::FORMAT_RGB8_UINT_PACK8,
			gli::FORMAT_RGB8_SINT_PACK8,
			gli::format::FORMAT_BGR8_UNORM_PACK8,
			gli::format::FORMAT_BGR8_SNORM_PACK8,
			gli::format::FORMAT_BGR8_SRGB_PACK8,
			// those give some block size mismatch error from gli:
			//gli::FORMAT_BGR8_UINT_PACK8,
			//gli::FORMAT_BGR8_SINT_PACK8,
			gli::format::FORMAT_RGBA8_UNORM_PACK8,
			gli::format::FORMAT_RGBA8_SNORM_PACK8,
			gli::format::FORMAT_RGBA8_SRGB_PACK8,
			gli::FORMAT_RGBA8_UINT_PACK8,
			gli::FORMAT_RGBA8_SINT_PACK8,
			gli::format::FORMAT_BGRA8_UNORM_PACK8,
			gli::format::FORMAT_BGRA8_SNORM_PACK8,
			gli::format::FORMAT_BGRA8_SRGB_PACK8,
			gli::FORMAT_BGRA8_UINT_PACK8,
			gli::FORMAT_BGRA8_SINT_PACK8,
			gli::format::FORMAT_RGBA8_UNORM_PACK32,
			gli::format::FORMAT_RGBA8_SNORM_PACK32,
			gli::format::FORMAT_RGBA8_SRGB_PACK32,
			gli::FORMAT_RGBA8_UINT_PACK32,
			gli::FORMAT_RGBA8_SINT_PACK32,
			gli::format::FORMAT_RGB10A2_UNORM_PACK32,
			//gli::FORMAT_RGB10A2_UINT_PACK32, // no gl format in core
			//gli::FORMAT_RGB10A2_SINT_PACK32,
			//gli::format::FORMAT_BGR10A2_UNORM_PACK32,
			//gli::format::FORMAT_BGR10A2_SNORM_PACK32,
			//gli::FORMAT_BGR10A2_UINT_PACK32,
			//gli::FORMAT_BGR10A2_SINT_PACK32,
			//gli::format::FORMAT_A8_UNORM_PACK8,
			//gli::format::FORMAT_A16_UNORM_PACK16,
			//gli::format::FORMAT_BGR8_UNORM_PACK32,
			//gli::format::FORMAT_BGR8_SRGB_PACK32,

			// float formats
			gli::format::FORMAT_R16_UNORM_PACK16,
			gli::format::FORMAT_R16_SNORM_PACK16,
			gli::FORMAT_R16_UINT_PACK16,
			gli::FORMAT_R16_SINT_PACK16,
			gli::format::FORMAT_R16_SFLOAT_PACK16,
			gli::format::FORMAT_RG16_UNORM_PACK16,
			gli::format::FORMAT_RG16_SNORM_PACK16,
			gli::format::FORMAT_RG16_SFLOAT_PACK16,
			gli::FORMAT_RG16_UINT_PACK16,
			gli::FORMAT_RG16_SINT_PACK16,
			gli::format::FORMAT_RGB16_UNORM_PACK16,
			gli::format::FORMAT_RGB16_SNORM_PACK16,
			gli::format::FORMAT_RGB16_SFLOAT_PACK16,
			gli::FORMAT_RGB16_UINT_PACK16,
			gli::FORMAT_RGB16_SINT_PACK16,
			gli::format::FORMAT_RGBA16_UNORM_PACK16,
			gli::format::FORMAT_RGBA16_SNORM_PACK16,
			gli::format::FORMAT_RGBA16_SFLOAT_PACK16,
			gli::FORMAT_RGBA16_UINT_PACK16,
			gli::FORMAT_RGBA16_SINT_PACK16,
			gli::format::FORMAT_R32_SFLOAT_PACK32,
			gli::FORMAT_R32_UINT_PACK32,
			gli::FORMAT_R32_SINT_PACK32,
			gli::format::FORMAT_RG32_SFLOAT_PACK32,
			gli::FORMAT_RG32_UINT_PACK32,
			gli::FORMAT_RG32_SINT_PACK32,
			gli::format::FORMAT_RGB32_SFLOAT_PACK32,
			gli::FORMAT_RGB32_UINT_PACK32,
			gli::FORMAT_RGB32_SINT_PACK32,
			gli::format::FORMAT_RGBA32_SFLOAT_PACK32,
			gli::FORMAT_RGBA32_UINT_PACK32,
			gli::FORMAT_RGBA32_SINT_PACK32,
			gli::format::FORMAT_RG11B10_UFLOAT_PACK32,
			gli::format::FORMAT_RGB9E5_UFLOAT_PACK32,

			// dds compressed
			// DXT
			gli::format::FORMAT_RGB_DXT1_UNORM_BLOCK8,
			gli::format::FORMAT_RGB_DXT1_SRGB_BLOCK8,
			gli::format::FORMAT_RGBA_DXT1_UNORM_BLOCK8,
			gli::format::FORMAT_RGBA_DXT1_SRGB_BLOCK8,
			gli::format::FORMAT_RGBA_DXT3_UNORM_BLOCK16,
			gli::format::FORMAT_RGBA_DXT3_SRGB_BLOCK16,
			gli::format::FORMAT_RGBA_DXT5_SRGB_BLOCK16,
			gli::format::FORMAT_RGBA_DXT5_UNORM_BLOCK16,
			gli::format::FORMAT_R_ATI1N_UNORM_BLOCK8,
			gli::format::FORMAT_R_ATI1N_SNORM_BLOCK8,
			gli::format::FORMAT_RG_ATI2N_UNORM_BLOCK16,
			gli::format::FORMAT_RG_ATI2N_SNORM_BLOCK16,
			gli::format::FORMAT_RGB_BP_UFLOAT_BLOCK16,
			gli::format::FORMAT_RGB_BP_SFLOAT_BLOCK16,
			gli::format::FORMAT_RGBA_BP_UNORM_BLOCK16,
			gli::format::FORMAT_RGBA_BP_SRGB_BLOCK16,
			/*gli::format::FORMAT_RGB_ETC2_UNORM_BLOCK8,
			gli::format::FORMAT_RGB_ETC2_SRGB_BLOCK8,
			gli::format::FORMAT_RGBA_ETC2_UNORM_BLOCK8,
			gli::format::FORMAT_RGBA_ETC2_SRGB_BLOCK8,
			gli::format::FORMAT_RGBA_ETC2_UNORM_BLOCK16,
			gli::format::FORMAT_RGBA_ETC2_SRGB_BLOCK16,
			gli::format::FORMAT_R_EAC_UNORM_BLOCK8,
			gli::format::FORMAT_R_EAC_SNORM_BLOCK8,
			gli::format::FORMAT_RG_EAC_UNORM_BLOCK16,
			gli::format::FORMAT_RG_EAC_SNORM_BLOCK16,
			gli::format::FORMAT_RGBA_ASTC_4X4_UNORM_BLOCK16,
			gli::format::FORMAT_RGBA_ASTC_4X4_SRGB_BLOCK16,
			gli::format::FORMAT_RGBA_ASTC_5X4_UNORM_BLOCK16,
			gli::format::FORMAT_RGBA_ASTC_5X4_SRGB_BLOCK16,
			gli::format::FORMAT_RGBA_ASTC_5X5_UNORM_BLOCK16,
			gli::format::FORMAT_RGBA_ASTC_5X5_SRGB_BLOCK16,
			gli::format::FORMAT_RGBA_ASTC_6X5_UNORM_BLOCK16,
			gli::format::FORMAT_RGBA_ASTC_6X5_SRGB_BLOCK16,
			gli::format::FORMAT_RGBA_ASTC_6X6_UNORM_BLOCK16,
			gli::format::FORMAT_RGBA_ASTC_6X6_SRGB_BLOCK16,
			gli::format::FORMAT_RGBA_ASTC_8X5_UNORM_BLOCK16,
			gli::format::FORMAT_RGBA_ASTC_8X5_SRGB_BLOCK16,
			gli::format::FORMAT_RGBA_ASTC_8X6_UNORM_BLOCK16,
			gli::format::FORMAT_RGBA_ASTC_8X6_SRGB_BLOCK16,
			gli::format::FORMAT_RGBA_ASTC_8X8_UNORM_BLOCK16,
			gli::format::FORMAT_RGBA_ASTC_8X8_SRGB_BLOCK16,
			gli::format::FORMAT_RGBA_ASTC_10X5_UNORM_BLOCK16,
			gli::format::FORMAT_RGBA_ASTC_10X5_SRGB_BLOCK16,
			gli::format::FORMAT_RGBA_ASTC_10X6_UNORM_BLOCK16,
			gli::format::FORMAT_RGBA_ASTC_10X6_SRGB_BLOCK16,
			gli::format::FORMAT_RGBA_ASTC_10X8_UNORM_BLOCK16,
			gli::format::FORMAT_RGBA_ASTC_10X8_SRGB_BLOCK16,
			gli::format::FORMAT_RGBA_ASTC_10X10_UNORM_BLOCK16,
			gli::format::FORMAT_RGBA_ASTC_10X10_SRGB_BLOCK16,
			gli::format::FORMAT_RGBA_ASTC_12X10_UNORM_BLOCK16,
			gli::format::FORMAT_RGBA_ASTC_12X10_SRGB_BLOCK16,
			gli::format::FORMAT_RGBA_ASTC_12X12_UNORM_BLOCK16,
			gli::format::FORMAT_RGBA_ASTC_12X12_SRGB_BLOCK16,
			gli::format::FORMAT_RGB_PVRTC1_8X8_UNORM_BLOCK32,
			gli::format::FORMAT_RGB_PVRTC1_8X8_SRGB_BLOCK32,
			gli::format::FORMAT_RGB_PVRTC1_16X8_UNORM_BLOCK32,
			gli::format::FORMAT_RGB_PVRTC1_16X8_SRGB_BLOCK32,
			gli::format::FORMAT_RGBA_PVRTC1_8X8_UNORM_BLOCK32,
			gli::format::FORMAT_RGBA_PVRTC1_8X8_SRGB_BLOCK32,
			gli::format::FORMAT_RGBA_PVRTC1_16X8_UNORM_BLOCK32,
			gli::format::FORMAT_RGBA_PVRTC1_16X8_SRGB_BLOCK32,
			gli::format::FORMAT_RGBA_PVRTC2_4X4_UNORM_BLOCK8,
			gli::format::FORMAT_RGBA_PVRTC2_4X4_SRGB_BLOCK8,
			gli::format::FORMAT_RGBA_PVRTC2_8X4_UNORM_BLOCK8,
			gli::format::FORMAT_RGBA_PVRTC2_8X4_SRGB_BLOCK8,
			gli::format::FORMAT_RGB_ETC_UNORM_BLOCK8,
			gli::format::FORMAT_RGB_ATC_UNORM_BLOCK8,
			gli::format::FORMAT_RGBA_ATCA_UNORM_BLOCK16,
			gli::format::FORMAT_RGBA_ATCI_UNORM_BLOCK16,
			gli::format::FORMAT_L8_UNORM_PACK8,

			gli::format::FORMAT_LA8_UNORM_PACK8,
			gli::format::FORMAT_L16_UNORM_PACK16,
			gli::format::FORMAT_LA16_UNORM_PACK16,
			*/
	};
}

std::vector<uint32_t> ktx2_get_export_formats()
{
	return std::vector<uint32_t>{
	gli::FORMAT_RG4_UNORM_PACK8,
	gli::FORMAT_RGBA4_UNORM_PACK16,
	gli::FORMAT_BGRA4_UNORM_PACK16,
	gli::FORMAT_R5G6B5_UNORM_PACK16,
	gli::FORMAT_B5G6R5_UNORM_PACK16,
	gli::FORMAT_RGB5A1_UNORM_PACK16,
	gli::FORMAT_BGR5A1_UNORM_PACK16,
	gli::FORMAT_A1RGB5_UNORM_PACK16,
	gli::FORMAT_R8_UNORM_PACK8,
	gli::FORMAT_R8_SNORM_PACK8,
	gli::FORMAT_R8_UINT_PACK8,
	gli::FORMAT_R8_SINT_PACK8,
	gli::FORMAT_R8_SRGB_PACK8,
	gli::FORMAT_RG8_UNORM_PACK8,
	gli::FORMAT_RG8_SNORM_PACK8,
	gli::FORMAT_RG8_UINT_PACK8,
	gli::FORMAT_RG8_SINT_PACK8,
	gli::FORMAT_RG8_SRGB_PACK8,
	gli::FORMAT_RGB8_UNORM_PACK8,
	gli::FORMAT_RGB8_SNORM_PACK8,
	gli::FORMAT_RGB8_UINT_PACK8,
	gli::FORMAT_RGB8_SINT_PACK8,
	gli::FORMAT_RGB8_SRGB_PACK8,
	gli::FORMAT_BGR8_UNORM_PACK8,
	gli::FORMAT_BGR8_SNORM_PACK8,
	// those give some block size mismatch error from gli:
	//gli::FORMAT_BGR8_UINT_PACK8,
	//gli::FORMAT_BGR8_SINT_PACK8,
	gli::FORMAT_BGR8_SRGB_PACK8,
	gli::FORMAT_RGBA8_UNORM_PACK8,
	gli::FORMAT_RGBA8_SNORM_PACK8,
	gli::FORMAT_RGBA8_UINT_PACK8,
	gli::FORMAT_RGBA8_SINT_PACK8,
	gli::FORMAT_RGBA8_SRGB_PACK8,
	gli::FORMAT_BGRA8_UNORM_PACK8,
	gli::FORMAT_BGRA8_SNORM_PACK8,
	gli::FORMAT_BGRA8_UINT_PACK8,
	gli::FORMAT_BGRA8_SINT_PACK8,
	gli::FORMAT_BGRA8_SRGB_PACK8,
	gli::FORMAT_RGBA8_UNORM_PACK32,
	gli::FORMAT_RGBA8_SNORM_PACK32,
	gli::FORMAT_RGBA8_UINT_PACK32,
	gli::FORMAT_RGBA8_SINT_PACK32,
	gli::FORMAT_RGBA8_SRGB_PACK32,
	gli::FORMAT_R16_UNORM_PACK16,
	gli::FORMAT_R16_SNORM_PACK16,
	gli::FORMAT_R16_UINT_PACK16,
	gli::FORMAT_R16_SINT_PACK16,
	gli::FORMAT_R16_SFLOAT_PACK16,
	// can not set image data
	//gli::FORMAT_RG16_UNORM_PACK16,
	//gli::FORMAT_RG16_SNORM_PACK16,
	gli::FORMAT_RG16_UINT_PACK16,
	gli::FORMAT_RG16_SINT_PACK16,
	gli::FORMAT_RG16_SFLOAT_PACK16,
	gli::FORMAT_RGB16_UNORM_PACK16,
	gli::FORMAT_RGB16_SNORM_PACK16,
	gli::FORMAT_RGB16_UINT_PACK16,
	gli::FORMAT_RGB16_SINT_PACK16,
	gli::FORMAT_RGB16_SFLOAT_PACK16,
	gli::FORMAT_RGBA16_UNORM_PACK16,
	// gli::FORMAT_RGBA16_SNORM_PACK16, // can not set image data
	gli::FORMAT_RGBA16_UINT_PACK16,
	gli::FORMAT_RGBA16_SINT_PACK16,
	gli::FORMAT_RGBA16_SFLOAT_PACK16,
	gli::FORMAT_R32_UINT_PACK32,
	gli::FORMAT_R32_SINT_PACK32,
	gli::FORMAT_R32_SFLOAT_PACK32,
	gli::FORMAT_RG32_UINT_PACK32,
	gli::FORMAT_RG32_SINT_PACK32,
	gli::FORMAT_RG32_SFLOAT_PACK32,
	gli::FORMAT_RGB32_UINT_PACK32,
	gli::FORMAT_RGB32_SINT_PACK32,
	gli::FORMAT_RGB32_SFLOAT_PACK32,
	gli::FORMAT_RGBA32_UINT_PACK32,
	gli::FORMAT_RGBA32_SINT_PACK32,
	gli::FORMAT_RGBA32_SFLOAT_PACK32,
	gli::FORMAT_R64_UINT_PACK64,
	gli::FORMAT_R64_SINT_PACK64,
	gli::FORMAT_R64_SFLOAT_PACK64,
	gli::FORMAT_RG64_UINT_PACK64,
	gli::FORMAT_RG64_SINT_PACK64,
	gli::FORMAT_RG64_SFLOAT_PACK64,
	gli::FORMAT_RGB64_UINT_PACK64,
	gli::FORMAT_RGB64_SINT_PACK64,
	gli::FORMAT_RGB64_SFLOAT_PACK64,
	gli::FORMAT_RGBA64_UINT_PACK64,
	gli::FORMAT_RGBA64_SINT_PACK64,
	gli::FORMAT_RGBA64_SFLOAT_PACK64,
	//gli::FORMAT_D16_UNORM_PACK16,
	//gli::FORMAT_D32_SFLOAT_PACK32,
	//gli::FORMAT_S8_UINT_PACK8,
	//gli::FORMAT_D16_UNORM_S8_UINT_PACK32,
	//gli::FORMAT_D24_UNORM_S8_UINT_PACK32,
	//gli::FORMAT_D32_SFLOAT_S8_UINT_PACK64,
	gli::FORMAT_RGB_DXT1_UNORM_BLOCK8,
	gli::FORMAT_RGB_DXT1_SRGB_BLOCK8,
	gli::FORMAT_RGBA_DXT1_UNORM_BLOCK8,
	gli::FORMAT_RGBA_DXT1_SRGB_BLOCK8,
	gli::FORMAT_RGBA_DXT3_UNORM_BLOCK16,
	gli::FORMAT_RGBA_DXT3_SRGB_BLOCK16,
	gli::FORMAT_RGBA_DXT5_UNORM_BLOCK16,
	gli::FORMAT_RGBA_DXT5_SRGB_BLOCK16,
	gli::FORMAT_RGB_ETC2_UNORM_BLOCK8,
	gli::FORMAT_RGB_ETC2_SRGB_BLOCK8,
	// compression error: the expected data size does not match the suggested data 
	//gli::FORMAT_RGBA_ETC2_UNORM_BLOCK8,
	//gli::FORMAT_RGBA_ETC2_SRGB_BLOCK8,
	// EAC formats are not supported
	//gli::FORMAT_R_EAC_UNORM_BLOCK8,
	//gli::FORMAT_R_EAC_SNORM_BLOCK8,
	//gli::FORMAT_RG_EAC_UNORM_BLOCK16,
	//gli::FORMAT_RG_EAC_SNORM_BLOCK16,
	gli::FORMAT_RGBA_ASTC_4X4_UNORM_BLOCK16,
	gli::FORMAT_RGBA_ASTC_4X4_SRGB_BLOCK16,
	gli::FORMAT_RGBA_ASTC_5X4_UNORM_BLOCK16,
	gli::FORMAT_RGBA_ASTC_5X4_SRGB_BLOCK16,
	gli::FORMAT_RGBA_ASTC_5X5_UNORM_BLOCK16,
	gli::FORMAT_RGBA_ASTC_5X5_SRGB_BLOCK16,
	gli::FORMAT_RGBA_ASTC_6X5_UNORM_BLOCK16,
	gli::FORMAT_RGBA_ASTC_6X5_SRGB_BLOCK16,
	gli::FORMAT_RGBA_ASTC_6X6_UNORM_BLOCK16,
	gli::FORMAT_RGBA_ASTC_6X6_SRGB_BLOCK16,
	gli::FORMAT_RGBA_ASTC_8X5_UNORM_BLOCK16,
	gli::FORMAT_RGBA_ASTC_8X5_SRGB_BLOCK16,
	gli::FORMAT_RGBA_ASTC_8X6_UNORM_BLOCK16,
	gli::FORMAT_RGBA_ASTC_8X6_SRGB_BLOCK16,
	gli::FORMAT_RGBA_ASTC_8X8_UNORM_BLOCK16,
	gli::FORMAT_RGBA_ASTC_8X8_SRGB_BLOCK16,
	gli::FORMAT_RGBA_ASTC_10X5_UNORM_BLOCK16,
	gli::FORMAT_RGBA_ASTC_10X5_SRGB_BLOCK16,
	gli::FORMAT_RGBA_ASTC_10X6_UNORM_BLOCK16,
	gli::FORMAT_RGBA_ASTC_10X6_SRGB_BLOCK16,
	gli::FORMAT_RGBA_ASTC_10X8_UNORM_BLOCK16,
	gli::FORMAT_RGBA_ASTC_10X8_SRGB_BLOCK16,
	gli::FORMAT_RGBA_ASTC_10X10_UNORM_BLOCK16,
	gli::FORMAT_RGBA_ASTC_10X10_SRGB_BLOCK16,
	gli::FORMAT_RGBA_ASTC_12X10_UNORM_BLOCK16,
	gli::FORMAT_RGBA_ASTC_12X10_SRGB_BLOCK16,
	gli::FORMAT_RGBA_ASTC_12X12_UNORM_BLOCK16,
	gli::FORMAT_RGBA_ASTC_12X12_SRGB_BLOCK16,
	// compressed and not yet supported
	//gli::FORMAT_RGB_PVRTC1_8X8_UNORM_BLOCK32,
	//gli::FORMAT_A8_UNORM_PACK8,
	//gli::FORMAT_LA8_UNORM_PACK8,
	//gli::FORMAT_A16_UNORM_PACK16,
	//gli::FORMAT_LA16_UNORM_PACK16,
	// can not set image data
	//gli::FORMAT_BGR8_UNORM_PACK32,
	//gli::FORMAT_BGR8_SRGB_PACK32,
	//gli::FORMAT_D24_UNORM_PACK32,
	gli::FORMAT_R_ATI1N_UNORM_BLOCK8,
	gli::FORMAT_R_ATI1N_SNORM_BLOCK8,
	gli::FORMAT_RG_ATI2N_UNORM_BLOCK16,
	gli::FORMAT_RG_ATI2N_SNORM_BLOCK16,
	gli::FORMAT_RGB_BP_UFLOAT_BLOCK16,
	gli::FORMAT_RGB_BP_SFLOAT_BLOCK16,
	gli::FORMAT_RGBA_BP_UNORM_BLOCK16,
	gli::FORMAT_RGBA_BP_SRGB_BLOCK16,
	// ETC2 Block16 formats are not supported
	//gli::FORMAT_RGBA_ETC2_UNORM_BLOCK16,
	//gli::FORMAT_RGBA_ETC2_SRGB_BLOCK16,
	};
}