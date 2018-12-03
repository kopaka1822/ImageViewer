#pragma once
#include <cstdint>

struct ImageFormat
{
	enum ComponentType
	{
		COMPONENT_TYPE_INT,
		COMPONENT_TYPE_FLOAT
	};

	// RGB BGRA...
	uint32_t openglInternalFormat;
	uint32_t openglExternalFormat;
	// UNSIGNED_BYTE UNSIGNED_SHORT
	uint32_t openglType;
	bool isCompressed;
	bool isSrgb;
	int gliFormat = 0;
};
