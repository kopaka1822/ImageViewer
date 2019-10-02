#include "pch.h"
#include "interface.h"
#include <unordered_map>
#include <memory>
#include <algorithm>
#include <fstream>
#include "Image.h"
#include "stbi_interface.h"
#include "gli_interface.h"
#include "pfm_interface.h"
#include "exr_interface.h"
#include <map>
#include "convert.h"

static int s_currentID = 1;
static std::unordered_map<int, std::unique_ptr<image::Image>> s_resources;
std::string s_error;

// key = extension (e.g. png), value = DXGI formats
static std::map<std::string, std::vector<uint32_t>> s_exportFormats;

bool hasEnding(std::string const& fullString, std::string const& ending) {
	if (fullString.length() >= ending.length()) {
		return (0 == fullString.compare(fullString.length() - ending.length(), ending.length(), ending));
	}
	return false;
}

inline bool file_exists(const std::string& name) {
	std::ifstream f(name.c_str());
	return f.good();
}

inline void assertSingleLayerMip(const image::Image& image)
{
	if (image.layer.size() != 1)
		throw std::runtime_error("expected single layer image");
	if (image.layer[0].mipmaps.size() != 1)
		throw std::runtime_error("expected single mipmap image");
}

int image_open(const char* filename)
{
	// try loading the resource
	// transform filename to lowercase for file extension check
	std::string fname = filename;
	std::transform(fname.begin(), fname.end(), fname.begin(), ::tolower);

	std::unique_ptr<image::Image> res;

	try
	{
		if (!file_exists(filename))
			throw std::exception("unable to open file");

		if (hasEnding(fname, ".pfm"))
		{
			res = pfm_load(filename);
		}
		else if (hasEnding(fname, ".dds") || hasEnding(fname, ".ktx"))
		{
			res = gli_load(filename);
		}
		else if (hasEnding(fname, ".exr"))
		{
			res = openexr_load(filename);
		}
		else
		{
			res = stb_image_load(filename);
		}
	}
	catch (const std::exception& e)
	{
		set_error(e.what());
	}
	if (!res) return 0;

	int id = s_currentID++;
	s_resources[id] = move(res);

	return id;
}

int image_allocate(uint32_t format, int width, int height, int layer, int mipmaps)
{
	auto res = std::make_unique<image::Image>();
	res->format = gli::format(format);
	res->original = res->format;
	if(image::isSupported(res->format))
	{
		set_error("image format is not supported for allocate");
		return 0;
	}

	const auto pixelSize = image::pixelSize(res->format);
	res->layer.resize(layer);
	for(auto& l : res->layer)
	{
		l.mipmaps.resize(mipmaps);
		int curWidth = width;
		int curHeight = height;
		for(auto& m : l.mipmaps)
		{
			m.bytes.resize(curWidth * curHeight * pixelSize);
			m.width = curWidth;
			m.height = curHeight;
			m.depth = 1;

			curWidth = std::max(curWidth / 2, 1);
			curHeight = std::max(curHeight / 2, 1);
		}
	}

	int id = s_currentID++;
	s_resources[id] = move(res);

	return id;
}

void image_release(int id)
{
	auto it = s_resources.find(id);
	if (it != s_resources.end())
		s_resources.erase(it);
}

void image_info(int id, uint32_t& format, uint32_t& originalFormat, int& nLayer, int& nMipmaps)
{
	auto it = s_resources.find(id);
	if (it == s_resources.end())
		return;

	format = uint32_t(it->second->format);
	originalFormat = uint32_t(it->second->original);
	nLayer = int(it->second->layer.size());

	nMipmaps = 0;
	if (nLayer > 0)
	{
		nMipmaps = static_cast<int>(it->second->layer.at(0).mipmaps.size());
	}
}

void image_info_mipmap(int id, int mipmap, int& width, int& height)
{
	auto it = s_resources.find(id);
	if (it == s_resources.end())
		return;

	if (it->second->layer.empty())
		return;

	if (unsigned(mipmap) >= it->second->layer[0].mipmaps.size())
		return;

	width = it->second->layer[0].mipmaps[mipmap].width;
	height = it->second->layer[0].mipmaps[mipmap].height;
}

unsigned char* image_get_mipmap(int id, int layer, int mipmap, uint32_t& size)
{
	auto it = s_resources.find(id);
	if (it == s_resources.end())
		return nullptr;

	if (unsigned(layer) >= it->second->layer.size())
		return nullptr;

	if (unsigned(mipmap) >= it->second->layer.at(layer).mipmaps.size())
		return nullptr;

	size = static_cast<int>(it->second->layer.at(layer).mipmaps[mipmap].bytes.size());
	return it->second->layer.at(layer).mipmaps[mipmap].bytes.data();
}

