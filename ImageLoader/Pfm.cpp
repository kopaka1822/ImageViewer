#include "Pfm.h"
#include <fstream>
#include <memory>
#include <iostream>
#include <gli/gl.hpp>

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
	uchar *uval = reinterpret_cast<uchar*>(&intval);
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

bool hasEnding(std::string const &fullString, std::string const &ending) 
{
	if (fullString.length() >= ending.length()) 
		return (0 == fullString.compare(fullString.length() - ending.length(), ending.length(), ending));

	return false;
}

std::unique_ptr<ImageResource> pfm_load(const char* filename) 
{
	if (!hasEnding(filename, ".pfm"))
		return nullptr;
	// create fstream object to read in pfm file 
	// open the file in binary
	std::fstream file(filename, std::ios::in | std::ios::binary);

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
		if (c == ' ' || c == '\t' || c == '\r') {
			//cout << "newline expected";
			return nullptr;
		}
		else {
			//cout << "whitespace expected";
			return nullptr;
		}
	}

	bool grayscale = (bands == "Pf");

	auto res = std::make_unique<ImageResource>();
	res->format.isCompressed = false;
	res->format.openglExternalFormat = res->format.openglInternalFormat = 
		grayscale? gli::gl::external_format::EXTERNAL_RED : gli::gl::external_format::EXTERNAL_RGB;

	res->format.openglType = gli::gl::type_format::TYPE_F32;
	res->layer.emplace_back();
	res->layer.at(0).faces.emplace_back();
	res->layer.at(0).faces.at(0).mipmaps.emplace_back();
	auto& mipmap = res->layer.at(0).faces.at(0).mipmaps.at(0);

	mipmap.width = width;
	mipmap.height = height;
	mipmap.bytes.resize(width * height * 4 * (grayscale? 1 : 3));
	auto data = reinterpret_cast<float*>(mipmap.bytes.data());

	if (bands == "Pf") {          // handle 1-band image 

		//cout << "Reading grayscale image (1-band)" << endl;
		//cout << "Reading into CV_32FC1 image" << endl;

		for (int i = 0; i < height; ++i) {
			for (int j = 0; j < width; ++j) {
				file.read(reinterpret_cast<char*>(&fvalue), sizeof(fvalue));
				if (needSwap) {
					swapBytes(&fvalue);
				}
				*(data + i * width + j) = fvalue;
			}
		}
	}
	else if (bands == "PF") {    // handle 3-band image
		//cout << "Reading color image (3-band)" << endl;
		//cout << "Reading into CV_32FC3 image" << endl;

		for (int i = 0; i < height; ++i) {
			for (int j = 0; j < width; ++j) {
				file.read(reinterpret_cast<char*>(&vfvalue), sizeof(vfvalue));
				if (needSwap) {
					swapBytes(&vfvalue.r);
					swapBytes(&vfvalue.g);
					swapBytes(&vfvalue.b);
				}
				*(data + (i * width + j) * 3) = vfvalue.r;
				*(data + (i * width + j) * 3 + 1) = vfvalue.g;
				*(data + (i * width + j) * 3 + 2) = vfvalue.b;
			}
		}
	}
	else {
		//cout << "unknown bands description";
		return nullptr;
	}
	//cout << setfill('=') << setw(19) << "=" << endl << endl;
	return res;
}
