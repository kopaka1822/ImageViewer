#include "ImageLoader.h"
#include <unordered_map>
#include <memory>
#include "ImageResource.h"

int add(int a, int b)
{
	return a + b;
}

static int s_currentID = 1;
static std::unordered_map<int, std::unique_ptr<ImageResource>> s_resources;

int open(const char* filename)
{
	int id = s_currentID++;
	auto& res = s_resources[id] = std::make_unique<ImageResource>();
	// init image resource
	res->format.componentSize = 4;
	res->format.componentType = ImageFormat::COMPONENT_TYPE_INT;
	res->format.componentCount = 3;

	return id;
}

void release(int id)
{
	auto it = s_resources.find(id);
	if (it != s_resources.end())
		s_resources.erase(it);
}

void image_info(int id, int& nComponents, int& componentSize, bool& isIntegerFormat, int& nLayers, int& nMipmaps)
{
	auto it = s_resources.find(id);
	if (it == s_resources.end())
		return;

	nComponents = it->second->format.componentCount;
	componentSize = it->second->format.componentSize;
	isIntegerFormat = it->second->format.componentType == ImageFormat::COMPONENT_TYPE_INT;
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
	height = it->second->layer[0].mipmaps[mipmap].heigt;
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
