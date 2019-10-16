#pragma once
#include <memory>
#include "Image.h"

std::unique_ptr<image::Image> compressonator_convert_image(image::Image& image, gli::format format, int quality);

bool is_compressonator_format(gli::format format);