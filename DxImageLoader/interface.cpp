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

static int s_currentID = 1;
static std::unordered_map<int, std::unique_ptr<image::Image>> s_resources;
std::string s_error;

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

int open(const char* filename)
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

void release(int id)
{
	auto it = s_resources.find(id);
	if (it != s_resources.end())
		s_resources.erase(it);
}

void image_info(int id, uint32_t& format, int& nLayer, int& nMipmaps, bool& isSrgb, bool& hasAlpha)
{
	auto it = s_resources.find(id);
	if (it == s_resources.end())
		return;

	format = uint32_t(it->second->format.dxgi);
	nLayer = int(it->second->layer.size());

	isSrgb = it->second->format.isSrgb;
	hasAlpha = it->second->format.hasAlpha;

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

const char* get_error(int& length)
{
	length = static_cast<int>(s_error.length());
	return s_error.data();
}

void set_error(const std::string& str)
{
	s_error = str;
}
