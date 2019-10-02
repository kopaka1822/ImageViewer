using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageFramework.DirectX;
using ImageFramework.ImageLoader;
using ImageFramework.Utility;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FrameworkTests
{
    static class TestData
    {
        public static string Directory = "../../FrameworkTests/TestData/";

        private static readonly Color[] smallData = new Color[]
        {
            new Color(1.0f, 0.0f, 0.0f, 0.5f), new Color(0.0f, 1.0f, 0.0f, 0.5f), new Color(0.0f, 0.0f, 1.0f, 0.5f),
            new Color(0.0f, 0.0f, 0.0f, 0.5f), new Color(0.2122308f, 0.2122308f, 0.2122308f, 0.5f),   new Color(0.0f, 0.0f, 0.0f, 0.5f),
            new Color(1.0f, 1.0f, 1.0f, 0.5f),new Color(1.0f, 1.0f, 1.0f, 0.5f),new Color(1.0f, 1.0f, 1.0f, 0.5f)
        };

        public static void CompareWithSmall(Image image, Color.Channel channels)
        {
            var tex = new TextureArray2D(image);
            var colors = tex.GetPixelColors(0, 0);

            CompareWithSmall(colors, channels);
        }

        public static void CompareWithSmall(Color[] colors, Color.Channel channels)
        {
            Assert.AreEqual(smallData.Length, colors.Length);
            for (int i = 0; i < colors.Length; ++i)
            {
                Assert.IsTrue(colors[i].Equals(smallData[i], channels, 0.02f)); // high tolerance because of jpg compression
            }
        }

        public static void TestCheckersLevel0(Color[] colors)
        {
            Assert.AreEqual(4 * 4, colors.Length);
            Assert.IsTrue(colors[0].Equals(Color.Black, Color.Channel.Rgb));
            Assert.IsTrue(colors[1].Equals(Color.Black, Color.Channel.Rgb));
            Assert.IsTrue(colors[2].Equals(Color.White, Color.Channel.Rgb));
            Assert.IsTrue(colors[3].Equals(Color.White, Color.Channel.Rgb));

            Assert.IsTrue(colors[4].Equals(Color.Black, Color.Channel.Rgb));
            Assert.IsTrue(colors[5].Equals(Color.Black, Color.Channel.Rgb));
            Assert.IsTrue(colors[6].Equals(Color.White, Color.Channel.Rgb));
            Assert.IsTrue(colors[7].Equals(Color.White, Color.Channel.Rgb));

            Assert.IsTrue(colors[8].Equals(Color.White, Color.Channel.Rgb));
            Assert.IsTrue(colors[9].Equals(Color.White, Color.Channel.Rgb));
            Assert.IsTrue(colors[10].Equals(Color.Black, Color.Channel.Rgb));
            Assert.IsTrue(colors[11].Equals(Color.Black, Color.Channel.Rgb));

            Assert.IsTrue(colors[12].Equals(Color.White, Color.Channel.Rgb));
            Assert.IsTrue(colors[13].Equals(Color.White, Color.Channel.Rgb));
            Assert.IsTrue(colors[14].Equals(Color.Black, Color.Channel.Rgb));
            Assert.IsTrue(colors[15].Equals(Color.Black, Color.Channel.Rgb));
        }

        public static void TestCheckersLevel1(Color[] colors)
        {
            Assert.AreEqual(2 * 2, colors.Length);
            Assert.IsTrue(colors[0].Equals(Color.Black, Color.Channel.Rgb));
            Assert.IsTrue(colors[1].Equals(Color.White, Color.Channel.Rgb));
            Assert.IsTrue(colors[2].Equals(Color.White, Color.Channel.Rgb));
            Assert.IsTrue(colors[3].Equals(Color.Black, Color.Channel.Rgb));
        }

        public static void TestCheckersLevel2(Color[] colors)
        {
            Assert.AreEqual(1, colors.Length);
            Assert.IsTrue(colors[0].Equals(new Color(0.5f, 0.5f, 0.5f), Color.Channel.Rgb));
        }

        public static void CompareColors(Color[] left, Color[] right, Color.Channel channels = Color.Channel.Rgb, float tolerance = 0.01f)
        {
            Assert.AreEqual(left.Length, right.Length);
            for (int i = 0; i < left.Length; ++i)
            {
                Assert.IsTrue(left[i].Equals(right[i], channels, tolerance));
            }
        }
    }
}
