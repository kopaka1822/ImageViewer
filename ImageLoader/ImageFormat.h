#pragma once
#include <cstdint>

struct ImageFormat
{
	enum ComponentType
	{
		COMPONENT_TYPE_INT,
		COMPONENT_TYPE_FLOAT
	};

	uint8_t componentSize;
	uint8_t componentType;
	uint8_t componentCount;
};
