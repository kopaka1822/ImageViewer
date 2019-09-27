// testmain
#include "ImageLoader.h"


int main(int argc, char** argv)
{
	if(argc >= 2)
	{
		auto id = image_open(argv[1]);

		image_release(id);
	}
	return 0;
}