#include "ImageSaver.h"
#include <exception>
#include "ImageLoader.h"
#include "stb_loader.h"

bool save_png(const char* filename, int width, int height, int components, const void* data)
{
	try
	{
		stb_save_png(filename, width, height, components, data);
	}
	catch (const std::exception& e)
	{
		set_error(e.what());
		return false;
	}
	return true;
}

bool save_bmp(const char* filename, int width, int height, int components, const void* data)
{
	try
	{
		stb_save_bmp(filename, width, height, components, data);
	}
	catch (const std::exception& e)
	{
		set_error(e.what());
		return false;
	}
	return true;
}

bool save_hdr(const char* filename, int width, int height, int components, const void* data)
{
	try
	{
		stb_save_hdr(filename, width, height, components, data);
	}
	catch (const std::exception& e)
	{
		set_error(e.what());
		return false;
	}
	return true;
}
