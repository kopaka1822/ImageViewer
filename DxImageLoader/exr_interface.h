#pragma once
#include "Image.h"
#include <memory>

std::unique_ptr<image::Image> openexr_load(const char* filename);