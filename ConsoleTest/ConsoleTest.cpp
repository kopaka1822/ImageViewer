#include "pch.h"
#include "CppUnitTest.h"
#include "ImageFramework.h"
#include "TestData.h"

using namespace Microsoft::VisualStudio::CppUnitTestFramework;

namespace ConsoleTest
{
	TEST_CLASS(Initialization)
	{
	public:
		
		TEST_METHOD(SimpleCtor)
		{
			{
				ImageFramework::Model framework;
			}
		}

		TEST_METHOD(SimpleOpen)
		{
			ImageFramework::Model framework;

			framework.Open(TestDirectory + "small.png");

			Assert::AreEqual(1, framework.GetNumLayers());
		}

		TEST_METHOD(OpenFailed)
		{
			ImageFramework::Model framework;

			Assert::ExpectException<std::runtime_error>([&]() {framework.Open("dummy.png"); });
		}
	};
}
