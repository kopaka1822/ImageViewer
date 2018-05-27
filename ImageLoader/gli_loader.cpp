#include "gli_loader.h"
#include <gli/gli.hpp>
#include <gli/gl.hpp>


void getImageFormat(ImageFormat& format, const gli::texture& tex)
{
	gli::gl GL(gli::gl::PROFILE_GL33);
	auto GLformat = GL.translate(tex.format(), tex.swizzles());
	// TODO check if format is srgb
	format.openglInternalFormat = static_cast<uint32_t>(GLformat.Internal);
	format.openglExternalFormat = static_cast<uint32_t>(GLformat.External);
	format.openglType = static_cast<uint32_t>(GLformat.Type);
	format.isCompressed = gli::is_compressed(tex.format());
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
				mipmap.bytes.assign(reinterpret_cast<char*>(data), reinterpret_cast<char*>(data) + size);
			}
		}
	}

	return res;
}