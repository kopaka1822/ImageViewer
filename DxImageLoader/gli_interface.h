#pragma once
#include <memory>
#include "Image.h"

std::unique_ptr<image::Image> gli_load(const char* filename);

/**
 * \brief allocates a texture with the given amount of layers and levels
 * \param format dxgi texture format
 * \param width width in pixels
 * \param height height in pixels
 * \param layer number of layers
 * \param levels number of levels
 */
void gli_create_storage(int format, int width, int height, int layer, int levels);

/**
 * \brief writes one level into the previously allocated texture (from gli_create_storage)
 * \param layer layer index
 * \param level level index
 * \param data
 * \param size size of data
 */
void gli_store_level(int layer, int level, const void* data, uint64_t size);

/**
* \brief retrieves the expected level size
* \param level level index
* \param size size of the level data
*/
void gli_get_level_size(int level, uint64_t& size);

/**
 * \brief saves the texture that was allocated by gli_create_storage and filled with gli_store_level into a ktx file
 * \param filename
 */
void gli_save_ktx(const char* filename);

/**
* \brief saves the texture that was allocated by gli_create_storage and filled with gli_store_level into a dds file
* \param filename
*/
void gli_save_dds(const char* filename);