#pragma once
#include <memory>
#include "Image.h"

std::unique_ptr<image::IImage> png_load(const char* filename);

std::vector<uint32_t> png_get_export_formats();

void png_write(image::IImage& image, const char* filename, gli::format format, int quality);