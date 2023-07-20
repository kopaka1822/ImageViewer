#pragma once
#include <memory>
#include "Image.h"

std::unique_ptr<image::IImage> numpy_load(const char* filename);
std::vector<uint32_t> numpy_image_get_export_formats();
//void numpy_image_save(const char* filename, const image::IImage* image, uint32_t format);