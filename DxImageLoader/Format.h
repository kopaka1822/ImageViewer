#pragma once
#include "framework.h"

namespace image
{
	struct Format
	{
		// format of the texture
		DXGI_FORMAT dxgi;
		// data should be converted from srgb space to linear space before being used for computations
		bool isSrgb;
		// indicates if the texture has a native alpha channel. some texture formats like R8G8B8 do not exist as DXGI_FORMAT
		bool hasAlpha;
	};
}