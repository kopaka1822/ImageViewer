#include "gli_loader.h"
#include <gli/gli.hpp>
#include <gli/gl.hpp>


bool getImageFormat(ImageFormat& format, const gli::texture& tex)
{
	gli::gl GL(gli::gl::PROFILE_GL33);
	auto GLformat = GL.translate(tex.format(), tex.swizzles());
	format.openglInternalFormat = static_cast<uint32_t>(GLformat.Internal);
	format.openglExternalFormat = static_cast<uint32_t>(GLformat.External);
	format.openglType = static_cast<uint32_t>(GLformat.Type);
	return true;
}

std::unique_ptr<ImageResource> gli_load(const char* filename)
{
	gli::texture tex = gli::load(filename);
	if (tex.empty())
		return nullptr;

	std::unique_ptr<ImageResource> res = std::make_unique<ImageResource>();
	
	// determine image format
	if (!getImageFormat(res->format, tex))
		return nullptr;

	// add layer
	res->layer.assign(tex.faces(), ImageLayer());
	for(size_t level = 0; level < tex.faces(); ++level)
	{
		// load layer (faces)
		res->layer[level].mipmaps.assign(tex.levels(), ImageMipmap());
		for(size_t mip = 0; mip < tex.levels(); ++mip)
		{
			// fill mipmap
			res->layer[level].mipmaps[mip].width = tex.extent(mip).x;
			res->layer[level].mipmaps[mip].height = tex.extent(mip).y;
			auto data = tex.data(0, level, mip);
			auto size = tex.size(mip);
			res->layer[level].mipmaps[mip].bytes.assign(reinterpret_cast<char*>(data), reinterpret_cast<char*>(data) + size);
		}
	}

	// get data
	return res;
}
