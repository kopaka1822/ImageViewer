#include "pch.h"
#include "GliImage.h"
#include "compress_interface.h"
#include "interface.h"

// mofified copy of gli convert
template <typename texture_type>
inline texture_type convert_mod(texture_type const& Texture, gli::format Format)
{
	typedef float T;
	typedef typename gli::texture::extent_type extent_type;
	typedef typename texture_type::size_type size_type;
	typedef typename extent_type::value_type component_type;
	typedef typename gli::detail::convert<texture_type, T, gli::defaultp>::fetchFunc fetch_type;
	typedef typename gli::detail::convert<texture_type, T, gli::defaultp>::writeFunc write_type;

	GLI_ASSERT(!Texture.empty());
	GLI_ASSERT(!is_compressed(Format));

	const auto isSrgb = gli::is_srgb(Texture.format());
	const auto isCompressed = gli::is_compressed(Texture.format());
	assert(!isCompressed); // this should be done by compressonator

	// for some reason the gli loader doesn't revert srgb compression in this case
	//const auto convertFromSrgb = isSrgb && isCompressed;

	fetch_type Fetch = gli::detail::convert<texture_type, T, gli::defaultp>::call(Texture.format()).Fetch;
	write_type Write = gli::detail::convert<texture_type, T, gli::defaultp>::call(Format).Write;

	gli::texture Storage(Texture.target(), Format, Texture.texture::extent(), Texture.layers(), Texture.faces(), Texture.levels(), Texture.swizzles());
	texture_type Copy(Storage);

	extent_type const& baseDim = Texture.texture::extent(0);
	// divide by 100 for range [0, 100]
	const size_t numSteps = image::IImage::calcNumPixels(uint32_t(Texture.layers() * Texture.faces()), uint32_t(Texture.levels()), baseDim.x, baseDim.y, baseDim.z) / 100;
	size_t curSteps = 0;

	for (size_type Layer = 0; Layer < Texture.layers(); ++Layer)
		for (size_type Face = 0; Face < Texture.faces(); ++Face)
			for (size_type Level = 0; Level < Texture.levels(); ++Level)
			{
				extent_type const& Dimensions = Texture.texture::extent(Level);

				for (component_type k = 0; k < Dimensions.z; ++k)
					for (component_type j = 0; j < Dimensions.y; ++j)
					{
						set_progress(uint32_t(curSteps / numSteps));

						for (component_type i = 0; i < Dimensions.x; ++i)
						{
							typename texture_type::extent_type const Texelcoord(extent_type(i, j, k));
							auto texel = Fetch(Texture, Texelcoord, Layer, Face, Level);
							//if (convertFromSrgb)
							//	texel = gli::convertSRGBToLinear(texel);

							Write(
								Copy, Texelcoord, Layer, Face, Level,
								texel);
						}
						curSteps += Dimensions.x;
					}
						
			}

	return texture_type(Copy);
}

GliImage::GliImage(const gli::texture& tex)
	: GliImage(tex, tex.format())
{}

std::unique_ptr<GliImage> GliImage::convert(gli::format format, int quality)
{
	if(is_compressonator_format(format) || is_compressonator_format(m_base.format())) // convert to compressed format
	{
		// compressed format, use compressonator to compress
		auto dst = std::make_unique<GliImage>(format, m_original, m_base.layers(), m_base.faces(), m_base.levels(), m_base.extent().x, m_base.extent().y, m_base.extent().z);
		compressonator_convert_image(*this, *dst, quality);
		return dst;
	}
	else // uncompressed format => use gli convert method
	{
		if (m_type == Cubes) return std::make_unique<GliImage>(convert_mod(m_cube, format), m_original);
		if (m_type == Volume) return std::make_unique<GliImage>(convert_mod(m_volume, format), m_original);
		return std::make_unique<GliImage>(convert_mod(m_array, format), m_original);
	}
}

void GliImage::saveKtx(const char* filename) const
{
	if (m_type == Cubes) gli::save_ktx(m_cube, filename);
	else if (m_type == Volume) gli::save_ktx(m_volume, filename);
	else gli::save_ktx(m_array, filename);
}

void GliImage::saveDds(const char* filename) const
{
	if (m_type == Cubes) gli::save_dds(m_cube, filename);
	else if (m_type == Volume) gli::save_dds(m_volume, filename);
	else gli::save_dds(m_array, filename);
}

void GliImage::flip()
{
	if (m_type == Cubes) m_cube = gli::flip(m_cube);
	//else if (m_type == Volume) m_volume = gli::flip(m_volume);
	else if(m_type == Planes) m_array = gli::flip(m_array);
}

GliImage::GliImage(gli::format format, gli::format original, size_t nLayer, size_t nFaces, size_t nLevel, size_t width,
	size_t height, size_t depth) :
GliImageBase(initTex(nFaces, depth), original)
{
	if (m_type == Cubes) m_cube = gli::texture_cube_array(format, gli::extent2d{ width, height }, nLayer, nLevel);
	else if (m_type == Volume)
	{
		assert(nLayer == 1);
		m_volume = gli::texture3d(format, gli::extent3d{ width, height, depth }, nLevel);
	}
	else m_array = gli::texture2d_array(format, gli::extent2d{ width, height }, nLayer, nLevel);
}

GliImage::GliImage(gli::format format, size_t nLayer, size_t nLevel, size_t width, size_t height, size_t depth)
	:
// create cube map array if nLayer == 6, otherwise 2d array
GliImage(format, format, nLayer == 6? 1 : nLayer, nLayer == 6 ? 6 : 1, nLevel, width, height, depth)
{}

GliImage::GliImage(const gli::texture& tex, gli::format original) :
	GliImageBase(initTex(tex.faces(), tex.extent().z), original)
{
	if (tex.empty())
		throw std::runtime_error("could not load image");

	if (m_type == Cubes) m_cube = gli::texture_cube_array(tex);
	else if (m_type == Volume) m_volume = gli::texture3d(tex);
	else m_array = gli::texture2d_array(tex);
} 

gli::texture& GliImage::initTex(size_t nFaces, size_t depth)
{
	if(depth > 1)
	{
		m_type = Volume;
		return m_volume;
	}

	if(nFaces == 6)
	{
		m_type = Cubes;
		return m_cube;
	}
	m_type = Planes;
	return m_array;
}
