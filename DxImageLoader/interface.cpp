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
#include "ktx_interface.h"
#include "png_interface.h"

static int s_currentID = 1;
static std::unordered_map<int, std::unique_ptr<image::IImage>> s_resources;
std::string s_error;
static ProgressCallback s_progress_callback = nullptr;
static uint32_t s_last_progress = -1;

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

inline void assertSingleLayerMip(const image::IImage& image)
{
	if (image.getNumLayers() != 1)
		throw std::runtime_error("expected single layer image");
	if (image.getNumMipmaps() != 1)
		throw std::runtime_error("expected single mipmap image");
	if (image.getDepth(0) != 1)
		throw std::runtime_error("expected 2D texture (depth = 1)");
}

int image_open(const char* filename)
{
	// try loading the resource
	// transform filename to lowercase for file extension check
	s_last_progress = -1;
	std::string fname = filename;
	std::transform(fname.begin(), fname.end(), fname.begin(), ::tolower);

	std::unique_ptr<image::IImage> res;

	try
	{
		if (!file_exists(filename))
			throw std::exception("unable to open file");

		if (hasEnding(fname, ".pfm"))
		{
			res = pfm_load(filename);
		}
		else if(hasEnding(fname, ".ktx2"))
		{
			res = ktx_load(filename);
		}
		else if (hasEnding(fname, ".dds") || hasEnding(fname, ".ktx"))
		{
			res = gli_load(filename);
		}
		else if (hasEnding(fname, ".exr"))
		{
			res = openexr_load(filename);
		}
		else if(hasEnding(fname, ".png"))
		{
			res = png_load(filename);
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

int image_allocate(uint32_t format, int width, int height, int depth, int layer, int mipmaps)
{
	auto res = std::make_unique<GliImage>(gli::format(format), layer, mipmaps, width, height, depth);
	if(!image::isSupported(res->getFormat()))
	{
		set_error("image format is not supported for allocate");
		return 0;
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

	format = uint32_t(it->second->getFormat());
	originalFormat = uint32_t(it->second->getOriginalFormat());
	nLayer = int(it->second->getNumLayers());

	nMipmaps = 0;
	if (nLayer > 0)
	{
		nMipmaps = static_cast<int>(it->second->getNumMipmaps());
	}
}

void image_info_mipmap(int id, int mipmap, int& width, int& height, int& depth)
{
	auto it = s_resources.find(id);
	if (it == s_resources.end())
		return;

	if (it->second->getNumLayers() == 0)
		return;

	if (unsigned(mipmap) >= it->second->getNumMipmaps())
		return;

	width = it->second->getWidth(mipmap);
	height = it->second->getHeight(mipmap);
	depth = it->second->getDepth(mipmap);
}

unsigned char* image_get_mipmap(int id, int layer, int mipmap, uint32_t& size)
{
	auto it = s_resources.find(id);
	if (it == s_resources.end())
		return nullptr;

	if (unsigned(layer) >= it->second->getNumLayers())
		return nullptr;

	if (unsigned(mipmap) >= it->second->getNumMipmaps())
		return nullptr;

	return it->second->getData(layer, mipmap, size);
}

bool image_save(int id, const char* filename, const char* extension, uint32_t format, int quality)
{
	s_last_progress = -1;
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
			gli_save_image(fullName.c_str(), dynamic_cast<GliImage&>(*it->second), gli::format(format), false, quality);
		else if (ext == "ktx")
			gli_save_image(fullName.c_str(), dynamic_cast<GliImage&>(*it->second), gli::format(format), true, quality);
		else if (ext == "pfm" || ext == "hdr")
		{
			assertSingleLayerMip(*it->second);
			if (it->second->getFormat() != gli::FORMAT_RGBA32_SFLOAT_PACK32)
				throw std::runtime_error ("expected RGBA32F image format for pfm, hdr export");

			uint32_t mipSize;
			auto mip = it->second->getData(0, 0, mipSize);
			auto width = it->second->getWidth(0);
			auto height = it->second->getHeight(0);
			int nComponents = 0;

			// only 2 possible formats
			if (format == gli::FORMAT_RGB32_SFLOAT_PACK32)
			{
				image::changeStride(mip, mipSize, 16, 12);
				nComponents = 3;
			}
			else if (format == gli::FORMAT_R32_SFLOAT_PACK32)
			{
				image::changeStride(mip, mipSize, 16, 4);
				nComponents = 1;
			}
			else throw std::runtime_error("export format not supported for pfm, hdr");

			if (ext == "hdr")
				stb_save_hdr(fullName.c_str(), width, height, nComponents, mip);
			else if (ext == "pfm")
				pfm_save(fullName.c_str(), width, height, nComponents, mip);
			else assert(false);
		}
		else if (ext == "png" || ext == "jpg" || ext == "bmp")
		{
			assertSingleLayerMip(*it->second);
			if (it->second->getFormat() != gli::FORMAT_RGBA8_SRGB_PACK8 &&
				it->second->getFormat() != gli::FORMAT_RGBA8_UNORM_PACK8 &&
				it->second->getFormat() != gli::FORMAT_RGBA8_SNORM_PACK8)
				throw std::runtime_error("unexpected image format. Expected one of FORMAT_RGBA8_SRGB_PACK8, FORMAT_RGBA8_UNORM_PACK8, FORMAT_RGBA8_SNORM_PACK8");

			uint32_t mipSize;
			auto mip = it->second->getData(0, 0, mipSize);
			auto width = it->second->getWidth(0);
			auto height = it->second->getHeight(0);
			int nComponents = stb_ldr_get_num_components(gli::format(format));

			if (nComponents == 3)
			{
				image::changeStride(mip, mipSize, 4, 3);
			}
			else if (nComponents == 1)
			{
				image::changeStride(mip, mipSize, 4, 1);
			}

			if (ext == "png")
				stb_save_png(fullName.c_str(), width, height, nComponents, mip);
			else if (ext == "bmp")
				stb_save_bmp(fullName.c_str(), width, height, nComponents, mip);
			else if (ext == "jpg")
				stb_save_jpg(fullName.c_str(), width, height, nComponents, mip, quality);
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

void set_progress_callback(ProgressCallback cb)
{
	s_progress_callback = cb;
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

void set_progress(uint32_t progress, const char* description)
{
	if (!s_progress_callback) return;
	progress = std::min(uint32_t(100), progress);

	if (progress == s_last_progress) return;
	s_last_progress = progress;
	if (description == nullptr) description = "";

	if (s_progress_callback(progress / 100.0f, description))
		throw std::runtime_error("aborted by user");
}
