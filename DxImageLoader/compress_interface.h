#pragma once
#include <memory>
#include "Image.h"

void compressonator_convert_image(image::IImage& src, image::IImage& dst, int quality);

bool is_compressonator_format(gli::format format);