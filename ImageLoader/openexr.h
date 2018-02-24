#pragma once
#include "ImageResource.h"
#include <memory>
#include <vector>

std::unique_ptr<ImageResource> openexr_load(const char* filename);