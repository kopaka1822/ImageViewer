using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageFramework.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FrameworkTests.Model
{
    [TestClass]
    public class FinalImageModelTest
    {
        private Models models;

        [TestInitialize]
        public void Init()
        {
            models = new Models();
        }

        [TestMethod]
        public void SingleImage()
        {
            using (var eq = new FinalImageModel(models))
            {

            }
        }
    }
}
