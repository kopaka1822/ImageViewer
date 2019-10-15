#include "pch.h"


TEST_CLASS(StatisticsTest)
{
public:

	TEST_METHOD(Checkers)
	{
		ImageFramework::Model framework;

		framework.OpenImage(TestDirectory + "checkers.dds");
		framework.Sync();

		auto stats = framework.GetStatistics();

		// luminance
		Assert::AreEqual(0.0f, stats.min.luminance);
		Assert::AreEqual(1.0f, stats.max.luminance, 0.01f);
		Assert::AreEqual(0.5f, stats.avg.luminance, 0.01f);

		// luma
		Assert::AreEqual(0.0f, stats.min.luma);
		Assert::AreEqual(1.0f, stats.max.luma, 0.01f);
		Assert::AreEqual(0.5f, stats.avg.luma, 0.01f);

		// lightness
		Assert::AreEqual(0.0f, stats.min.lightness);
		Assert::AreEqual(100.0f, stats.max.lightness, 0.5f);
		Assert::AreEqual(50.0f, stats.avg.lightness, 0.5f);
	}

};