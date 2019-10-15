#include "pch.h"


TEST_CLASS(ExportTest)
{
public:
	const std::string ExportDir = TestDirectory + "export/";

	TEST_METHOD(SetExportLayerSimple)
	{
		ImageFramework::Model framework;
		framework.SetExportLayer(0);
		framework.Sync();
	}

	TEST_METHOD(SetExportMipmapSimple)
	{
		ImageFramework::Model framework;
		framework.SetExportMipmap(0);
		framework.Sync();
	}

	TEST_METHOD(SetExportQualitySimple)
	{
		ImageFramework::Model framework;
		framework.SetExportQuality(50);
		framework.Sync();
	}

	TEST_METHOD(DisableExportCroppingSimple)
	{
		ImageFramework::Model framework;
		framework.DisableExportCropping();
		framework.Sync();
	}

	TEST_METHOD(SetExportCroppingSimple)
	{
		ImageFramework::Model framework;
		framework.SetExportCropping(0, 0, 10, 10);
		framework.Sync();
	}

	TEST_METHOD(GetExportFormats)
	{
		ImageFramework::Model framework;
		auto formats = framework.GetExportFormats("png");

		Assert::IsFalse(formats.empty());
	}

	TEST_METHOD(ExportSimple)
	{
		ImageFramework::Model framework;
		framework.OpenImage(TestDirectory + "small.pfm");

		// export and import
		framework.Export(ExportDir + "cppsmall.pfm", "RGB32_SFLOAT");
		framework.OpenImage(ExportDir + "cppsmall.pfm");

		// compare with each other
		framework.SetEquation("abs(I0 - I1)");
		const auto stats = framework.GetStatistics();

		// there should be no difference
		Assert::AreEqual(stats.max.luminance, 0.0f);
	}

	TEST_METHOD(ExportInvalidFormat)
	{
		ImageFramework::Model framework;
		framework.OpenImage(TestDirectory + "small.pfm");

		// export and import
		framework.Export(ExportDir + "cppsmall.pfm", "RGBA8_SRGB");

		Assert::ExpectException<std::runtime_error>([&]() { framework.Sync(); });
	}
};