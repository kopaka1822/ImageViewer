#pragma once
#include <vector>
#include <assert.h>
#include <array>

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

	// performs a change of stride inplace
	// => byte array with 4 byte stride (RGBA) can be changed to 2 byte stride (RA) when using bitmask = 0b1001
	inline void changeStrideEx(uint8_t* bytes, size_t byteSize, size_t oldStride, size_t bitmask)
	{
		const auto numElements = byteSize / oldStride;
		size_t newStride = 0;

		assert(oldStride <= 16);
		std::array<uint8_t, 16> offsets;

		// new stride is the number of set bits
		for(size_t i = 0; i < oldStride; ++i)
		{
			size_t mask = 1ull << i;
			if (mask & bitmask)
				offsets[newStride++] = uint8_t(i);
		}

		for(size_t src = 0, dst = 0; src < byteSize; src += oldStride, dst += newStride)
		{
			for(size_t i = 0; i < newStride; ++i)
			{
				bytes[dst + i] = bytes[src + offsets[i]];
			}
		}
	}

	// check whether machine is little endian
	inline int littleendian()
	{
		int intval = 1;
		unsigned char* uval = reinterpret_cast<unsigned char*>(&intval);
		return uval[0] == 1;
	}
}