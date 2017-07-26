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
image_info(int id, uint32_t& openglInternalFormat, uint32_t& openglExternalFormat, uint32_t& openglType, int& nLayers, int& nMipmaps);

extern "C"
__declspec(dllexport)
void
__cdecl
image_info_mipmap(int id, int mipmap, int& width, int& height, uint32_t& size);

extern "C"
__declspec(dllexport)
unsigned char*
__cdecl
image_get_mipmap(int id, int layer, int mipmap);