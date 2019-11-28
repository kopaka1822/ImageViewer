using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using ImageFramework.DirectX;
using ImageFramework.DirectX.Query;
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
            var upload = new UploadBuffer(  4*4);
            upload.SetData(new int[]{1, 2, 3, 4});

            Assert.AreEqual(10, Reduce<int>(upload, new ReduceShader(upload, "a+b", "0", "int")));
        }

        [TestMethod]
        public void TestNotSoSmallButStillSmall()
        {
            var upload = new UploadBuffer(4 * 13);

            upload.SetData(new int[] { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 });
            Assert.AreEqual(13, Reduce<int>(upload, new ReduceShader(upload, "a+b", "0", "int")));

            upload.SetData(new int[]{1,2,3,4,5,6,7,8,9,10,11,12,13});
            Assert.AreEqual(91, Reduce<int>(upload, new ReduceShader(upload, "a+b", "0", "int")));
        }

        [TestMethod]
        public void TestMultipleDispatch()
        {
            var upload = new UploadBuffer(4 * (ReduceShader.ElementsPerGroup + ReduceShader.ElementsPerGroup / 2 + 1));
            var count = upload.ByteSize / 4;
            var data = new int[count];
            for (int i = 0; i < data.Length; ++i)
                data[i] = i + 1;
            upload.SetData(data);

            int expected = (count * (count + 1)) / 2;
            Assert.AreEqual(expected, Reduce<int>(upload, new ReduceShader(upload, "a+b", "0", "int")));
        }

        [TestMethod]
        public void VeryLarge()
        {
            // 400 mb of data
            var upload = new UploadBuffer(4 * 1024 * 1024 * 100);
            var count = upload.ByteSize / 4;
            var data = new float[count];
            for (int i = 0; i < count; ++i)
                data[i] = (float)(i + 1);
            upload.SetData(data);

            float expected = (float)count;

            Assert.AreEqual(expected, Reduce<float>(upload, new ReduceShader(upload, "max(a,b)")));
        }


        private T Reduce<T>(UploadBuffer data, ReduceShader shader) where T : struct
        {
            Console.WriteLine("num groups: " + Utility.DivideRoundUp(data.ByteSize / 4, ReduceShader.ElementsPerGroup));

            using (var buf = new GpuBuffer(4, data.ByteSize / 4))
            {
                buf.CopyFrom(data);

                using (var timer = new GpuTimer())
                {
                    timer.Start();
                    //for(int i = 0; i < 100; ++i)
                        shader.Run(buf, data.ByteSize / 4);
                    timer.Stop();
                    Console.WriteLine(timer.GetDelta());
                }

                using (var res = new DownloadBuffer(4))
                {
                    res.CopyFrom(buf);

                    var resData = res.GetData<T>();

                    return resData;
                }
            }
        }
    }
}
