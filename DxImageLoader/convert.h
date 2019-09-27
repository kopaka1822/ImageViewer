#pragma once
#include <vector>
#include <assert.h>

namespace image
{
	/// preforms a change of stride inplace
	/// => byte array with 4 byte stride (RGBA) can be changed to a 2 byte stride array (RG)
	inline void changeStride(std::vector<uint8_t>& bytes, const size_t oldStride, const size_t newStride)
	{
		if (newStride == oldStride) return;

		assert(newStride < oldStride);
		const auto numElements = bytes.size() / oldStride;
		for(size_t src = oldStride, dst = newStride; src < bytes.size(); src += oldStride, dst += newStride)
		{
			for (size_t i = 0; i < newStride; ++i)
				bytes[dst + i] = bytes[src + i];
		}

		bytes.resize(newStride * numElements);
	}
}