#include "pch.h"
#include "pfm_interface.h"
#include <fstream>
#include <memory>
#include <iostream>

using uchar = unsigned char;

void skip_space(std::fstream& fileStream)
{
	// skip white space in the headers or pnm files

	char c;
	do {
		c = fileStream.get();
	} while (c == '\n' || c == ' ' || c == '\t' || c == '\r');
	fileStream.unget();
}

// check whether machine is little endian
int littleendian()
{
	int intval = 1;
	uchar* uval = reinterpret_cast<uchar*>(&intval);
	return uval[0] == 1;
}

void swapBytes(float* fptr)
{ // if endianness doesn't agree, swap bytes
	uchar* ptr = reinterpret_cast<uchar*>(fptr);
	uchar tmp = 0;
	tmp = ptr[0]; ptr[0] = ptr[3]; ptr[3] = tmp;
	tmp = ptr[1]; ptr[1] = ptr[2]; ptr[2] = tmp;
}

struct Pixel
{
	float r, g, b;
};

std::unique_ptr<image::Image> pfm_load(const char* filename)
{
	// create fstream object to read in pfm file 
	// open the file in binary
	std::fstream file(filename, std::ios::in | std::ios::binary);
	if (!file.is_open())
		throw std::exception("error opening file");

	//                          "PF" = color        (3-band)
	int width, height;      // width and height of the image
	float scalef, fvalue;   // scale factor and temp value to hold pixel value
	Pixel vfvalue;          // temp value to hold 3-band pixel value

							// extract header information, skips whitespace 
	//file >> bands;
	char bandBuffer[3];
	file.read(bandBuffer, 2);
	bandBuffer[2] = '\0';
	skip_space(file);
	std::string bands = bandBuffer;

	file >> width;
	file >> height;
	file >> scalef;

	// determine endianness 
	int littleEndianFile = (scalef < 0);
	int littleEndianMachine = littleendian();
	int needSwap = (littleEndianFile != littleEndianMachine);

	// skip SINGLE newline character after reading third arg
	char c = file.get();
	if (c == '\r')      // <cr> in some files before newline
		c = file.get();
	if (c != '\n') {
		if (c == ' ' || c == '\t' || c == '\r')
			throw std::exception("invalid header - newline expected");
		throw std::exception("invalid header - whitespace expected");
	}

	bool grayscale = (bands == "Pf");

	auto res = std::make_unique<image::Image>();
	res->original = grayscale ? gli::FORMAT_R32_SFLOAT_PACK32 : gli::FORMAT_RGB32_SFLOAT_PACK32;
	res->format = gli::format::FORMAT_RGBA32_SFLOAT_PACK32;
	res->layer.emplace_back();
	res->layer.at(0).mipmaps.emplace_back();
	auto& mipmap = res->layer.at(0).mipmaps.at(0);

	mipmap.width = width;
	mipmap.height = height;
	mipmap.bytes.resize(width * height * 4 * 4);
	auto data = reinterpret_cast<float*>(mipmap.bytes.data());

	if (bands == "Pf") {          // handle 1-band image 

		for (int i = 0; i < height; ++i) {
			for (int j = 0; j < width; ++j) {
				file.read(reinterpret_cast<char*>(&fvalue), sizeof(fvalue));
				if (needSwap) {
					swapBytes(&fvalue);
				}
				auto offset = data + ((height - i - 1) * width + j) * 4;
				offset[0] = fvalue; // grayscale => repeat on each channel
				offset[1] = fvalue;
				offset[2] = fvalue;
				offset[3] = 1.0f; // alpha
			}
		}
	}
	else if (bands == "PF") {    // handle 3-band image

		for (int i = 0; i < height; ++i) {
			for (int j = 0; j < width; ++j) {
				file.read(reinterpret_cast<char*>(&vfvalue), sizeof(vfvalue));
				if (needSwap) {
					swapBytes(&vfvalue.r);
					swapBytes(&vfvalue.g);
					swapBytes(&vfvalue.b);
				}
				auto offset = data + ((height - i - 1) * width + j) * 4;
				offset[0] = vfvalue.r;
				offset[1] = vfvalue.g;
				offset[2] = vfvalue.b;
				offset[3] = 1.0f; // alpha
			}
		}
	}
	else
		throw std::exception("invalid header - unknown bands description");

	return res;
}

std::vector<uint32_t> pfm_get_export_formats()
{
	return {
		gli::format::FORMAT_RGBA32_SFLOAT_PACK32,
		gli::format::FORMAT_R32_SFLOAT_PACK32,
	};
}
