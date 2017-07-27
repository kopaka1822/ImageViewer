#pragma once
#include "ImageResource.h"
#include <memory>

std::unique_ptr<ImageResource> pfm_load(const char* filename);
