#include "pch.h"
#include "gli_interface.h"
#include <gli/gli.hpp>
#include <gli/gl.hpp>

static gli::dx DX;

bool hasAlpha(DXGI_FORMAT format)
{
	switch (format)
	{
	case DXGI_FORMAT_R32G32B32A32_TYPELESS: 
	case DXGI_FORMAT_R32G32B32A32_FLOAT: 
	case DXGI_FORMAT_R32G32B32A32_UINT: 
	case DXGI_FORMAT_R32G32B32A32_SINT: 
	case DXGI_FORMAT_R16G16B16A16_TYPELESS: 
	case DXGI_FORMAT_R16G16B16A16_FLOAT: 
	case DXGI_FORMAT_R16G16B16A16_UNORM: 
	case DXGI_FORMAT_R16G16B16A16_UINT: 
	case DXGI_FORMAT_R16G16B16A16_SNORM: 
	case DXGI_FORMAT_R16G16B16A16_SINT: 
	case DXGI_FORMAT_R10G10B10A2_TYPELESS: 
	case DXGI_FORMAT_R10G10B10A2_UNORM: 
	case DXGI_FORMAT_R10G10B10A2_UINT: 
	case DXGI_FORMAT_R8G8B8A8_TYPELESS: 
	case DXGI_FORMAT_R8G8B8A8_UNORM: 
	case DXGI_FORMAT_R8G8B8A8_UNORM_SRGB: 
	case DXGI_FORMAT_R8G8B8A8_UINT: 
	case DXGI_FORMAT_R8G8B8A8_SNORM: 
	case DXGI_FORMAT_R8G8B8A8_SINT: 
	case DXGI_FORMAT_A8_UNORM: 
	case DXGI_FORMAT_B5G5R5A1_UNORM: 
	case DXGI_FORMAT_B8G8R8A8_UNORM: 
	case DXGI_FORMAT_B8G8R8A8_TYPELESS: 
	case DXGI_FORMAT_B8G8R8A8_UNORM_SRGB: 
		return true;
	}
	return false;
}

void getImageFormat(image::Format& format, const gli::texture& tex)
{
	const auto dxFormat = DX.translate(tex.format());
	
	// even if the data is stored in srgb space directx reads the data in linear space because of the given texture format
	// therefore this should be false for the image loader
	format.isSrgb = false;
	format.dxgi = DXGI_FORMAT(dxFormat.DXGIFormat.DDS);
	format.hasAlpha = hasAlpha(format.dxgi);
}

std::unique_ptr<image::Image> gli_load(const char* filename)
{
	gli::texture tex = gli::load(filename);
	if (tex.empty())
		throw std::exception("error opening file");

	std::unique_ptr<image::Image> res = std::make_unique<image::Image>();

	// determine image format
	getImageFormat(res->format, tex);

	// put layers and faces together (only case that has layers and faces would be of type CUBE_ARRAY)
	res->layer.assign(tex.layers() * tex.faces(), image::Layer());
	size_t outputLayer = 0;
	for (size_t layer = 0; layer < tex.layers(); ++layer)
	{
		for (size_t face = 0; face < tex.faces(); ++face)
		{
			res->layer.at(outputLayer).mipmaps.assign(tex.levels(), image::Mipmap());
			for (size_t mip = 0; mip < tex.levels(); ++mip)
			{
				// fill mipmap
				image::Mipmap& mipmap = res->layer.at(outputLayer).mipmaps.at(mip);
				mipmap.width = tex.extent(mip).x;
				mipmap.height = tex.extent(mip).y;
				mipmap.depth = tex.extent(mip).z;

				auto data = tex.data(layer, face, mip);
				auto size = tex.size(mip);
				mipmap.bytes.reserve(size);
				mipmap.bytes.assign(reinterpret_cast<char*>(data), reinterpret_cast<char*>(data) + size);
			}
			outputLayer++;
		}
	}

	return res;
}

// intermediate texture for ktx image export
static bool s_useCubemap = false;
static gli::texture2d_array s_textureArray;
static gli::texture_cube s_textureCube;
static size_t s_numLayer;

void gli_create_storage(int format, int width, int height, int layer, int levels)
{
	s_numLayer = layer;

	auto dxFormat = DXGI_FORMAT(format);
	auto gliFormat = DX.find(gli::dx::d3dfmt::D3DFMT_DX10, gli::dx::dxgiFormat(gli::dx::dxgi_format_dds(dxFormat)));

	if (layer == 6)
	{
		// cube map
		s_useCubemap = true;
		s_textureCube = gli::texture_cube(gliFormat, gli::extent2d(width, height), levels);
		if (s_textureCube.empty())
			throw std::runtime_error("could not create texture");
	}
	else
	{
		s_useCubemap = false;
		s_textureArray = gli::texture2d_array(gliFormat, gli::extent2d(width, height), layer, levels);
		if (s_textureArray.empty())
			throw std::runtime_error("could not create texture");
	}
}

void gli_store_level(int layer, int level, const void* data, uint64_t size)
{
	if (s_useCubemap)
	{
		if (s_textureCube.size(level) != size)
			throw std::runtime_error("data size mismatch. Expected "
				+ std::to_string(s_textureCube.size(level)) + " but got " + std::to_string(size));
		memcpy(s_textureCube.data(0, layer, level), data, size);
	}
	else
	{
		if (s_textureArray.size(level) != size)
			throw std::runtime_error("data size mismatch. Expected "
				+ std::to_string(s_textureArray.size(level)) + " but got " + std::to_string(size));
		memcpy(s_textureArray.data(layer, 0, level), data, size);
	}
}

void gli_get_level_size(int level, uint64_t& size)
{
	if (s_useCubemap)
	{
		size = s_textureCube.size(level);
	}
	else
	{
		size = s_textureArray.size(level);
	}
}

void gli_save_ktx(const char* filename)
{
	if (s_useCubemap)
	{
		if (!gli::save_ktx(s_textureCube, filename))
		{
			// clear texture
			s_textureCube = gli::texture_cube();
			throw std::exception("could not save file");
		}
		// clear texture
		s_textureCube = gli::texture_cube();
	}
	else
	{
		if (!gli::save_ktx(s_textureArray, filename))
		{
			// clear texture
			s_textureArray = gli::texture2d_array();
			throw std::exception("could not save file");
		}
		// clear texture
		s_textureArray = gli::texture2d_array();
	}
}

void gli_save_dds(const char* filename)
{
	if (s_useCubemap)
	{
		if (!gli::save_dds(s_textureCube, filename))
		{
			// clear texture
			s_textureCube = gli::texture_cube();
			throw std::exception("could not save file");
		}
		// clear texture
		s_textureCube = gli::texture_cube();
	}
	else
	{
		if (!gli::save_dds(s_textureArray, filename))
		{
			// clear texture
			s_textureArray = gli::texture2d_array();
			throw std::exception("could not save file");
		}
		// clear texture
		s_textureArray = gli::texture2d_array();
	}
}