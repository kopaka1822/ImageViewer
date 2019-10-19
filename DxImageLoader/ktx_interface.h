#pragma once
#include <memory>
#include "Image.h"

std::unique_ptr<image::Image> ktx_load(const char* filename);

std::vector<uint32_t> dds_get_export_formats();
//std::vector<uint32_t> ktx_get_export_formats();

//void ktx_save_image(const char* filename, image::Image& image, gli::format format, int quality);