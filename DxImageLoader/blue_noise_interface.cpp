#include "pch.h"

#include <algorithm>
#include <stdexcept>
#include <string>

#include "interface.h"
#include "../dependencies/blue_noise/blue_noise_generator.h"
#include "../dependencies/blue_noise/blue_noise_generator_parameters.h"

#include "../dependencies/blue_noise/blue_noise_generator.cpp"
#include "../dependencies/blue_noise/blue_noise_generator_parameters.cpp"
// SIGGRAPH 2016 paper "Blue-noise Dithered Sampling" by Iliyan Georgiev and Marcos Fajardo from Solid Angle

class CBlueNoiseGenProgress : public IBlueNoiseGenProgressMonitor
{
public:
	CBlueNoiseGenProgress(size_t totalNumIter) : numIterationsToFindDistribution(totalNumIter)
	{}
private:
	size_t numIterationsToFindDistribution;

private:
	virtual void OnProgress(size_t iterCount, double bestScore, size_t swapCount, size_t swapAttempt) override
	{
		uint32_t newPercent = uint32_t(double(iterCount) / (double(numIterationsToFindDistribution) / 100.0));
		set_progress(newPercent);
	}
	virtual void OnStartWhiteNoiseGeneration() override {}
	virtual void OnStartBlueNoiseGeneration() override {}
	virtual void OnSliceGenerated(size_t sliceIndex, size_t totalSliceCount) override {}
	virtual void OnSliceRefined(size_t sliceIndex, size_t totalSliceCount) override {}
};


class BlueNoiseImage final : public image::IImage
{
public:
	BlueNoiseImage(int width, int height, int depth, int layer, int mipmaps)
		: m_width(width)
		, m_height(height)
		, m_depth(depth)
		, m_layer(layer)
		, m_mipmaps(mipmaps)
		, m_size(size_t(width)* size_t(height)* size_t(depth) * 4)
	{
		BlueNoiseGeneratorParameters params;
		BlueNoiseGenerator generator;

		// 2D Texture example
		params.chosenMethod = BlueNoiseGeneratorParameters::Method_SolidAngle;
		params.N_dimensions = depth > 1 ? 3u : 2u;
		params.dimensionSize[0] = width;
		params.dimensionSize[1] = height;
		params.dimensionSize[2] = depth > 1 ? depth : 0u;
		params.dimensionSize[3] = 0;
		params.N_valuesPerItem = 1u;
		params.useMultithreading = false;
		params.useIncrementalUpdate = true; // required for multithreading
		params.numIterationsToFindDistribution = 4 * 256 * 1024;

		std::vector<float> whiteNoise;
		std::vector<float> blueNoise;
		CBlueNoiseGenProgress progress(params.numIterationsToFindDistribution);
		BlueNoiseGenerator::EResult result = generator.GenerateBlueNoise(params, whiteNoise, blueNoise, &progress);
		switch (result)
		{
		case BlueNoiseGenerator::Result_DimensionSmallerThanKernelSize:
			throw std::runtime_error(std::string("Texture dimension must be greater or equal than ") + std::to_string(generator.GetMinTextureSize()) + " texels.");
		}

		// convert float to uint32_t
		{
			std::vector<uint32_t> data;
			data.resize(blueNoise.size());
			std::transform(blueNoise.begin(), blueNoise.end(), data.begin(), [](float f)
				{
					return glm::packUnorm4x8(glm::vec4(f, f, f, 1.0));
				});
			m_values.push_back(move(data));
		}

		// TODO mipmaps, for now leave empty with zeros
		for (int i = 1; i < mipmaps; ++i)
		{
			std::vector<uint32_t> data;
			data.resize(size_t(getWidth(i)) * size_t(getHeight(i)) * size_t(getDepth(i)));
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
		return const_cast<BlueNoiseImage*>(this)->getData(layer, mipmap, size);
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

std::unique_ptr<image::IImage> noise_get_blue_noise(int width, int height, int depth, int layer, int mipmaps)
{
	return std::make_unique<BlueNoiseImage>(width, height, depth, layer, mipmaps);
}