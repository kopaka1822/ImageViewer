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
#include "hdr_interface.h"
#include "numpy_interface.h"
#include "threadsafe_unordered_map.h"

static std::atomic<int> s_currentID = 1;
static threadsafe_unordered_map<int, image::IImage> s_resources;

std::string s_error;
static ProgressCallback s_progress_callback = nullptr;
static uint32_t s_last_progress = -1;

// key = extension (e.g. png), value = DXGI formats
static std::map<std::string, std::vector<uint32_t>> s_exportFormats;
static std::map<std::string, int> s_globalParameteri;

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
		else if(hasEnding(fname, ".ktx") || hasEnding(fname, ".ktx2"))
		{
			res = ktx_load(filename);
		}
		else if (hasEnding(fname, ".dds"))
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
		else if(hasEnding(fname, ".hdr"))
		{
			res = hdr_load(filename);
		}
		else if(hasEnding(fname, ".npy"))
		{
			res = numpy_load(filename);
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

	if(res->requiresGrayscalePostprocess())
	{
		assert(image::isSupported(res->getFormat()));
		for(uint32_t layer = 0; layer < res->getNumLayers(); ++layer)
			for(uint32_t mip = 0; mip < res->getNumMipmaps(); ++mip)
			{
				size_t size;
				auto data = res->getData(layer, mip, size);
				if (res->getFormat() == gli::FORMAT_RGBA32_SFLOAT_PACK32)
					image::copyRedToGreenBlue<4>(data, size);
				else image::copyRedToGreenBlue<1>(data, size);
			}
	}

	const int id = s_currentID++;
	s_resources.insert(id, move(res));

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

	const int id = s_currentID++;
	s_resources.insert(id, move(res));

	return id;
}

void image_release(int id)
{
	s_resources.erase(id);
}

void image_info(int id, uint32_t& format, uint32_t& originalFormat, int& nLayer, int& nMipmaps)
{
	auto img = s_resources.find(id);
	if(!img)
		return;

	format = uint32_t(img->getFormat());
	originalFormat = uint32_t(img->getOriginalFormat());
	nLayer = int(img->getNumLayers());

	nMipmaps = 0;
	if (nLayer > 0)
	{
		nMipmaps = static_cast<int>(img->getNumMipmaps());
	}
}

void image_info_mipmap(int id, int mipmap, int& width, int& height, int& depth)
{
	auto img = s_resources.find(id);
	if(!img)
		return;

	if (img->getNumLayers() == 0)
		return;

	if (unsigned(mipmap) >= img->getNumMipmaps())
		return;

	width = img->getWidth(mipmap);
	height = img->getHeight(mipmap);
	depth = img->getDepth(mipmap);
}

unsigned char* image_get_mipmap(int id, int layer, int mipmap, uint64_t& size)
{
	auto img = s_resources.find(id);
	if (!img)
		return nullptr;

	if (unsigned(layer) >= img->getNumLayers())
		return nullptr;

	if (unsigned(mipmap) >= img->getNumMipmaps())
		return nullptr;

	return img->getData(layer, mipmap, size);
}

bool image_save(int id, const char* filename, const char* extension, uint32_t format, int quality)
{
	s_last_progress = -1;
	auto img = s_resources.find(id);
	if (!img)
	{
		set_error("invalid image id");
		return false;
	}

	const std::string ext = extension;
	const std::string fullName = filename + std::string(".") + extension;
	try
	{
		if (ext == "dds")
			gli_save_image(fullName.c_str(), dynamic_cast<GliImage&>(*img), gli::format(format), false, quality);
		else if (ext == "ktx")
			gli_save_image(fullName.c_str(), dynamic_cast<GliImage&>(*img), gli::format(format), true, quality);
		else if (ext == "ktx2")
			ktx2_save_image(fullName.c_str(), dynamic_cast<GliImage&>(*img), gli::format(format), quality);
		else if(ext == "hdr")
		{
			assertSingleLayerMip(*img);
			hdr_write(*img, fullName.c_str());
		}
		else if (ext == "pfm")
		{
			assertSingleLayerMip(*img);
			if (img->getFormat() != gli::FORMAT_RGBA32_SFLOAT_PACK32)
				throw std::runtime_error ("expected RGBA32F image format for pfm export");

			size_t mipSize;
			auto mip = img->getData(0, 0, mipSize);
			auto width = img->getWidth(0);
			auto height = img->getHeight(0);
			int nComponents = 0;

			// only 2 possible formats
			if (format == gli::FORMAT_RGB32_SFLOAT_PACK32 || format == gli::FORMAT_RGB8E8_UFLOAT_PACK32)
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

			pfm_save(fullName.c_str(), width, height, nComponents, mip);
		}
		else if(ext == "png")
		{
			assertSingleLayerMip(*img);
			png_write(*img, fullName.c_str(), gli::format(format), quality);
		}
		else if (ext == "jpg" || ext == "bmp" || ext == "tga")
		{
			assertSingleLayerMip(*img);
			if (img->getFormat() != gli::FORMAT_RGBA8_SRGB_PACK8 &&
				img->getFormat() != gli::FORMAT_RGBA8_UNORM_PACK8 &&
				img->getFormat() != gli::FORMAT_RGBA8_SNORM_PACK8)
				throw std::runtime_error("unexpected image format. Expected one of FORMAT_RGBA8_SRGB_PACK8, FORMAT_RGBA8_UNORM_PACK8, FORMAT_RGBA8_SNORM_PACK8");

			size_t mipSize;
			auto mip = img->getData(0, 0, mipSize);
			auto width = img->getWidth(0);
			auto height = img->getHeight(0);
			int nComponents = stb_ldr_get_num_components(gli::format(format));

			if (nComponents == 3)
			{
				image::changeStride(mip, mipSize, 4, 3);
			}
			else if (nComponents == 1)
			{
				image::changeStride(mip, mipSize, 4, 1);
			}

			if (ext == "bmp")
				stb_save_bmp(fullName.c_str(), width, height, nComponents, mip);
			else if (ext == "jpg")
				stb_save_jpg(fullName.c_str(), width, height, nComponents, mip, quality);
			else if (ext == "tga")
				stb_save_tga(fullName.c_str(), width, height, nComponents, mip);
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
		s_exportFormats["hdr"] = hdr_get_export_formats();
		s_exportFormats["jpg"] = stb_image_get_export_formats("jpg");
		s_exportFormats["png"] = png_get_export_formats();
		s_exportFormats["bmp"] = stb_image_get_export_formats("bmp");
		s_exportFormats["ktx2"] = ktx2_get_export_formats();
		s_exportFormats["tga"] = stb_image_get_export_formats("tga");
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

bool get_global_parameter_i(const char* name, int& results)
{
	if (s_globalParameteri.empty())
	{
		// insert default values
		set_global_parameter_i("normalmap", 0);
		set_global_parameter_i("uastc srgb", 0);
	}

	auto it = s_globalParameteri.find(name);
	if (it == s_globalParameteri.end())
	{
	    set_error("could not find global parameter: " + std::string(name));
		return false;
	}

	results = it->second;
	return true;
}

int get_global_parameter_i(const char* name)
{
	int res = 0;
	if(get_global_parameter_i(name, res)) return res;

	throw std::runtime_error(s_error);
}

void set_global_parameter_i(const char* name, int value)
{
	s_globalParameteri[name] = value;
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
