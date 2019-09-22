#pragma once
#include "Image.h"
#include <memory>

std::unique_ptr<image::Image> pfm_load(const char* filename);
