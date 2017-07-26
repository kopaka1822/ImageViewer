#include "ImageLoader.h"
#include <unordered_map>
#include <memory>
#include "ImageResource.h"
#include "stb_loader.h"
#include "gli_loader.h"

int add(int a, int b)
{
	return a + b;
}

static int s_currentID = 1;
static std::unordered_map<int, std::unique_ptr<ImageResource>> s_resources;

int open(const char* filename)
{
	// try loading the resource
	auto res = stb_load(filename);
	if (!res) // TODO set error
	{
		res = gli_load(filename);
		if (!res)
			return 0;
	}

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

void image_info(int id, uint32_t& openglInternalFormat, uint32_t& openglExternalFormat, uint32_t& openglType, int& nLayers, int& nMipmaps)
{
	auto it = s_resources.find(id);
	if (it == s_resources.end())
		return;

	openglInternalFormat = it->second->format.openglInternalFormat;
	openglType = it->second->format.openglType;
	openglExternalFormat = it->second->format.openglExternalFormat;
	nLayers = it->second->layer.size();
	nMipmaps = 0;
	if (it->second->layer.size())
		nMipmaps = it->second->layer[0].mipmaps.size();
}

void image_info_mipmap(int id, int mipmap, int& width, int& height, uint32_t& size)
{
	auto it = s_resources.find(id);
	if (it == s_resources.end())
		return;

	if (!it->second->layer.size())
		return;

	if (unsigned(mipmap) >= it->second->layer[0].mipmaps.size())
		return;

	width = it->second->layer[0].mipmaps[mipmap].width;
	height = it->second->layer[0].mipmaps[mipmap].height;
	size = it->second->layer[0].mipmaps[mipmap].bytes.size();
}

unsigned char* image_get_mipmap(int id, int layer, int mipmap)
{
	auto it = s_resources.find(id);
	if (it == s_resources.end())
		return nullptr;

	if (unsigned(layer) >= it->second->layer.size())
		return nullptr;

	if (unsigned(mipmap) >= it->second->layer[layer].mipmaps.size())
		return nullptr;

	return it->second->layer[layer].mipmaps[mipmap].bytes.data();
}
