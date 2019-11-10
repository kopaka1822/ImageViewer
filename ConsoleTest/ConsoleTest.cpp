#include "pch.h"

TEST_CLASS(Initialization)
{
public:
	
	TEST_METHOD(SimpleCtor)
	{
		{
			ImageFramework::Model framework;
		}
	}

	TEST_METHOD(Thumbnail)
	{
		ImageFramework::Model framework;
		framework.OpenImage(TestDirectory + "checkers.dds");
		int width = 0;
		int height = 0;
		auto data = framework.GenThumbnail(2, width, height);

		Assert::AreEqual(2, width);
		Assert::AreEqual(2, height);
		Assert::AreEqual(size_t(2 * 2 * 4), data.size());

		// checkers: Black White
		//           White Black

		const uint8_t white = 254;

		// top left
		Assert::AreEqual(data[0], uint8_t(0)); // red
		Assert::AreEqual(data[1], uint8_t(0)); // green
		Assert::AreEqual(data[2], uint8_t(0)); // blue
		Assert::AreEqual(data[3], uint8_t(255)); // alpha

		// top right
		Assert::AreEqual(data[4], uint8_t(white)); // red
		Assert::AreEqual(data[5], uint8_t(white)); // green
		Assert::AreEqual(data[6], uint8_t(white)); // blue
		Assert::AreEqual(data[7], uint8_t(255)); // alpha

		// bot left
		Assert::AreEqual(data[8], uint8_t(white)); // red
		Assert::AreEqual(data[9], uint8_t(white)); // green
		Assert::AreEqual(data[10], uint8_t(white)); // blue
		Assert::AreEqual(data[11], uint8_t(255)); // alpha

		// bot right
		Assert::AreEqual(data[12], uint8_t(0)); // red
		Assert::AreEqual(data[13], uint8_t(0)); // green
		Assert::AreEqual(data[14], uint8_t(0)); // blue
		Assert::AreEqual(data[15], uint8_t(255)); // alpha
	}

	TEST_METHOD(Thumbnail3D)
	{
		ImageFramework::Model framework;
		framework.OpenImage(TestDirectory + "checkers3d.dds");
		int width = 0;
		int height = 0;
		auto data = framework.GenThumbnail(2, width, height);

		Assert::AreEqual(2, width);
		Assert::AreEqual(2, height);
		Assert::AreEqual(size_t(2 * 2 * 4), data.size());

		// checkers: Black White
		//           White Black

		const uint8_t white = 255;

		// top left
		Assert::AreEqual(data[0], uint8_t(0)); // red
		Assert::AreEqual(data[1], uint8_t(0)); // green
		Assert::AreEqual(data[2], uint8_t(0)); // blue
		Assert::AreEqual(data[3], uint8_t(255)); // alpha

		// top right
		Assert::AreEqual(data[4], uint8_t(white)); // red
		Assert::AreEqual(data[5], uint8_t(white)); // green
		Assert::AreEqual(data[6], uint8_t(white)); // blue
		Assert::AreEqual(data[7], uint8_t(255)); // alpha

		// bot left
		Assert::AreEqual(data[8], uint8_t(white)); // red
		Assert::AreEqual(data[9], uint8_t(white)); // green
		Assert::AreEqual(data[10], uint8_t(white)); // blue
		Assert::AreEqual(data[11], uint8_t(255)); // alpha

		// bot right
		Assert::AreEqual(data[12], uint8_t(0)); // red
		Assert::AreEqual(data[13], uint8_t(0)); // green
		Assert::AreEqual(data[14], uint8_t(0)); // blue
		Assert::AreEqual(data[15], uint8_t(255)); // alpha
	}
};