bool image_save(int id, const char* filename, const char* extension, uint32_t format, int quality)
{
	auto it = s_resources.find(id);
	if (it == s_resources.end())
	{
		set_error("invalid image id");
		return false;
	}

	const std::string ext = extension;
	const std::string fullName = filename + std::string(".") + extension;
	try
	{
		if (ext == "dds")
			gli_save_image(fullName.c_str(), *it->second, gli::format(format), false);
		else if (ext == "ktx")
			gli_save_image(fullName.c_str(), *it->second, gli::format(format), true);
		else if (ext == "pfm" || ext == "hdr")
		{
			assertSingleLayerMip(*it->second);
			if (it->second->format != gli::FORMAT_RGBA32_SFLOAT_PACK32)
				throw std::runtime_error ("expected RGBA32F image format for pfm, hdr export");

			auto& mip = it->second->layer[0].mipmaps[0];
			int nComponents = 0;

			// only 2 possible formats
			if (format == gli::FORMAT_RGB32_SFLOAT_PACK32)
			{
				image::changeStride(mip.bytes, 32, 24);
				nComponents = 3;
			}
			else if (format == gli::FORMAT_R32_SFLOAT_PACK32)
			{
				image::changeStride(mip.bytes, 32, 8);
				nComponents = 1;
			}
			else throw std::runtime_error("export format not supported for pfm, hdr");

			if (ext == "hdr")
				stb_save_hdr(fullName.c_str(), mip.width, mip.height, nComponents, mip.bytes.data());
			else if (ext == "pfm")
				pfm_save(fullName.c_str(), mip.width, mip.height, nComponents, mip.bytes.data());
			else assert(false);
		}
		else if (ext == "png" || ext == "jpg" || ext == "bmp")
		{
			assertSingleLayerMip(*it->second);
			if (it->second->format != gli::FORMAT_RGBA8_SRGB_PACK8 &&
				it->second->format != gli::FORMAT_RGBA8_UNORM_PACK8 &&
				it->second->format != gli::FORMAT_RGBA8_SNORM_PACK8)
				throw std::runtime_error("unexpected image format. Expected one of FORMAT_RGBA8_SRGB_PACK8, FORMAT_RGBA8_UNORM_PACK8, FORMAT_RGBA8_SNORM_PACK8");

			auto& mip = it->second->layer[0].mipmaps[0];
			int nComponents = 0;
			if (format == gli::FORMAT_RGBA8_SRGB_PACK8)
			{
				nComponents = 4;
			}
			else if (format == gli::FORMAT_RGB8_SRGB_PACK8)
			{
				nComponents = 3;
				image::changeStride(mip.bytes, 4, 3);
			}
			else if (format == gli::FORMAT_R8_SRGB_PACK8)
			{
				nComponents = 1;
				image::changeStride(mip.bytes, 4, 1);
			}
			else throw std::runtime_error("export format not supported for png, jpg, bmp");

			if (ext == "png")
				stb_save_png(fullName.c_str(), mip.width, mip.height, nComponents, mip.bytes.data());
			else if (ext == "bmp")
				stb_save_bmp(fullName.c_str(), mip.width, mip.height, nComponents, mip.bytes.data());
			else if (ext == "jpg")
				stb_save_jpg(fullName.c_str(), mip.width, mip.height, nComponents, mip.bytes.data(), quality);
			else assert(false);
		}
		else throw std::runtime_error("file extension not supported");
	}
	catch(const std::exception& e)
	{
		set_error(e.what());
		return false;
	}

	return true;
}

const uint32_t* get_export_formats(const char* extension, int& numFormats)
{
	if(s_exportFormats.empty())
	{
		s_exportFormats["dds"] = dds_get_export_formats();
		s_exportFormats["ktx"] = ktx_get_export_formats();
		s_exportFormats["pfm"] = pfm_get_export_formats();
		s_exportFormats["hdr"] = stb_image_get_export_formats("hdr");
		s_exportFormats["jpg"] = stb_image_get_export_formats("jpg");
		s_exportFormats["png"] = stb_image_get_export_formats("png");
		s_exportFormats["bmp"] = stb_image_get_export_formats("bmp");
	}
	auto it = s_exportFormats.find(extension);
	if(it == s_exportFormats.end())
	{	
		numFormats = 0;
		return nullptr;
	}

	numFormats = int(it->second.size());
	return it->second.data();
}

uint32_t get_staging_format(const char* extension)
{
	const auto ext = std::string(extension);
	if (ext == "jpg" || ext == "png" || ext == "bmp")
		return gli::FORMAT_RGBA8_SRGB_PACK8;
	return gli::FORMAT_RGBA32_SFLOAT_PACK32;
}

const char* get_error(int& length)
{
	length = static_cast<int>(s_error.length());
	return s_error.data();
}

void set_error(const std::string& str)
{
	s_error = str;
}
