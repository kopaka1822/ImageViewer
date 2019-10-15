#include "pch.h"


TEST_CLASS(FilterTest)
{
public:

	TEST_METHOD(SimpleOpen)
	{
		ImageFramework::Model framework;
		framework.OpenFilter("Filter/gamma.hlsl");

		framework.Sync();
	}

	TEST_METHOD(FailedOpen)
	{
		ImageFramework::Model framework;
		framework.OpenFilter("Filter/sfkahfjksas.hlsl");

		Assert::ExpectException<std::runtime_error>([&]() { framework.Sync(); });
	}

	TEST_METHOD(DeleteFilter)
	{
		ImageFramework::Model framework;
		framework.OpenFilter("Filter/gamma.hlsl");
		framework.OpenFilter("Filter/blur.hlsl");
		auto blurParams = framework.GetFilterParams(1);

		framework.DeleteFilter(0);

		framework.Sync();

		//Assert::ExpectException<std::runtime_error>([&]() { framework.GetFilterParams(1); });
		auto newParams = framework.GetFilterParams(0);

		Assert::AreEqual(blurParams.size(), newParams.size());
		for (size_t i = 0; i < blurParams.size(); ++i)
			Assert::AreEqual(blurParams[i].name, newParams[i].name);
	}

	TEST_METHOD(ClearFilter)
	{
		ImageFramework::Model framework;
		framework.OpenFilter("Filter/gamma.hlsl");
		framework.OpenFilter("Filter/blur.hlsl");

		framework.ClearFilter();

		framework.Sync();

		Assert::ExpectException<std::runtime_error>([&]() { framework.GetFilterParams(0); });
	}

	TEST_METHOD(FilterParams)
	{
		ImageFramework::Model framework;
		framework.OpenFilter("Filter/blur.hlsl");
		framework.OpenFilter("Filter/gamma.hlsl");

		framework.SetFilterParam(1, "Gamma", "10");

		auto gammaParams = framework.GetFilterParams(1);
		auto idx = std::find_if(gammaParams.begin(), gammaParams.end(), [](const ImageFramework::Model::FilterParam& p)
			{
				return p.name == "Gamma";
			});

		Assert::IsTrue(idx != gammaParams.end());
		Assert::AreEqual(idx->value, std::string("10"));
	}
};