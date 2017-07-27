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
	format.isCompressed = gli::is_compressed(tex.format());
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

	auto depth = tex.extent(0).z;

	if (depth == 1)
	{
		res->layer.assign(tex.faces() * tex.layers(), ImageLayer());

		for (size_t layer = 0; layer < tex.layers(); ++layer)
		{
			for (size_t face = 0; face < tex.faces(); ++face)
			{
				// load layer (faces)
				res->layer[layer * tex.faces() + face].mipmaps.assign(tex.levels(), ImageMipmap());
				for (size_t mip = 0; mip < tex.levels(); ++mip)
				{
					// fill mipmap
					res->layer[layer * tex.faces() + face].mipmaps[mip].width = tex.extent(mip).x;
					res->layer[layer * tex.faces() + face].mipmaps[mip].height = tex.extent(mip).y;
					auto data = tex.data(layer, face, mip);
					auto size = tex.size(mip);
					res->layer[layer * tex.faces() + face].mipmaps[mip].bytes.assign(reinterpret_cast<char*>(data), reinterpret_cast<char*>(data) + size);
				}
			}
		}
	}
	else if(tex.levels() == 1) // no mipmaps
	{
		res->layer.assign(tex.faces() * tex.layers() * depth, ImageLayer());
		// load 3d texture as layered texture (experimental)
		for (size_t layer = 0; layer < tex.layers(); ++layer)
		{
			for (size_t face = 0; face < tex.faces(); ++face)
			{
				auto data = tex.data(layer, face, 0);
				auto size = tex.size(0);
				auto step = size / depth;
				for (auto curDepth = 0; curDepth < depth; ++curDepth)
				{
					auto idx = (layer * tex.faces() + face) * depth + curDepth;
					res->layer[idx].mipmaps.assign(1, ImageMipmap());
					// fill mipmap
					res->layer[idx].mipmaps[0].width = tex.extent(0).x;
					res->layer[idx].mipmaps[0].height = tex.extent(0).y;

					res->layer[idx].mipmaps[0].bytes.assign(reinterpret_cast<char*>(data), reinterpret_cast<char*>(data) + step);
					data = reinterpret_cast<void*>(reinterpret_cast<char*>(data) + step);
				}
			}
		}
	}
	else return nullptr;

	// get data
	return res;
}
