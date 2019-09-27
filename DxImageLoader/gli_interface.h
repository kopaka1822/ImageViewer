#pragma once
#include <memory>
#include "Image.h"

std::unique_ptr<image::Image> gli_load(const char* filename);

std::vector<uint32_t> dds_get_export_formats();
std::vector<uint32_t> ktx_get_export_formats();

void gli_save_image(const char* filename, const image::Image& image, gli::format format, bool ktx);