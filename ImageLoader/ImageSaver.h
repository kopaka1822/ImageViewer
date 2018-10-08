#pragma once

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