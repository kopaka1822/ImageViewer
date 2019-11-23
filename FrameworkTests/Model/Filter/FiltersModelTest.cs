using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageFramework.Model;
using ImageFramework.Model.Filter;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FrameworkTests.Model.Filter
{
    [TestClass]
    public class FiltersModelTest
    {
        private Models models;
        private FiltersModel filters;

        [TestInitialize]
        public void Init()
        {
            models = new Models(1);
            filters = models.Filter;
        }

        [TestCleanup]
        public void Cleanup()
        {
            models.Dispose();
        }

        [TestMethod]
        public void Retarget()
        {
            // add filter (will be added as 2D)
            Assert.AreEqual(FilterLoader.TargetType.Tex2D, filters.CurrentTarget);

            // load gamma filter and set factor to 4.0
            var gamma = models.CreateFilter("filter/gamma.hlsl");
            bool found = false;
            foreach (var param in gamma.Parameters)
            {
                if (param.GetBase().Name == "Factor")
                {
                    param.GetFloatModel().Value = 4.0f;
                    found = true;
                }
            }
            Assert.IsTrue(found);
            models.Filter.AddFilter(gamma);
            
            // add 3d image which should trigger retarget
            models.AddImageFromFile(TestData.Directory + "checkers3d.dds");
            Assert.AreEqual(FilterLoader.TargetType.Tex3D, filters.CurrentTarget);
            Assert.AreEqual(1, filters.Filter.Count);

            // verify that parameter is still set
            found = false;
            foreach (var param in filters.Filter[0].Parameters)
            {
                if (param.GetBase().Name == "Factor")
                {
                    Assert.AreEqual(4.0f, param.GetFloatModel().Value);
                    found = true;
                }
            }
            Assert.IsTrue(found);
        }

        [TestMethod]
        public void RetargetError()
        {
            // add filter that cannot be retargeted
            // add filter (will be added as 2D)
            Assert.AreEqual(FilterLoader.TargetType.Tex2D, filters.CurrentTarget);

            // load gamma filter and set factor to 4.0
            var filter = models.CreateFilter("filter/silhouette.hlsl");
            models.Filter.AddFilter(filter);

            bool retargetFailed = false;
            models.Filter.RetargetError += (o, e) => retargetFailed = true;

            // add 3d image which should trigger retarget
            models.AddImageFromFile(TestData.Directory + "checkers3d.dds");

            Assert.IsTrue(retargetFailed);
            Assert.AreEqual(0, filters.Filter.Count);
        }
    }
}
