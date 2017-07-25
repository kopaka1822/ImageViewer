#pragma once
#include "ImageLayer.h"
#include <vector>
#include "ImageFormat.h"

struct ImageResource
{
	std::vector<ImageLayer> layer;
	ImageFormat format;
};
