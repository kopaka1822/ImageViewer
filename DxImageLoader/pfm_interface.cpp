#include "pch.h"
#include "pfm_interface.h"
#include <fstream>
#include <memory>
#include <iostream>
#include "convert.h"
#include "interface.h"

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

std::unique_ptr<image::IImage> pfm_load(const char* filename)
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
	int littleEndianMachine = image::littleendian();
	int needSwap = (littleEndianFile != littleEndianMachine);
	float absScale = std::abs(scalef);

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

	auto res = std::make_unique<image::SimpleImage>(
		grayscale ? gli::FORMAT_R32_SFLOAT_PACK32 : gli::FORMAT_RGB32_SFLOAT_PACK32,
		gli::format::FORMAT_RGBA32_SFLOAT_PACK32,
		width, height, 4 * 4);

	size_t size;
	auto data = reinterpret_cast<float*>(res->getData(0, 0, size));

	if (bands == "Pf") {          // handle 1-band image 

		for (int i = 0; i < height; ++i) {
			for (int j = 0; j < width; ++j) {
				file.read(reinterpret_cast<char*>(&fvalue), sizeof(fvalue));
				if (needSwap) {
					swapBytes(&fvalue);
				}
				auto offset = data + ((height - i - 1) * width + j) * 4;
				fvalue *= absScale; // apply scale
				offset[0] = fvalue; // grayscale => repeat on each channel
				offset[1] = fvalue;
				offset[2] = fvalue;
				offset[3] = 1.0f; // alpha
			}
			set_progress(i * 100 / height);
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
				offset[0] = vfvalue.r * absScale;
				offset[1] = vfvalue.g * absScale;
				offset[2] = vfvalue.b * absScale;
				offset[3] = 1.0f; // alpha
			}
			set_progress(i * 100 / height);
		}
	}
	else
		throw std::exception("invalid header - unknown bands description");

	return res;
}

std::vector<uint32_t> pfm_get_export_formats()
{
	return {
		gli::format::FORMAT_RGB32_SFLOAT_PACK32,
		gli::format::FORMAT_R32_SFLOAT_PACK32,
	};
}

void pfm_save(const char* filename, int width, int height, int components, const void* data)
{
	if (components != 1 && components != 3) 
		throw std::runtime_error("pfm supports either 1 or 3 components");

	std::ofstream file(filename, std::ios::binary);
	if (file.bad() || file.fail())
		throw std::runtime_error("Writing hdr image to " + std::string(filename) + ". cannot open file");
	
	file.write(components == 3 ? "PF\n" : "Pf\n", sizeof(char) * 3);
	file << width << " " << height << "\n";

	file.write("-1.000000\n", sizeof(char) * 10);

	const float* v = reinterpret_cast<const float*>(data);
	for (int y = 0; y < height; ++y)
	{
		for (int x = 0; x < width; ++x)
		{
			for (int c = 0; c < components; ++c)
				file.write(reinterpret_cast<const char*>(&v[c + components * (x + (height - y - 1) * width)]), sizeof(float));
		}
		set_progress(y * 100 / height);
	}
}