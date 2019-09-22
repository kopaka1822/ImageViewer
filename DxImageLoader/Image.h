#pragma once
#include "Layer.h"
#include "Format.h"

namespace image
{
	struct Image
	{
		std::vector<Layer> layer;
		Format format;
	};
}