#pragma once
#include <memory>
#include "ImageResource.h"

std::unique_ptr<ImageResource> gli_load(const char* filename);