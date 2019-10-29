#include "pch.h"
#include "stbi_interface.h"

#define STB_IMAGE_IMPLEMENTATION
#define STB_IMAGE_WRITE_IMPLEMENTATION
#include "../dependencies/stb_image.h"
#include "../dependencies/stb_image_write.h"
#include <fstream>

gli::format getFloatFormat(int numComponents)
{
	switch(numComponents)
	{
	case 1: return gli::FORMAT_R32_SFLOAT_PACK32;
	case 2: return gli::FORMAT_RG32_SFLOAT_PACK32;
	case 3: return gli::FORMAT_RGB32_SFLOAT_PACK32;
	case 4: return gli::FORMAT_RGBA32_SFLOAT_PACK32;
	}
	return gli::FORMAT_UNDEFINED;
}

gli::format getSrgbFormat(int numComponents)
{
	switch(numComponents)
	{
	case 1: return gli::FORMAT_R8_SRGB_PACK8;
	case 2: return gli::FORMAT_RG8_SRGB_PACK8;
	case 3: return gli::FORMAT_RGB8_SRGB_PACK8;
	case 4: return gli::FORMAT_RGBA8_SRGB_PACK8;
	}
	return gli::FORMAT_UNDEFINED;
}

class StbImage final : public image::IImage
{
public:
	StbImage(const char* filename)
	{
		//stbi_set_flip_vertically_on_load(true);
		if (stbi_is_hdr(filename))
		{
			// load hdr file
			int nComponents = 0;
			m_data = reinterpret_cast<stbi_uc*>(stbi_loadf(filename, &m_width, &m_height, &nComponents, 4));
			if (!m_data)
				throw std::exception("error during reading file");

			m_original = getFloatFormat(nComponents);
			m_format = gli::format::FORMAT_RGBA32_SFLOAT_PACK32;
			m_size = m_width * m_height * 4 * 4;
		}
		else
		{
			// load ldr file
			int nComponents = 0;
			m_data = stbi_load(filename, &m_width, &m_height, &nComponents, 4);
			if (!m_data)
				throw std::exception("error during reading file");

			m_original = getSrgbFormat(nComponents);
			m_format = gli::format::FORMAT_RGBA8_SRGB_PACK8;
			m_size = m_width * m_height * 4;
		}
	}

	~StbImage()
	{
		stbi_image_free(m_data);
	}

	uint32_t getNumLayers() const override { return 1; }
	uint32_t getNumMipmaps() const override { return 1; }
	uint32_t getWidth(uint32_t mipmap) const override { return m_width; }
	uint32_t getHeight(uint32_t mipmap) const override { return m_height; }
	gli::format getFormat() const override { return m_format; }
	gli::format getOriginalFormat() const override { return m_original; }
	uint8_t* getData(uint32_t layer, uint32_t mipmap, uint32_t& size) override
	{
		size = m_size;
		return m_data;
	}
	const uint8_t* getData(uint32_t layer, uint32_t mipmap, uint32_t& size) const override
	{
		size = m_size;	
		return m_data;
	}

private:
	stbi_uc* m_data = nullptr;
	int m_width = 0;
	int m_height = 0;
	uint32_t m_size = 0;
	gli::format m_original;
	gli::format m_format;
};

std::unique_ptr<image::IImage> stb_image_load(const char* filename)
{
	return std::make_unique<StbImage>(filename);
	
}

std::vector<uint32_t> stb_image_get_export_formats(const char* extension)
{
	auto ext = std::string(extension);
	if (ext == "bmp" || ext == "jpg")
		return std::vector<uint32_t>{
			gli::format::FORMAT_RGB8_SRGB_PACK8,
			gli::format::FORMAT_R8_SRGB_PACK8
		};
	if (ext == "png")
		return std::vector<uint32_t>{
			gli::format::FORMAT_RGBA8_SRGB_PACK8,
			gli::format::FORMAT_RGB8_SRGB_PACK8,
			gli::format::FORMAT_R8_SRGB_PACK8
		};
	if (ext == "hdr")
		return std::vector<uint32_t>{
			gli::format::FORMAT_RGB32_SFLOAT_PACK32,
			gli::format::FORMAT_R32_SFLOAT_PACK32
		};
	return {};
}

void stb_save_png(const char* filename, int width, int height, int components, const void* data)
{
	stbi_write_png_compression_level = 16;
	//stbi_flip_vertically_on_write(1);
	auto res = stbi_write_png(filename, width, height, components, data, width * components);
	if (!res)
		throw std::exception("could not save file");
}

void stb_save_bmp(const char* filename, int width, int height, int components, const void* data)
{
	//stbi_flip_vertically_on_write(1);
	auto res = stbi_write_bmp(filename, width, height, components, data);
	if (!res)
		throw std::exception("could not save file");
}

void stb_save_hdr(const char* filename, int width, int height, int components, const void* data)
{
	//stbi_flip_vertically_on_write(1);
	auto res = stbi_write_hdr(filename, width, height, components, reinterpret_cast<const float*>(data));
	if (!res)
		throw std::exception("could not save file");
}

void stb_save_jpg(const char* filename, int width, int height, int components, const void* data, int quality)
{
	if (quality < 1 || quality > 100)
		throw std::out_of_range("quality must be between 1 and 100");
	auto res = stbi_write_jpg(filename, width, height, components, data, quality);
	if (!res)
		throw std::exception("could not save file");
}
