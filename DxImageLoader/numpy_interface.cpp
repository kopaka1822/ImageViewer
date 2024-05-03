#include "pch.h"
#include "numpy_interface.h"
#include <functional>
#include <sstream>


#include "npy.h"
#include "convert.h"
#include "interface.h"
using namespace npy;

unsigned* npy_get_shape(const char* filename, unsigned int* dim)
{
	std::ifstream stream(filename, std::ifstream::binary);
	if (!stream)
	{
		set_error("could not open file");
		return nullptr;
	}

	std::string header_s = read_header(stream);
	// parse header
	header_t header = parse_header(header_s);
	
	static std::vector<unsigned int> s_shape;
	s_shape.assign(header.shape.begin(), header.shape.end());
	
	if (std::any_of(header.shape.begin(), header.shape.end(), [](ndarray_len_t i){
		return i > static_cast<ndarray_len_t>(std::numeric_limits<unsigned int>::max());
		}
	))
	{
		set_error("one of the shape dimensions is larger than 32bit (too large for import)");
		return nullptr;
	}

	if(dim) *dim = unsigned(s_shape.size());

	return s_shape.data();
}

static bool NumpyIs3D()
{
	return get_global_parameter_i("npy is3D", 0) != 0;
}

static bool NumpyUseChannels()
{
	return get_global_parameter_i("npy useChannel", 1) != 0;
}

static int NumpyFirstLayer()
{
	return get_global_parameter_i("npy firstLayer", 0);
}

static int NumpyLastLayer()
{
	return get_global_parameter_i("npy lastLayer", -1);
}

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

		auto nComponents = 1; // for now nComponents is always 1

		// last dimension is usually the channel size. Try to use it as channel size if it is small enough (and texture is at least 2D)
		if (NumpyUseChannels() && shape.back() <= 4)
		{
			nComponents = shape.back();
			shape.pop_back(); // remove from list
		}

		// reverse shape (width is always the last dimension, then height, depth...)
		std::reverse(shape.begin(), shape.end());
		
		// split shape information into width, height, layers and components (image format)
		if(!shape.empty())
			m_width = shape[0];
		if (shape.size() > 1)
			m_height = shape[1];
		if (shape.size() > 2)
			m_depth = calcRemainingDimensions(shape, 2);

		// pad format to fit 4 components
		switch (nComponents)
		{
		case 1: // pad with 3 additional floats
			m_data.resize(m_data.size() * 4);
			image::expandRtoRGBA(m_data.data(), m_width * m_height * m_depth, 1.0f);
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

		// determine if data needs to be cropped
		uint32_t firstLayer = NumpyFirstLayer();
		uint32_t lastLayer = NumpyLastLayer();
		if (lastLayer == unsigned(-1))
			lastLayer = m_depth - 1u;

		// crop data if required
		if(firstLayer != 0 || unsigned(lastLayer) != (m_depth - 1u))
		{
			size_t sliceSize = size_t(m_width) * size_t(m_height) * 4;

			std::vector<float> newData;
			newData.assign(m_data.data() + sliceSize * firstLayer, m_data.data() + sliceSize * (lastLayer + 1));
			m_data = std::move(newData);
			m_depth = lastLayer - firstLayer + 1;
		}
	}

	uint32_t getNumLayers() const override { return NumpyIs3D() ? 1 : m_depth; }
	uint32_t getNumMipmaps() const override { return 1; }
	uint32_t getWidth(uint32_t mipmap) const override { return m_width; }
	uint32_t getHeight(uint32_t mipmap) const override { return m_height; }
	uint32_t getDepth(uint32_t mipmap) const override { return NumpyIs3D() ? m_depth : 1; }
	gli::format getFormat() const override { return gli::FORMAT_RGBA32_SFLOAT_PACK32; }
	gli::format getOriginalFormat() const override { return m_originalFormat; }
	uint8_t* getData(uint32_t layer, uint32_t mipmap, size_t& size) override
	{
		if (NumpyIs3D())
		{
			assert(layer == 0);
			assert(mipmap == 0);
			size = m_data.size() * sizeof(float);
			return reinterpret_cast<uint8_t*>(m_data.data());
		}
		else
		{
			assert(mipmap == 0);
			size = m_data.size() * sizeof(float) / m_depth;
			return reinterpret_cast<uint8_t*>(m_data.data()) + size * layer;
		}
	}
	const uint8_t* getData(uint32_t layer, uint32_t mipmap, size_t& size) const override
	{
		return const_cast<NumpyImage*>(this)->getData(layer, mipmap, size);
	}

