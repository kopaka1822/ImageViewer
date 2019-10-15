#pragma once
#include <memory>
#include "Image.h"

image::Image compressonator_convert_image(const image::Image& image, gli::format format, int quality);

bool is_compressonator_format(gli::format format);