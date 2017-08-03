#pragma once
#include "ImageResource.h"
#include <memory>
#include <vector>

std::unique_ptr<ImageResource> pfm_load(const char* filename);