private:

	template<typename T>
	static void fixEndian(T* data, size_t size, char dataEndian, char hostEndian)
	{
		if (dataEndian == no_endian_char) return; // nothing do do
		if (dataEndian == hostEndian) return;
		if (sizeof(T) == 1) return;

		// reverse bit order
		for (const auto end = data + size; data != end; ++data)
		{
			char* byteStart = reinterpret_cast<char*>(data);
			std::reverse(byteStart, byteStart + sizeof(T));
		}

	}
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
				fixEndian(reinterpret_cast<float*>(tmpData.data()), size, header.dtype.byteorder, host_endian_char);
				std::copy_n(reinterpret_cast<float*>(tmpData.data()), size, data.begin());
				originalFormat = gli::format::FORMAT_R32_SFLOAT_PACK32;
				break;
			case sizeof(double):
			// same as double
			//case sizeof(long double):
				fixEndian(reinterpret_cast<double*>(tmpData.data()), size, header.dtype.byteorder, host_endian_char);
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
				fixEndian(reinterpret_cast<short*>(tmpData.data()), size, header.dtype.byteorder, host_endian_char);
				std::copy_n(reinterpret_cast<short*>(tmpData.data()), size, data.begin());
				originalFormat = gli::format::FORMAT_R16_SINT_PACK16;
				break;
			case sizeof(int):
				fixEndian(reinterpret_cast<int*>(tmpData.data()), size, header.dtype.byteorder, host_endian_char);
				std::copy_n(reinterpret_cast<int*>(tmpData.data()), size, data.begin());
				originalFormat = gli::format::FORMAT_R32_SINT_PACK32;
				break;
			case sizeof(long long):
				fixEndian(reinterpret_cast<long long*>(tmpData.data()), size, header.dtype.byteorder, host_endian_char);
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
			case sizeof(unsigned char):
				std::copy_n(reinterpret_cast<unsigned char*>(tmpData.data()), size, data.begin());
				originalFormat = gli::format::FORMAT_R8_UINT_PACK8;
				break;
			case sizeof(unsigned short):
				fixEndian(reinterpret_cast<unsigned short*>(tmpData.data()), size, header.dtype.byteorder, host_endian_char);
				std::copy_n(reinterpret_cast<unsigned short*>(tmpData.data()), size, data.begin());
				originalFormat = gli::format::FORMAT_R16_UINT_PACK16;
				break;
			case sizeof(unsigned int):
				fixEndian(reinterpret_cast<unsigned int*>(tmpData.data()), size, header.dtype.byteorder, host_endian_char);
				std::copy_n(reinterpret_cast<unsigned int*>(tmpData.data()), size, data.begin());
				originalFormat = gli::format::FORMAT_R32_UINT_PACK32;
				break;
			case sizeof(unsigned long long):
				fixEndian(reinterpret_cast<unsigned long long*>(tmpData.data()), size, header.dtype.byteorder, host_endian_char);
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
	uint32_t m_height = 1;
	uint32_t m_depth = 1;
};

std::unique_ptr<image::IImage> numpy_load(const char* filename)
{
	return std::make_unique<NumpyImage>(filename);
}

std::vector<uint32_t> numpy_get_export_formats()
{
	return std::vector<uint32_t>{
		// float formats
		gli::format::FORMAT_R32_SFLOAT_PACK32,
		gli::format::FORMAT_RG32_SFLOAT_PACK32,
		gli::format::FORMAT_RGB32_SFLOAT_PACK32,
		gli::format::FORMAT_RGBA32_SFLOAT_PACK32,

		// int formats
		// 32 bit signed
		gli::format::FORMAT_R32_SINT_PACK32,
		gli::format::FORMAT_RG32_SINT_PACK32,
		gli::format::FORMAT_RGB32_SINT_PACK32,
		gli::format::FORMAT_RGBA32_SINT_PACK32,
		// 32 bit unsigned
		gli::format::FORMAT_R32_UINT_PACK32,
		gli::format::FORMAT_RG32_UINT_PACK32,
		gli::format::FORMAT_RGB32_UINT_PACK32,
		gli::format::FORMAT_RGBA32_UINT_PACK32,

		// 16 bit signed
		gli::format::FORMAT_R16_SINT_PACK16,
		gli::format::FORMAT_RG16_SINT_PACK16,
		gli::format::FORMAT_RGB16_SINT_PACK16,
		gli::format::FORMAT_RGBA16_SINT_PACK16,
		// 16 bit unsigned
		gli::format::FORMAT_R16_UINT_PACK16,
		gli::format::FORMAT_RG16_UINT_PACK16,
		gli::format::FORMAT_RGB16_UINT_PACK16,
		gli::format::FORMAT_RGBA16_UINT_PACK16,

		// 8 bit signed
		gli::format::FORMAT_R8_SINT_PACK8,
		gli::format::FORMAT_RG8_SINT_PACK8,
		gli::format::FORMAT_RGB8_SINT_PACK8,
		gli::format::FORMAT_RGBA8_SINT_PACK8,

		// 8 bit unsigned
		gli::format::FORMAT_R8_UINT_PACK8,
		gli::format::FORMAT_RG8_UINT_PACK8,
		gli::format::FORMAT_RGB8_UINT_PACK8,
		gli::format::FORMAT_RGBA8_UINT_PACK8,
	};
}


