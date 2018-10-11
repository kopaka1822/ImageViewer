#pragma once
#include <stdint.h>

/**
 * \brief 
 * \param filename destination file
 * \param width width in pixel
 * \param height height in pixel
 * \param components image components (1=Red, 2 = RedBlue, 3 = RGB)
 * \param data image data
 * \return true on success
 */
extern "C"
__declspec(dllexport)
bool
__cdecl
save_png(const char* filename, int width, int height, int components, const void* data);

/**
* \brief
* \param filename destination file
* \param width width in pixel
* \param height height in pixel
* \param components image components (1=Red, 2 = RedBlue, 3 = RGB)
* \param data image data
* \return true on success
*/
extern "C"
__declspec(dllexport)
bool
__cdecl
save_bmp(const char* filename, int width, int height, int components, const void* data);

/**
* \brief
* \param filename destination file
* \param width width in pixel
* \param height height in pixel
* \param components image components (1=Red, 2 = RedBlue, 3 = RGB)
* \param data image data
* \return true on success
*/
extern "C"
__declspec(dllexport)
bool
__cdecl
save_hdr(const char* filename, int width, int height, int components, const void* data);

/**
* \brief Stores Grayscale or RGB hdr images to raw float format PFM.
* \param filename destination file
* \param width width in pixel
* \param height height in pixel
* \param components image components (1=Red, 3 = RGB)
* \param data image data
* \return true on success
*/
extern "C"
__declspec(dllexport)
bool
__cdecl
save_pfm(const char* filename, int width, int height, int components, const void* data);

/**
* \brief 
* \param filename destination file
* \param width width in pixel
* \param height height in pixel
* \param components image components (1=Red, 2 = RedBlue, 3 = RGB)
* \param data image data
* \param quality compression quality (1 = lowest, 100 = highest)
* \return true on success
*/
extern "C"
__declspec(dllexport)
bool
__cdecl
save_jpg(const char* filename, int width, int height, int components, const void* data, int quality);

/**
* \brief allocates a texture with the given amount of layers and levels (used for ktx export)
* \param format gli texture format
* \param width width in pixels
* \param height height in pixels
* \param layer number of layers
* \param levels number of levels
*/
extern "C"
__declspec(dllexport)
bool
__cdecl
create_storage(int format, int width, int height, int layer, int levels);

/**
* \brief writes one level into the prevoiusly allocated texture (from create_storage)
* \param layer layer index
* \param level level index
* \param data
* \param size size of data
*/
extern "C"
__declspec(dllexport)
bool
__cdecl
store_level(int layer, int level, const void* data, uint64_t size);

/**
* \brief saves the texture that was allocated by create_storage and filled with store_level into a ktx file
* \param filename
*/
extern "C"
__declspec(dllexport)
bool
__cdecl
save_ktx(const char* filename);