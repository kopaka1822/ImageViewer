using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageFramework.ImageLoader;
using ImageFramework.Model;
using ImageFramework.Model.Export;
using ImageFramework.Model.Overlay;
using ImageFramework.Utility;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FrameworkTests.Model
{
    [TestClass]
    public class OverlayTest
    {
        [TestMethod]
        public void TestBoxOverlay()
        {
            var models = new Models(1);
            models.AddImageFromFile( TestData.Directory + "sphere.png");

            // 31 by 31 image
            Assert.AreEqual(models.Images.Size.X, 31);
            Assert.AreEqual(models.Images.Size.Y, 31);

            var boxes = new BoxOverlay(models);
            models.Overlay.Overlays.Add(boxes);

            Assert.IsNull(models.Overlay.Overlay);
            Assert.IsFalse(boxes.HasWork);

            boxes.Boxes.Add(new BoxOverlay.Box
            {
                StartX = 1,
                StartY = 2,
                EndX = 8,
                EndY = 7,
                Border = 2,
                Color = new Color(1.0f, 0.0f, 0.0f)
            });

            Assert.IsTrue(boxes.HasWork);
            Assert.IsNotNull(models.Overlay.Overlay);

            var reference = IO.LoadImageTexture(TestData.Directory + "sphere_overlay.png");
            var refColors = reference.GetPixelColors(0, 0);
            var actualColors = models.Overlay.Overlay.GetPixelColors(0, 0);

            /*models.Export.Export(models.Overlay.Overlay, new ExportDescription(TestData.Directory + "overlay_tmp", "png", models.Export)
            {
                FileFormat = GliFormat.RGBA8_SRGB
            });*/

            TestData.CompareColors(refColors, actualColors, Color.Channel.Rgba, 0.0f);
        }

        [TestMethod]
        public void ExportOverlayed()
        {
            var models = new Models(1);
            models.AddImageFromFile(TestData.Directory + "sphere.png");
            models.Apply();
            // 31 by 31 image
            Assert.AreEqual(models.Images.Size.X, 31);
            Assert.AreEqual(models.Images.Size.Y, 31);

            var boxes = new BoxOverlay(models);
            models.Overlay.Overlays.Add(boxes);

            boxes.Boxes.Add(new BoxOverlay.Box
            {
                StartX = 18,
                StartY = 11,
                EndX = 24,
                EndY = 20,
                Border = 1,
                Color = new Color(1.0f, 1.0f, 0.0f)
            });

            boxes.Boxes.Add(new BoxOverlay.Box
            {
                StartX = 7,
                StartY = 9,
                EndX = 15,
                EndY = 15,
                Border = 2,
                Color = new Color(1.0f, 0.0f, 0.0f)
            });

            Assert.IsTrue(boxes.HasWork);
            Assert.IsNotNull(models.Overlay.Overlay);

            var filename = ExportTest.ExportDir + "overlayed";
            // create export directory
            ExportTest.Init(null);
            models.Export.Export(models.Pipelines[0].Image, new ExportDescription(filename, "png", models.Export)
            {
                FileFormat = GliFormat.RGBA8_SRGB
            });

            var imported = IO.LoadImageTexture(filename + ".png");
            var reference = IO.LoadImageTexture(TestData.Directory + "sphere_with_overlay.png");

            TestData.CompareColors(reference.GetPixelColors(0, 0), imported.GetPixelColors(0, 0), Color.Channel.Rgba, 0.0f);
        }
    }
}
