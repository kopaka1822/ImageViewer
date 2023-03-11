#pragma once
#include <memory>
#include "Image.h"
#include "GliImage.h"

std::unique_ptr<image::IImage> gli_load(const char* filename);

std::vector<uint32_t> dds_get_export_formats();
std::vector<uint32_t> ktx_get_export_formats();

void gli_save_image(const char* filename, GliImage& image, gli::format format, bool ktx, int quality);

gli::format get_format_from_GL(uint32_t internalFormat, uint32_t externalFormat, uint32_t type);
uint32_t get_gl_format(gli::format format);