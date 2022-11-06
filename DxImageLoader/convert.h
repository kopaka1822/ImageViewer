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

	template<size_t channelSize>
	inline void copyRedToGreenBlue(uint8_t* bytes, size_t size)
	{
		for(auto end = bytes + size; bytes < end; bytes += 4 * channelSize)
		{
			size_t off = channelSize;
			for(size_t i = 0; i < 2; ++i) // loop for green and blue
			{
				for(size_t j = 0 ; j < channelSize; ++j) // loop through channel size
				{
					bytes[off++] = bytes[j];
				}
			}
		}
	}

	template<class T>
	inline void expandRGBtoRGBA(T* data, size_t numPixels, T alphaValue)
	{
		T* curEnd = data + numPixels * 3;
		T* actualEnd = data + numPixels * 4;
		// move to last pixel
		curEnd -= 3;
		actualEnd -= 4;

		while(curEnd >= data)
		{
			auto r = curEnd[0];
			auto g = curEnd[1];
			auto b = curEnd[2];
			actualEnd[0] = r;
			actualEnd[1] = g;
			actualEnd[2] = b;
			actualEnd[3] = alphaValue;

			// move one pixel left
			curEnd -= 3;
			actualEnd -= 4;
		}
		assert(curEnd + 3 == data);
		assert(actualEnd + 4 == data);
	}

	template<class T>
	inline void expandBGRtoRGBA(T* data, size_t numPixels, T alphaValue)
	{
		T* curEnd = data + numPixels * 3;
		T* actualEnd = data + numPixels * 4;
		// move to last pixel
		curEnd -= 3;
		actualEnd -= 4;

		while (curEnd >= data)
		{
			auto b = curEnd[0];
			auto g = curEnd[1];
			auto r = curEnd[2];
			actualEnd[0] = r;
			actualEnd[1] = g;
			actualEnd[2] = b;
			actualEnd[3] = alphaValue;

			// move one pixel left
			curEnd -= 3;
			actualEnd -= 4;
		}

		assert(curEnd + 3 == data);
		assert(actualEnd + 4 == data);
	}
}