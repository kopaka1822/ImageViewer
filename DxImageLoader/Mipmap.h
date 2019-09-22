#pragma once
#include <vector>

namespace image
{
	struct Mipmap
	{
		std::vector<uint8_t> bytes;
		uint32_t width;
		uint32_t height;
		uint32_t depth = 1;
	};
}