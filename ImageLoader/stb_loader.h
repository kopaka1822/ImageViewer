#pragma once
#include <memory>
#include "ImageResource.h"
#include <vector>

std::unique_ptr<ImageResource> stb_load(const char* filename);

void stb_save_png(const char* filename, int width, int height, int components, const void* data);
void stb_save_bmp(const char* filename, int width, int height, int components, const void* data);
void stb_save_hdr(const char* filename, int width, int height, int components, const void* data);
void stb_save_jpg(const char* filename, int width, int height, int components, const void* data, int quality);