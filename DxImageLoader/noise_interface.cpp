#include "pch.h"
#include "noise_interface.h"
#include <random>

class NoiseImage final : public image::IImage
{
public:
	NoiseImage(int width, int height, int depth, int layer, int mipmaps, int seed)
		: m_width(width)
		, m_height(height)
		, m_depth(depth)
		, m_layer(layer)
		, m_mipmaps(mipmaps)
		, m_size(size_t(width)* size_t(height)* size_t(depth) * 4)
	{
		std::mt19937 gen(seed);
		std::uniform_real_distribution<float> dis(0.0, 1.0);

		for(int i = 0; i < mipmaps; ++i)
		{
			std::vector<uint32_t> data;
			data.resize(size_t(getWidth(i)) * size_t(getHeight(i)) * size_t(getDepth(i)));
			for(auto& v : data)
				v = glm::packUnorm4x8(glm::vec4(dis(gen), dis(gen), dis(gen), 1.0f));

			m_values.push_back(move(data));
		}
	}

	uint32_t getNumLayers() const override { return m_layer; }
	uint32_t getNumMipmaps() const override { return m_mipmaps; }
	uint32_t getWidth(uint32_t mipmap) const override { return std::max(1u, uint32_t(m_width) >> mipmap); }
	uint32_t getHeight(uint32_t mipmap) const override { return std::max(1u, uint32_t(m_height) >> mipmap); }
	uint32_t getDepth(uint32_t mipmap) const override { return std::max(1u, uint32_t(m_depth) >> mipmap); }
	gli::format getFormat() const override { return gli::format::FORMAT_RGBA8_UNORM_PACK8; }
	gli::format getOriginalFormat() const override { return gli::format::FORMAT_RGBA8_UNORM_PACK8; }
	uint8_t* getData(uint32_t layer, uint32_t mipmap, size_t& size) override
	{
		size = m_values.at(mipmap).size() * sizeof(m_values[0][0]);
		return reinterpret_cast<uint8_t*>(m_values.at(mipmap).data());
	}
	const uint8_t* getData(uint32_t layer, uint32_t mipmap, size_t& size) const override
	{
		return const_cast<NoiseImage*>(this)->getData(layer, mipmap, size);
	}

private:
	int m_width = 0;
	int m_height = 0;
	int m_depth = 0;
	int m_layer = 0;
	int m_mipmaps = 0;
	uint32_t m_size = 0;

	std::vector<std::vector<uint32_t>> m_values;
};

std::unique_ptr<image::IImage> noise_get_white_noise(int width, int height, int depth, int layer, int mipmaps, int seed)
{
	return std::make_unique<NoiseImage>(width, height, depth, layer, mipmaps, seed);
}