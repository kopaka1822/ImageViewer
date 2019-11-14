using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using ImageFramework.DirectX;
using ImageFramework.Model.Shader;
using ImageFramework.Utility;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FrameworkTests.Model.Shader
{

    [TestClass]
    public class ReduceTest
    {
        [TestMethod]
        public void TestSmall()
        {
            var upload = new UploadBuffer<int>(4);
            upload.SetData(new int[]{1, 2, 3, 4});

            Assert.AreEqual(10, Reduce(upload, new ReduceShader("a+b", "0", "int")));
        }

        [TestMethod]
        public void TestNotSoSmallButStillSmall()
        {
            var upload = new UploadBuffer<int>(13);

            upload.SetData(new int[] { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 });
            Assert.AreEqual(13, Reduce(upload, new ReduceShader("a+b", "0", "int")));

            upload.SetData(new int[]{1,2,3,4,5,6,7,8,9,10,11,12,13});
            Assert.AreEqual(91, Reduce(upload, new ReduceShader("a+b", "0", "int")));
        }

        [TestMethod]
        public void TestMultipleDispatch()
        {
            var upload = new UploadBuffer<int>(ReduceShader.ElementsPerGroup + ReduceShader.ElementsPerGroup / 2 + 1);
            var data = new int[upload.ElementCount];
            for (int i = 0; i < data.Length; ++i)
                data[i] = i + 1;
            upload.SetData(data);

            int expected = (upload.ElementCount * (upload.ElementCount + 1)) / 2;
            Assert.AreEqual(expected, Reduce(upload, new ReduceShader("a+b", "0", "int")));
        }

        [TestMethod]
        public void VeryLarge()
        {
            // 800 mb of data
            var upload = new UploadBuffer<float>(1024 * 1024 * 400);
            var data = new float[upload.ElementCount];
            for (int i = 0; i < upload.ElementCount; ++i)
                data[i] = (float)(i + 1);
            upload.SetData(data);

            float expected = (float) upload.ElementCount;

            Reduce(upload, new ReduceShader("max(a,b)"));
        }


        private T Reduce<T>(UploadBuffer<T> data, ReduceShader shader) where T : struct
        {
            Console.WriteLine("num groups: " + Utility.DivideRoundUp(data.ElementCount, ReduceShader.ElementsPerGroup));

            using (var buf = new GpuBuffer(4, data.ElementCount))
            {
                buf.CopyFrom(data);

                shader.Run(buf, data.ElementCount);

                using (var res = new DownloadBuffer<T>())
                {
                    res.CopyFrom(buf);

                    var resData = res.GetData();

                    return resData;
                }
            }
        }
    }
}
