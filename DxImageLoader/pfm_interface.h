#pragma once
#include "Image.h"
#include <memory>

std::unique_ptr<image::Image> pfm_load(const char* filename);

std::vector<uint32_t> pfm_get_export_formats();