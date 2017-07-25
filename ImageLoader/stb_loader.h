#pragma once
#include <memory>
#include "ImageResource.h"

std::unique_ptr<ImageResource> stb_load(const char* filename);
