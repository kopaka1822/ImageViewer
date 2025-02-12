#pragma once
#include <memory>
#include "Image.h"

std::unique_ptr<image::IImage> noise_get_white_noise(int width, int height, int depth, int layer, int mipmaps, int seed);

std::unique_ptr<image::IImage> noise_get_blue_noise(int width, int height, int depth, int layer, int mipmaps);