#pragma once
#include <cstdint>
#include <string>

#define EXPORT(rtype) extern "C" __declspec(dllexport) rtype __cdecl

/// \brief tries to open the file with the given filename
/// \param filename absolute or relative path
/// \return returns a non zero integer on success.
/// The error can be retrieved with get_error on failure.
EXPORT(int) image_open(const char* filename);

/// \brief allocates a texture with the given amount of layers and levels
/// \param format dxgi texture format (must be one of the compatible formats, see Image.h)
/// \param width width in pixels
/// \param height height in pixels
/// \param depth depth in pixels
/// \param layer number of layers
/// \param mipmaps number of mipmap levels 
EXPORT(int) image_allocate(uint32_t format, int width, int height, int depth, int layer, int mipmaps);

/// \brief releases all resources from the file with the given id
EXPORT(void) image_release(int id);

/// \brief retrieves image info
/// \brief format internal texture format, will be one of the compatible formats from Image.h
EXPORT(void) image_info(int id, uint32_t& format, uint32_t& originalFormat, int& nLayer, int& nMipmaps);

/// \brief retrieve info for one mipmap
EXPORT(void) image_info_mipmap(int id, int mipmap, int& width, int& height, int& depth);

/// \brief get mipmap bytes
/// \return mipmap data. Can also be used to write mipmap data
EXPORT(unsigned char*) image_get_mipmap(int id, int layer, int mipmap, uint32_t& size);

/// \brief saves the image with the given format and extension
/// \param id valid image id
/// \param filename filename without extension
/// \param extension file format extension (without dot)
/// \param format format that must be compatible with the extension. Can by queried with image_get_export_formats
/// \param quality quality for compressed formats or .jpg. range: [0, 100]
/// \warning the image data might be changed by calling this function. Thus, the image should no longer be used after a call to save
/// \remarks for pfm and hdr export: the image format must be FORMAT_RGBA32_SFLOAT_PACK32.
///          for png, jpg and bmp export the image format must be one of: FORMAT_RGBA8_SRGB_PACK8, FORMAT_RGBA8_UNORM_PACK8, FORMAT_RGBA8_SNORM_PACK8
EXPORT(bool) image_save(int id, const char* filename, const char* extension, uint32_t format, int quality);

/// \brief retrieves an array with all supported dxgi formats that are available for export with the extension
EXPORT(const uint32_t*) get_export_formats(const char* extension, int& numFormats);

typedef void(__stdcall* ProgressCallback)(float, const char*);

/// \brief sets the progress report callback
EXPORT(void) set_progress_callback(ProgressCallback cb);

/// \brief get last error
EXPORT(const char*) get_error(int& length);

/// \brief set current error (for internal use only) 
void set_error(const std::string& str);

/// \brief set current progress (for internal use only)
void set_progress(uint32_t progress, const char* description = nullptr);