template<class T>
void numpy_save_t(const char* filename, const image::IImage* image, uint32_t nChannels, glm::vec4 minClamp, glm::vec4 maxClamp)
{
	assert(image->getFormat() == gli::FORMAT_RGBA32_SFLOAT_PACK32);

	unsigned long shape[4] = {
		static_cast<unsigned long>(image->getDepth(0) * image->getNumLayers()),
		static_cast<unsigned long>(image->getHeight(0)),
		static_cast<unsigned long>(image->getWidth(0)),
		nChannels
	};

	std::vector<T> outData;
	outData.resize(shape[0] * shape[1] * shape[2] * shape[3]);

	auto cur = outData.begin();
	for(size_t layer = 0; layer < image->getNumLayers(); ++layer)
	{
		size_t size = 0;
		auto data = image->getData(layer, 0, size);
		auto fdata = reinterpret_cast<const glm::vec4*>(data);
		auto fdataEnd = fdata + (size / sizeof(glm::vec4));

		// convert vec4 data to T with nChannels
		for(; fdata != fdataEnd; ++fdata)
		{
			// clamp data to prevent overflows
			auto src = glm::clamp(*fdata, minClamp, maxClamp);
			for(uint32_t i = 0; i < nChannels; ++i)
				*cur++ = static_cast<T>(src[i]);
		}
	}

	//npy::SaveArrayAsNumpy(filename, false, nChannels > 1 ? 4u : 3u, shape, outData);
	// TODO let the user choose if the last channel is exported as a separate channel or not for grayscale
	npy::SaveArrayAsNumpy(filename, false, 4u, shape, outData);
}

void numpy_save(const char* filename, const image::IImage* image, uint32_t format)
{
	const auto fi = gli::detail::get_format_info(gli::format(format));
	const auto nChannels = fi.Component;
	auto [minClamp, maxClamp] = gli::min_max_values(gli::format(format));

	switch(gli::format(format))
	{
		// float formats
		case gli::format::FORMAT_R32_SFLOAT_PACK32:
		case gli::format::FORMAT_RG32_SFLOAT_PACK32:
		case gli::format::FORMAT_RGB32_SFLOAT_PACK32:
		case gli::format::FORMAT_RGBA32_SFLOAT_PACK32:
			numpy_save_t<float>(filename, image, nChannels, minClamp, maxClamp);
			break;
		// int formats
		// 32 bit signed
		case gli::format::FORMAT_R32_SINT_PACK32:
		case gli::format::FORMAT_RG32_SINT_PACK32:
		case gli::format::FORMAT_RGB32_SINT_PACK32:
		case gli::format::FORMAT_RGBA32_SINT_PACK32:
			numpy_save_t<int32_t>(filename, image, nChannels, minClamp, maxClamp);
			break;

		// 32 bit unsigned
		case gli::format::FORMAT_R32_UINT_PACK32:
		case gli::format::FORMAT_RG32_UINT_PACK32:
		case gli::format::FORMAT_RGB32_UINT_PACK32:
		case gli::format::FORMAT_RGBA32_UINT_PACK32:
			numpy_save_t<uint32_t>(filename, image, nChannels, minClamp, maxClamp);
			break;

		// 16 bit signed
		case gli::format::FORMAT_R16_SINT_PACK16:
		case gli::format::FORMAT_RG16_SINT_PACK16:
		case gli::format::FORMAT_RGB16_SINT_PACK16:
		case gli::format::FORMAT_RGBA16_SINT_PACK16:
			numpy_save_t<int16_t>(filename, image, nChannels, minClamp, maxClamp);
			break;

		// 16 bit unsigned
		case gli::format::FORMAT_R16_UINT_PACK16:
		case gli::format::FORMAT_RG16_UINT_PACK16:
		case gli::format::FORMAT_RGB16_UINT_PACK16:
		case gli::format::FORMAT_RGBA16_UINT_PACK16:
			numpy_save_t<uint16_t>(filename, image, nChannels, minClamp, maxClamp);
			break;

		// 8 bit signed
		case gli::format::FORMAT_R8_SINT_PACK8:
		case gli::format::FORMAT_RG8_SINT_PACK8:
		case gli::format::FORMAT_RGB8_SINT_PACK8:
		case gli::format::FORMAT_RGBA8_SINT_PACK8:
			numpy_save_t<int8_t>(filename, image, nChannels, minClamp, maxClamp);
			break;

		// 8 bit unsigned
		case gli::format::FORMAT_R8_UINT_PACK8:
		case gli::format::FORMAT_RG8_UINT_PACK8:
		case gli::format::FORMAT_RGB8_UINT_PACK8:
		case gli::format::FORMAT_RGBA8_UINT_PACK8:
			numpy_save_t<uint8_t>(filename, image, nChannels, minClamp, maxClamp);
			break;

	default:
		throw std::runtime_error("Unsupported format");
	}
}
