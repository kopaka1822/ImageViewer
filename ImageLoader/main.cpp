// testmain
#include "ImageLoader.h"


int main(int argc, char** argv)
{
	if(argc >= 2)
	{
		auto id = open(argv[1]);

		release(id);
	}
	return 0;
}