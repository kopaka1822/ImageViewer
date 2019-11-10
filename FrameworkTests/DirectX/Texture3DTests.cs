using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageFramework.DirectX;
using ImageFramework.ImageLoader;
using ImageFramework.Utility;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FrameworkTests.DirectX
{
    [TestClass]
    public class Texture3DTests
    {

        public static Color[] GetSlice(Color[] data, int startIdx, int count)
        {
            Color[] res = new Color[count];
            for (int i = 0; i < count; ++i)
                res[i] = data[startIdx + i];
            return res;
        }

        [TestMethod]
        public void TestMipmapLoad()
        {
            var tex = new Texture3D(IO.LoadImage(TestData.Directory + "checkers3d.dds"));
            Assert.AreEqual(4, tex.Width);
            Assert.AreEqual(4, tex.Height);
            Assert.AreEqual(4, tex.Depth);
            Assert.AreEqual(3, tex.NumMipmaps);

            // check first two slices
            TestData.TestCheckersLevel0(GetSlice(tex.GetPixelColors(0), 0, 4 * 4));
            TestData.TestCheckersLevel0(GetSlice(tex.GetPixelColors(0), 4 * 4, 4 * 4));

            TestData.TestCheckersLevel1(GetSlice(tex.GetPixelColors(1), 0, 2 * 2));

            TestData.TestCheckersLevel2(tex.GetPixelColors(2));
        }

        [TestMethod]
        public void TestMipmapGen()
        {
            var original = new Texture3D(IO.LoadImage(TestData.Directory + "checkers3d.dds"));
            var tex = original.CloneWithoutMipmapsT();

            // check first two slices
            TestData.TestCheckersLevel0(GetSlice(tex.GetPixelColors(0), 0, 4 * 4));
            TestData.TestCheckersLevel0(GetSlice(tex.GetPixelColors(0), 4 * 4, 4 * 4));

            // gen mipmaps
            tex = tex.GenerateMipmapLevelsT(3);

            TestData.CompareColors(original.GetPixelColors(0), tex.GetPixelColors(0));
            TestData.CompareColors(original.GetPixelColors(1), tex.GetPixelColors(1));
            TestData.CompareColors(original.GetPixelColors(2), tex.GetPixelColors(2));
        }
    }
}
