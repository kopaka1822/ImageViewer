#include "ImageLoader.h"
#include <unordered_map>
#include <memory>
#include "ImageResource.h"
#include "stb_loader.h"
#include "gli_loader.h"
#include "Pfm.h"

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
		{
			res = pfm_load(filename);
			if (!res)
				return 0;
		}
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

void image_info(int id, uint32_t& openglInternalFormat, uint32_t& openglExternalFormat, uint32_t& openglType,
	int& nImages, int& nFaces, int& nMipmaps, bool& isCompressed)
{
	auto it = s_resources.find(id);
	if (it == s_resources.end())
		return;

	openglInternalFormat = it->second->format.openglInternalFormat;
	openglType = it->second->format.openglType;
	openglExternalFormat = it->second->format.openglExternalFormat;
	isCompressed = it->second->format.isCompressed;

	nImages = it->second->layer.size();
	nMipmaps = 0;
	nFaces = 0;
	if(nImages > 0)
	{
		nFaces = it->second->layer.at(0).faces.size();
		if (nFaces > 0)
			nMipmaps = it->second->layer.at(0).faces.at(0).mipmaps.size();
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

	size = it->second->layer.at(image).faces.at(face).mipmaps[mipmap].bytes.size();
	return it->second->layer.at(image).faces.at(face).mipmaps[mipmap].bytes.data();
}
