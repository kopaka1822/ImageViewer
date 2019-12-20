#pragma once
#include <memory>
#include "Image.h"

std::unique_ptr<image::IImage> png_load(const char* filename);

//std::vector<uint32_t> png_image_get_export_formats(const char* extension);