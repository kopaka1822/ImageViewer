#pragma once
#include <memory>
#include "Image.h"

std::unique_ptr<image::IImage> stb_image_load(const char* filename);

std::vector<uint32_t> stb_image_get_export_formats(const char* extension);

// helper for exporting
int stb_ldr_get_num_components(gli::format format);
void stb_save_png(const char* filename, int width, int height, int components, const void* data);
void stb_save_bmp(const char* filename, int width, int height, int components, const void* data);
void stb_save_hdr(const char* filename, int width, int height, int components, const void* data);
void stb_save_jpg(const char* filename, int width, int height, int components, const void* data, int quality);