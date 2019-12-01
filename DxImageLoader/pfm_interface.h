#pragma once
#include "Image.h"
#include <memory>

std::unique_ptr<image::IImage> pfm_load(const char* filename);

std::vector<uint32_t> pfm_get_export_formats();

void pfm_save(const char* filename, int width, int height, int components, const void* data);