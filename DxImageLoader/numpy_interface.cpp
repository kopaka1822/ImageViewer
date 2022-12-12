#include "pch.h"
#include "numpy_interface.h"
#include <functional>
#include <sstream>


#include "npy.h"
#include "convert.h"
using namespace npy;

class NumpyImage final : public image::IImage
{
public:
	NumpyImage(const char* filename)
	{
		// load numpy file
		std::vector<unsigned long> shape;
		m_data = LoadArrayFromNumpyForceFloat(filename, shape, m_originalFormat);
		if (shape.empty())
			throw std::exception("array shape is empty");

		// split shape information into width, height, layers and components (image format)
		m_height = shape[0];
		if (shape.size() > 1)
			m_width = shape[1];
		auto nComponents = 1;
		if (shape.size() > 2)
		{
			// try to fit the remaining dimensions into the color format
			if(shape[2] <= 4 && shape.size() == 3)
			{
				nComponents = shape[2];
			}
			else
			{
				m_depth = calcRemainingDimensions(shape, 2);
			}
		}
		
		// pad format to fit 4 components
		switch (nComponents)
		{
		case 1: // pad with 3 additional floats
			m_data.resize(m_data.size() * 4);
			image::expandRtoRGBA(m_data.data(), m_width * m_height * m_depth, 0.0f, 1.0f);
			break;
		case 2:
			m_data.resize(m_data.size() * 2);
			image::expandRGtoRGBA(m_data.data(), m_width * m_height * m_depth, 0.0f, 1.0f);
			break;
		case 3:
			m_data.resize(m_width * m_height * m_depth * 4);
			image::expandRGBtoRGBA(m_data.data(), m_width * m_height * m_depth, 1.0f);
			break;
		case 4: break; // everything is ok
		default:
			assert(false);
		}
	}

