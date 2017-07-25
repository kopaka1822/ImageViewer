#pragma once
#include <vector>
#include "ImageMipmap.h"

struct ImageLayer
{
	std::vector<ImageMipmap> mipmaps;
};
