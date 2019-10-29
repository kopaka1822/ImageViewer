#pragma once
#include "Image.h"
#include <memory>

std::unique_ptr<image::IImage> openexr_load(const char* filename);