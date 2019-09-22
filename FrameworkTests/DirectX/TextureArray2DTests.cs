using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageFramework.DirectX;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FrameworkTests.DirectX
{
    [TestClass]
    public class TextureArray2DTests
    {
        [ClassInitialize]
        public void Init()
        {
            Device.Get();
        }

        [TestMethod]
        void TestImageCtor()
        {

        }
    }
}
