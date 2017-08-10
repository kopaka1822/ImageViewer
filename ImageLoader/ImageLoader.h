#pragma once
#include <cstdint>
#include <string>


/**
 * \brief tries to open the file with the given filename
 * \param filename absolute or relative path
 * \return returns a non zero integer on sucess.
 * The error can be retrieved with get_error on failure.
 */
extern "C"
__declspec(dllexport)
int
__cdecl
open(const char* filename);

extern "C"
__declspec(dllexport)
void
__cdecl
release(int id);

extern "C"
__declspec(dllexport)
void
__cdecl
image_info(int id, uint32_t& openglInternalFormat, uint32_t& openglExternalFormat, uint32_t& openglType, int& nImages, int& nFaces, int& nMipmaps, bool& isCompressed);

extern "C"
__declspec(dllexport)
void
__cdecl
image_info_mipmap(int id, int mipmap, int& width, int& height);

extern "C"
__declspec(dllexport)
unsigned char*
__cdecl
image_get_mipmap(int id, int image, int face, int mipmap, uint32_t& size);

extern "C"
__declspec(dllexport)
const char*
__cdecl
get_error(int& length);

void set_error(const std::string& str);