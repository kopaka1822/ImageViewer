#pragma once
#include "Layer.h"
#include "framework.h"
#include <cassert>

namespace image
{
	struct Image
	{
		std::vector<Layer> layer;
		// must fulfill isSupported(format)
		gli::format format;
		// the original format of the file
		gli::format original;
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
