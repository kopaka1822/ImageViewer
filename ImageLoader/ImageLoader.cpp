#include "ImageLoader.h"
#include <unordered_map>
#include <memory>
#include "ImageResource.h"
#include "stb_loader.h"
#include "gli_loader.h"
#include "Pfm.h"
#include <algorithm>
#include <fstream>
#include "openexr.h"

static int s_currentID = 1;
static std::unordered_map<int, std::unique_ptr<ImageResource>> s_resources;
std::string s_error;

bool hasEnding(std::string const &fullString, std::string const &ending) {
	if (fullString.length() >= ending.length()) {
		return (0 == fullString.compare(fullString.length() - ending.length(), ending.length(), ending));
	}
	return false;
}

inline bool file_exists(const std::string& name) {
	std::ifstream f(name.c_str());
	return f.good();
}

int open(const char* filename)
{
	// try loading the resource
	// transform filename to lowercase for file extension check
	std::string fname = filename;
	std::transform(fname.begin(), fname.end(), fname.begin(), ::tolower);

	std::unique_ptr<ImageResource> res;

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
			res = stb_load(filename);
		}
	}
	catch(const std::exception& e)
	{
		set_error(e.what());
	}
	if (!res) return 0;

	int id = s_currentID++;
	s_resources[id] = move(res);

	return id;
}

void release(int id)
{
	auto it = s_resources.find(id);
	if (it != s_resources.end())
		s_resources.erase(it);
}

void image_info(int id, uint32_t& openglInternalFormat, uint32_t& openglExternalFormat, uint32_t& openglType,
	int& nImages, int& nFaces, int& nMipmaps, bool& isCompressed, bool& isSrgb)
{
	auto it = s_resources.find(id);
	if (it == s_resources.end())
		return;

	openglInternalFormat = it->second->format.openglInternalFormat;
	openglType = it->second->format.openglType;
	openglExternalFormat = it->second->format.openglExternalFormat;
	isCompressed = it->second->format.isCompressed;
	isSrgb = it->second->format.isSrgb;

	nImages = static_cast<int>(it->second->layer.size());
	nMipmaps = 0;
	nFaces = 0;
	if(nImages > 0)
	{
		nFaces = static_cast<int>(it->second->layer.at(0).faces.size());
		if (nFaces > 0)
			nMipmaps = static_cast<int>(it->second->layer.at(0).faces.at(0).mipmaps.size());
	}
}

void image_info_mipmap(int id, int mipmap, int& width, int& height)
{
	auto it = s_resources.find(id);
	if (it == s_resources.end())
		return;

	if (!it->second->layer.size())
		return;

	if (!it->second->layer.at(0).faces.size())
		return;

	if (unsigned(mipmap) >= it->second->layer[0].faces[0].mipmaps.size())
		return;

	width = it->second->layer[0].faces[0].mipmaps[mipmap].width;
	height = it->second->layer[0].faces[0].mipmaps[mipmap].height;
}

unsigned char* image_get_mipmap(int id, int image, int face, int mipmap, uint32_t& size)
{
	auto it = s_resources.find(id);
	if (it == s_resources.end())
		return nullptr;

	if (unsigned(image) >= it->second->layer.size())
		return nullptr;

	if (unsigned(face) >= it->second->layer.at(image).faces.size())
		return nullptr;

	if (unsigned(mipmap) >= it->second->layer.at(image).faces.at(face).mipmaps.size())
		return nullptr;

	size = static_cast<int>(it->second->layer.at(image).faces.at(face).mipmaps[mipmap].bytes.size());
	return it->second->layer.at(image).faces.at(face).mipmaps[mipmap].bytes.data();
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
