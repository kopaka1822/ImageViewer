#pragma once
#include <memory>
#include "Image.h"

std::unique_ptr<image::IImage> hdr_load(const char* filename);

std::vector<uint32_t> hdr_get_export_formats();

void hdr_write(image::IImage& image, const char* filename);