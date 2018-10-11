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
static gli::texture2d_array s_texture;

void gli_create_storage(int format, int width, int height, int layer, int levels)
{
	s_texture = gli::texture2d_array(gli::format(format), gli::extent2d(width, height), layer, levels);
	if (s_texture.empty())
		throw std::runtime_error("could not create texture");
}

void gli_store_level(int layer, int level, const void* data, uint64_t size)
{
	if (s_texture.size() != size)
		throw std::runtime_error("data size mismatch. Expected "
			+ std::to_string(s_texture.size()) + " but got " + std::to_string(size));
	memcpy(s_texture.data(layer, 0, level), data, s_texture.size(level));
}

void gli_save_ktx(const char* filename)
{
	if (!gli::save_ktx(s_texture, filename))
	{
		// clear texture
		s_texture = gli::texture2d_array();
		throw std::exception("could not save file");
	}
	// clear texture
	s_texture = gli::texture2d_array();
}