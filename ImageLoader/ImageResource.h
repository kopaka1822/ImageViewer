#pragma once
#include <vector>
#include "ImageFormat.h"
#include "ImageLayer.h"

struct ImageResource
{
	std::vector<ImageLayer> layer;
	ImageFormat format;
};
