#pragma once
#include "Layer.h"
#include "framework.h"
#include <cassert>

namespace image
{
	// image interface
	class IImage
	{
	public:
		virtual ~IImage() = default;
		virtual uint32_t getNumLayers() const = 0;
		virtual uint32_t getNumMipmaps() const = 0;
		virtual uint32_t getWidth(uint32_t mipmap) const = 0;
		virtual uint32_t getHeight(uint32_t mipmap) const = 0;
		virtual uint32_t getDepth(uint32_t mipmap) const = 0;
		
		// must fulfill isSupported(format)
		virtual gli::format getFormat() const = 0;
		// the original format of the file
		virtual gli::format getOriginalFormat() const = 0;
		virtual uint8_t* getData(uint32_t layer, uint32_t mipmap, size_t& size) = 0;
		virtual const uint8_t* getData(uint32_t layer, uint32_t mipmap, size_t& size) const = 0;

		// progress helper
		static size_t calcNumPixels(uint32_t numLayer, uint32_t numLevels, uint32_t width, uint32_t height, uint32_t depth);
		size_t getNumPixels() const;

		// additional postprocessing
		
		// if true indicates that the value of the red channel should be written to green and blue as well
		virtual bool requiresGrayscalePostprocess() { return false; }
	};

	// default interface that supplies internal storage for a single layer/mipmap
	class SimpleImage final : public IImage
	{
	public:
		SimpleImage(gli::format originalFormat, gli::format internalFormat, uint32_t width, uint32_t height, uint32_t pixelByteSize);

		uint32_t getNumLayers() const override { return 1; }
		uint32_t getNumMipmaps() const override { return 1; }
		uint32_t getWidth(uint32_t mipmap) const override { return m_width; }
		uint32_t getHeight(uint32_t mipmap) const override { return m_height; }
		uint32_t getDepth(uint32_t mipmap) const override { return 1; }
		gli::format getFormat() const override { return m_format; }
		gli::format getOriginalFormat() const override { return m_original; }
		uint8_t* getData(uint32_t layer, uint32_t mipmap, size_t& size) override
		{
			size = m_data.size();
			return m_data.data();
		}
		const uint8_t* getData(uint32_t layer, uint32_t mipmap, size_t& size) const override
		{
			size = m_data.size();
			return m_data.data();
		}

	private:
		uint32_t m_width;
		uint32_t m_height;
		std::vector<uint8_t> m_data;
		gli::format m_original;
		gli::format m_format;
	};

	inline bool isSupported(gli::format format)
	{
		switch (format)
		{
		case gli::format::FORMAT_RGBA8_SRGB_PACK8:
		case gli::format::FORMAT_RGBA8_UNORM_PACK8:
		case gli::format::FORMAT_RGBA8_SNORM_PACK8:
		case gli::format::FORMAT_RGBA32_SFLOAT_PACK32:
			return true;
		}
		return false;
	}

	uint32_t pixelSize(gli::format format);

	gli::format getSupportedFormat(gli::format format);
}
