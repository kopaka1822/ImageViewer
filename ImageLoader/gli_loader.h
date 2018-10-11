#pragma once
#include <memory>
#include "ImageResource.h"

std::unique_ptr<ImageResource> gli_load(const char* filename);
void gli_save_2d_ktx(const char* filename, int format, int width, int height, int levels, const void* data, uint64_t size);


/**
 * \brief converts a gli format into an openGL format
 * \param gliFormat
 * \param glInternal
 * \param glExternal 
 * \param glType 
 * \param isCompressed 
 * \param isSrgb 
 */
extern "C"
__declspec(dllexport)
void
__cdecl
gli_to_opengl_format(int gliFormat, int& glInternal, int& glExternal, int& glType, bool& isCompressed, bool& isSrgb);