#pragma once
#include <memory>
#include "Image.h"
#include "GliImage.h"

std::unique_ptr<image::IImage> gli_load(const char* filename);

std::vector<uint32_t> dds_get_export_formats();
std::vector<uint32_t> ktx_get_export_formats();

void gli_save_image(const char* filename, GliImage& image, gli::format format, bool ktx, int quality);