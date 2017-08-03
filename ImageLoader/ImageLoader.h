#pragma once
#include <cstdint>

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