	uint32_t getNumLayers() const override { return 1; }
	uint32_t getNumMipmaps() const override { return 1; }
	uint32_t getWidth(uint32_t mipmap) const override { return m_width; }
	uint32_t getHeight(uint32_t mipmap) const override { return m_height; }
	uint32_t getDepth(uint32_t mipmap) const override { return m_depth; }
	gli::format getFormat() const override { return gli::FORMAT_RGBA32_SFLOAT_PACK32; }
	gli::format getOriginalFormat() const override { return m_originalFormat; }
	uint8_t* getData(uint32_t layer, uint32_t mipmap, size_t& size) override
	{
		assert(layer == 0);
		assert(mipmap == 0);
		size = m_data.size() * sizeof(float);
		return reinterpret_cast<uint8_t*>(m_data.data());
	}
	const uint8_t* getData(uint32_t layer, uint32_t mipmap, size_t& size) const override
	{
		return const_cast<NumpyImage*>(this)->getData(layer, mipmap, size);
	}

private:
    static std::vector <float> LoadArrayFromNumpyForceFloat(const char* filename, std::vector<unsigned long>& shape, gli::format& originalFormat)
    {
    	std::ifstream stream(filename, std::ifstream::binary);
        if (!stream) {
            throw std::runtime_error("io error: failed to open a file.");
        }

        std::string header_s = read_header(stream);

        // parse header
        header_t header = parse_header(header_s);

		std::vector<float> data; // output
		std::vector<char> tmpData; // tmp buffer


        shape = header.shape;
        //fortran_order = header.fortran_order;

        // compute the data size based on the shape
        auto size = static_cast<size_t>(comp_size(shape));
		tmpData.resize(size * header.dtype.itemsize);
		data.resize(size);

        // read the data raw
        stream.read(tmpData.data(), header.dtype.itemsize * size);

		// convert to float format
		switch (header.dtype.kind)
		{
		case 'f': // float kind
			switch (header.dtype.itemsize)
			{
			case sizeof(float):
				std::copy_n(reinterpret_cast<float*>(tmpData.data()), size, data.begin());
				originalFormat = gli::format::FORMAT_R32_SFLOAT_PACK32;
				break;
			case sizeof(double):
			// same as double
			//case sizeof(long double):
				std::copy_n(reinterpret_cast<double*>(tmpData.data()), size, data.begin());
				originalFormat = gli::format::FORMAT_R64_SFLOAT_PACK64;
				break;
			default:
				throw std::runtime_error("unsupported itemsize for float kind");
			}
			break;
		case 'i': // integer kind
			switch (header.dtype.itemsize)
			{
			case sizeof(char):
				std::copy_n(reinterpret_cast<char*>(tmpData.data()), size, data.begin());
				originalFormat = gli::format::FORMAT_R8_SINT_PACK8;
				break;
			case sizeof(short):
				std::copy_n(reinterpret_cast<short*>(tmpData.data()), size, data.begin());
				originalFormat = gli::format::FORMAT_R16_SINT_PACK16;
				break;
			case sizeof(int):
				std::copy_n(reinterpret_cast<int*>(tmpData.data()), size, data.begin());
				originalFormat = gli::format::FORMAT_R32_SINT_PACK32;
				break;
			case sizeof(long long):
				std::copy_n(reinterpret_cast<long long*>(tmpData.data()), size, data.begin());
				originalFormat = gli::format::FORMAT_R64_SINT_PACK64;
				break;
			default:
				throw std::runtime_error("unsupported itemsize for integer kind");
			}
			break;
		case 'u': // unsigned integer kind
			switch (header.dtype.itemsize)
			{
			case sizeof(unsigned char) :
				std::copy_n(reinterpret_cast<unsigned char*>(tmpData.data()), size, data.begin());
				originalFormat = gli::format::FORMAT_R8_UINT_PACK8;
				break;
			case sizeof(unsigned short) :
				std::copy_n(reinterpret_cast<unsigned short*>(tmpData.data()), size, data.begin());
				originalFormat = gli::format::FORMAT_R16_UINT_PACK16;
				break;
			case sizeof(unsigned int) :
				std::copy_n(reinterpret_cast<unsigned int*>(tmpData.data()), size, data.begin());
				originalFormat = gli::format::FORMAT_R32_UINT_PACK32;
				break;
			case sizeof(unsigned long long) :
				std::copy_n(reinterpret_cast<unsigned long long*>(tmpData.data()), size, data.begin());
				originalFormat = gli::format::FORMAT_R64_UINT_PACK64;
				break;
			default:
				throw std::runtime_error("unsupported itemsize for integer kind");
			}
			break;
		//case 'c': // complex kind (float2)
		//	throw std::runtime_error("unsupported complex kind");
		default:
			std::stringstream ss;
			ss << "unsupported kind: " << header.dtype.kind;
			throw std::runtime_error(ss.str());
		}

		// fix endianess ?
		if(header.dtype.byteorder != host_endian_char && header.dtype.byteorder != no_endian_char)
		{
			auto swapBytes = [](float& fptr)
			{
				char* ptr = reinterpret_cast<char*>(&fptr);
				std::swap(ptr[0], ptr[3]);
				std::swap(ptr[1], ptr[2]);
			};
			std::for_each(data.begin(), data.end(), swapBytes);
		}

		return data;
    }
	static gli::format getFloatFormat(int nComponents)
    {
	    switch (nComponents)
	    {
		case 1: return gli::format::FORMAT_R32_SFLOAT_PACK32;
		case 2: return gli::format::FORMAT_RG32_SFLOAT_PACK32;
		case 3: return gli::format::FORMAT_RGB32_SFLOAT_PACK32;
		case 4: return gli::format::FORMAT_RGBA32_SFLOAT_PACK32;
	    }
		assert(false);
		return gli::format::FORMAT_UNDEFINED;
    }
	static uint32_t calcRemainingDimensions(const std::vector<unsigned long>& shape, size_t startIndex)
    {
		uint32_t size = 1;
	    for(size_t i = startIndex; i < shape.size(); ++i)
			size *= shape[i];
		
		return size;
    }

	std::vector<float> m_data;
	gli::format m_originalFormat = gli::format::FORMAT_UNDEFINED;
	uint32_t m_width = 1;
	uint32_t m_height = 0;
	uint32_t m_depth = 1;
};

std::unique_ptr<image::IImage> numpy_load(const char* filename)
{
	return std::make_unique<NumpyImage>(filename);
}
