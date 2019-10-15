#include "pch.h"


TEST_CLASS(ImageTest)
{
public:

	TEST_METHOD(SimpleOpen)
	{
		ImageFramework::Model framework;

		framework.OpenImage(TestDirectory + "small.png");

		Assert::AreEqual(1, framework.GetNumLayers());
	}

	TEST_METHOD(OpenFailed)
	{
		ImageFramework::Model framework;
		framework.OpenImage("dummy.png");

		Assert::ExpectException<std::runtime_error>([&]() { framework.Sync(); });
	}

	TEST_METHOD(DeleteImage)
	{
		ImageFramework::Model framework;
		framework.OpenImage(TestDirectory + "small.png");
		framework.OpenImage(TestDirectory + "small.png");

		framework.DeleteImage(1);

		Assert::AreEqual(1, framework.GetNumLayers());
	}

	TEST_METHOD(ClearImages)
	{
		ImageFramework::Model framework;
		framework.OpenImage(TestDirectory + "small.png");
		framework.OpenImage(TestDirectory + "small.png");

		framework.ClearImages();

		Assert::AreEqual(0, framework.GetNumLayers());
	}

	TEST_METHOD(GenMipmaps)
	{
		ImageFramework::Model framework;
		framework.OpenImage(TestDirectory + "small.png");
		framework.GenMipmaps();

		Assert::AreEqual(2, framework.GetNumMipmaps());
	}

	TEST_METHOD(ReGenMipmaps)
	{
		ImageFramework::Model framework;
		framework.OpenImage(TestDirectory + "checkers.dds");

		Assert::AreEqual(3, framework.GetNumMipmaps());
		framework.GenMipmaps();

		Assert::AreEqual(3, framework.GetNumMipmaps());
	}

	TEST_METHOD(DeleteMipmaps)
	{
		ImageFramework::Model framework;
		framework.OpenImage(TestDirectory + "checkers.dds");

		Assert::AreEqual(3, framework.GetNumMipmaps());
		framework.DeleteMipmaps();

		Assert::AreEqual(1, framework.GetNumMipmaps());
	}

	TEST_METHOD(GetSize)
	{
		ImageFramework::Model framework;
		framework.OpenImage(TestDirectory + "checkers.dds");

		auto sz = framework.GetSize(0);
		Assert::AreEqual(4, sz.x);
		Assert::AreEqual(4, sz.y);

		sz = framework.GetSize(1);
		Assert::AreEqual(2, sz.x);
		Assert::AreEqual(2, sz.y);

		sz = framework.GetSize(2);
		Assert::AreEqual(1, sz.x);
		Assert::AreEqual(1, sz.y);
	}

	TEST_METHOD(IsAlphaTrue)
	{
		ImageFramework::Model framework;
		framework.OpenImage(TestDirectory + "small.dds");

		Assert::IsTrue(framework.IsAlpha());
	}

	TEST_METHOD(IsAlphaFalse)
	{
		ImageFramework::Model framework;
		framework.OpenImage(TestDirectory + "small.hdr");

		Assert::IsFalse(framework.IsAlpha());
	}

	TEST_METHOD(GetPixelColor)
	{
		ImageFramework::Model framework;
		framework.OpenImage(TestDirectory + "small_srgb.ktx");

		auto c = framework.GetPixelColor(0, 0);
		// red
		Assert::AreEqual(1.0f, c.linear.r);
		Assert::AreEqual(0.0f, c.linear.g);
		Assert::AreEqual(0.0f, c.linear.b);
		Assert::AreEqual(1.0f, c.linear.a);
		Assert::AreEqual(uint8_t(255), c.srgb.r);
		Assert::AreEqual(uint8_t(0), c.srgb.g);
		Assert::AreEqual(uint8_t(0), c.srgb.b);
		Assert::AreEqual(uint8_t(255), c.srgb.a);

		c = framework.GetPixelColor(1, 0);
		// green
		Assert::AreEqual(0.0f, c.linear.r);
		Assert::AreEqual(1.0f, c.linear.g);
		Assert::AreEqual(0.0f, c.linear.b);
		Assert::AreEqual(1.0f, c.linear.a);
		Assert::AreEqual(uint8_t(0), c.srgb.r);
		Assert::AreEqual(uint8_t(255), c.srgb.g);
		Assert::AreEqual(uint8_t(0), c.srgb.b);
		Assert::AreEqual(uint8_t(255), c.srgb.a);

		c = framework.GetPixelColor(2, 0);
		// blue
		Assert::AreEqual(0.0f, c.linear.r);
		Assert::AreEqual(0.0f, c.linear.g);
		Assert::AreEqual(1.0f, c.linear.b);
		Assert::AreEqual(1.0f, c.linear.a);
		Assert::AreEqual(uint8_t(0), c.srgb.r);
		Assert::AreEqual(uint8_t(0), c.srgb.g);
		Assert::AreEqual(uint8_t(255), c.srgb.b);
		Assert::AreEqual(uint8_t(255), c.srgb.a);

		c = framework.GetPixelColor(0, 1);
		// black
		Assert::AreEqual(0.0f, c.linear.r);
		Assert::AreEqual(0.0f, c.linear.g);
		Assert::AreEqual(0.0f, c.linear.b);
		Assert::AreEqual(1.0f, c.linear.a);
		Assert::AreEqual(uint8_t(0), c.srgb.r);
		Assert::AreEqual(uint8_t(0), c.srgb.g);
		Assert::AreEqual(uint8_t(0), c.srgb.b);
		Assert::AreEqual(uint8_t(255), c.srgb.a);
	}
};