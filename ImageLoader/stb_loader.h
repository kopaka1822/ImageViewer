#pragma once
#include <memory>
#include "ImageResource.h"
#include <vector>

std::unique_ptr<ImageResource> stb_load(const char* filename);
