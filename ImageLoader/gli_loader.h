#pragma once
#include <memory>
#include "ImageResource.h"

std::unique_ptr<ImageResource> gli_load(const char* filename);

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

/**
 * \brief allocates a texture with the given amount of layers and levels
 * \param format gli texture format
 * \param width width in pixels
 * \param height height in pixels
 * \param layer number of layers
 * \param levels number of levels
 */
void gli_create_storage(int format, int width, int height, int layer, int levels);

/**
 * \brief writes one level into the prevoiusly allocated texture (from gli_create_storage)
 * \param layer layer index
 * \param level level index
 * \param data 
 * \param size size of data
 */
void gli_store_level(int layer, int level, const void* data, uint64_t size);

/**
 * \brief saves the texture that was allocated by gli_create_storage and filled with gli_store_level into a ktx file
 * \param filename 
 */
void gli_save_ktx(const char* filename);