#pragma once
#include <vector>
#include <assert.h>

namespace image
{
	/// preforms a change of stride inplace
	/// => byte array with 4 byte stride (RGBA) can be changed to a 2 byte stride array (RG)
	inline size_t changeStride(uint8_t* bytes, size_t byteSize, const size_t oldStride, const size_t newStride)
	{
		if (newStride == oldStride) return byteSize;

		assert(newStride < oldStride);
		assert(byteSize % oldStride == 0);
		const auto numElements = byteSize / oldStride;
		for(size_t src = oldStride, dst = newStride; src < byteSize; src += oldStride, dst += newStride)
		{
			for (size_t i = 0; i < newStride; ++i)
				bytes[dst + i] = bytes[src + i];
		}

		return newStride * numElements;
	}
}