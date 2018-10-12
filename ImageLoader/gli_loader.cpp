#include "gli_loader.h"
#include <gli/gli.hpp>
#include <gli/gl.hpp>


void getImageFormat(ImageFormat& format, const gli::texture& tex)
{
	gli::gl GL(gli::gl::PROFILE_GL33);
	const auto GLformat = GL.translate(tex.format(), tex.swizzles());
	format.openglInternalFormat = static_cast<uint32_t>(GLformat.Internal);
	format.openglExternalFormat = static_cast<uint32_t>(GLformat.External);
	format.openglType = static_cast<uint32_t>(GLformat.Type);
	format.isCompressed = gli::is_compressed(tex.format());
	//format.isSrgb = gli::is_srgb(tex.format());
	// even if the data is stored in srgb space opengl reads the data in linear space because of the given texture format
	// therefore this should be false for the image loader
	format.isSrgb = false;
}

void gli_to_opengl_format(int gliFormat, int& glInternal, int& glExternal, int& glType, bool& isCompressed, bool& isSrgb)
{
	gli::gl GL(gli::gl::PROFILE_GL33);
	// assume no swizzeling
	auto f = gli::format(gliFormat);
	const auto GLFormat = GL.translate(f, gli::swizzles(gli::swizzle::SWIZZLE_RED, gli::SWIZZLE_GREEN, gli::SWIZZLE_BLUE, gli::SWIZZLE_ALPHA));

	glInternal = GLFormat.Internal;
	glExternal = GLFormat.External;
	glType = GLFormat.Type;
	isCompressed = gli::is_compressed(f);
	isSrgb = gli::is_srgb(f);
}

std::unique_ptr<ImageResource> gli_load(const char* filename)
{
	gli::texture tex = gli::load(filename);
	if (tex.empty())
		throw std::exception("error opening file");

	std::unique_ptr<ImageResource> res = std::make_unique<ImageResource>();
	
	// determine image format
	getImageFormat(res->format, tex);

	res->layer.assign(tex.layers(), ImageLayer());
	for(size_t layer = 0; layer < tex.layers(); ++layer)
	{
		res->layer.at(layer).faces.assign(tex.faces(), ImageFace());
		for(size_t face = 0; face < tex.faces(); ++face)
		{
			// load face
			res->layer.at(layer).faces.at(face).mipmaps.assign(tex.levels(), ImageMipmap());
			for(size_t mip = 0; mip < tex.levels(); ++mip)
			{
				// fill mipmap
				ImageMipmap& mipmap = res->layer.at(layer).faces.at(face).mipmaps.at(mip);
				mipmap.width = tex.extent(mip).x;
				mipmap.height = tex.extent(mip).y;
				mipmap.depth = tex.extent(mip).z;

				auto data = tex.data(layer, face, mip);
				auto size = tex.size(mip);
				mipmap.bytes.reserve(size);
				mipmap.bytes.assign(reinterpret_cast<char*>(data), reinterpret_cast<char*>(data) + size);
			}
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
	if(layer == 6)
	{
		// cube map
		s_useCubemap = true;
		s_textureCube = gli::texture_cube(gli::format(format), gli::extent2d(width, height), levels);
		if (s_textureCube.empty())
			throw std::runtime_error("could not create texture");
	}
	else
	{
		s_useCubemap = false;
		s_textureArray = gli::texture2d_array(gli::format(format), gli::extent2d(width, height), layer, levels);
		if (s_textureArray.empty())
			throw std::runtime_error("could not create texture");
	}
}

void gli_store_level(int layer, int level, const void* data, uint64_t size)
{
	if(s_useCubemap)
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
	if(s_useCubemap)
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
	if(s_useCubemap)
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
