using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageFramework.DirectX;
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

            var dim = models.Images.Size;
            boxes.Boxes.Add(new BoxOverlay.Box
            {
                Start = new Size2(1, 2).ToCoords(dim.XY),
                End = new Size2(8, 7).ToCoords(dim.XY),
                Border = 2,
                Color = new Color(1.0f, 0.0f, 0.0f)
            });

            Assert.IsTrue(boxes.HasWork);
            Assert.IsNotNull(models.Overlay.Overlay);

            var reference = IO.LoadImageTexture(TestData.Directory + "sphere_overlay.png");
            var refColors = reference.GetPixelColors(LayerMipmapSlice.Mip0);
            var actualColors = models.Overlay.Overlay.GetPixelColors(LayerMipmapSlice.Mip0);

            models.Export.Export(new ExportDescription(models.Overlay.Overlay, ExportTest.ExportDir + "overlay_tmp", "png")
            {
                FileFormat = GliFormat.RGBA8_SRGB
            });

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

            var dim = models.Images.Size;
            boxes.Boxes.Add(new BoxOverlay.Box
            {
                Start = new Size2(18, 11).ToCoords(dim.XY),
                End = new Size2(24, 20).ToCoords(dim.XY),
                Border = 1,
                Color = new Color(1.0f, 1.0f, 0.0f)
            });

            boxes.Boxes.Add(new BoxOverlay.Box
            {
                Start = new Size2(7, 9).ToCoords(dim.XY),
                End = new Size2(15, 15).ToCoords(dim.XY),
                Border = 2,
                Color = new Color(1.0f, 0.0f, 0.0f)
            });

            Assert.IsTrue(boxes.HasWork);
            Assert.IsNotNull(models.Overlay.Overlay);

            var filename = ExportTest.ExportDir + "overlayed";
            // create export directory
            ExportTest.Init(null);
            models.Export.Export(new ExportDescription(models.Pipelines[0].Image, filename, "png")
            {
                FileFormat = GliFormat.RGBA8_SRGB,
                Overlay = models.Overlay.Overlay
            });

            var imported = IO.LoadImageTexture(filename + ".png");
            var reference = IO.LoadImageTexture(TestData.Directory + "sphere_with_overlay.png");

            TestData.CompareColors(reference.GetPixelColors(LayerMipmapSlice.Mip0), imported.GetPixelColors(LayerMipmapSlice.Mip0), Color.Channel.Rgba, 0.0f);
        }

        [TestMethod]
        public void CropOverlaySimple()
        {
            var models = new Models(1);
            models.AddImageFromFile(TestData.Directory + "small.pfm");
            models.Apply();
            var dim = models.Images.Size;

            var crop = new CropOverlay(models);
            models.Overlay.Overlays.Add(crop);

            // set up cropping
            crop.Start = new Size3(1, 2, 0).ToCoords(dim);
            crop.End = new Size3(2, 2, 0).ToCoords(dim);
            crop.IsEnabled = false;
            
            // nothing should change
            var expectedColors = models.Pipelines[0].Image.GetPixelColors(LayerMipmapSlice.Mip0);
            var actualColors = GetColorAfterExport(models);

            TestData.CompareColors(expectedColors, actualColors, Color.Channel.Rgba, 0.0f);

            // enable cropping
            crop.IsEnabled = true;
            // gray out colors
            for (int i = 0; i <= 6; ++i)
                expectedColors[i] = expectedColors[i].Scale(0.5f, Color.Channel.Rgb);

            actualColors = GetColorAfterExport(models);
            TestData.CompareColors(expectedColors, actualColors, Color.Channel.Rgba);
        }

        [TestMethod]
        public void CropOverlayGrayOutAll()
        {
            var models = new Models(1);
            models.AddImageFromFile(TestData.Directory + "small.pfm");
            models.Apply();

            var crop = new CropOverlay(models);
            models.Overlay.Overlays.Add(crop);
            
            crop.Start = null;
            crop.End = null;
            crop.IsEnabled = true;

            var expectedColors = models.Pipelines[0].Image.GetPixelColors(LayerMipmapSlice.Mip0);
            for (var i = 0; i < expectedColors.Length; i++)
            {
                expectedColors[i] = expectedColors[i].Scale(0.5f, Color.Channel.Rgb);
            }

            var actualColors = GetColorAfterExport(models);
            TestData.CompareColors(expectedColors, actualColors);
        }

        [TestMethod]
        public void CropOverlayLayers()
        {
            var models = new Models(1);
            models.AddImageFromFile(TestData.Directory + "cubemap.dds");
            models.Apply();
            var dim = models.Images.Size;

            var crop = new CropOverlay(models);
            models.Overlay.Overlays.Add(crop);
            
            // everything should be the same
            crop.Start = Float3.Zero;
            crop.End = Float3.One;
            crop.Layer = -1;
            crop.IsEnabled = true;

            Assert.IsFalse(crop.HasWork);

            crop.Layer = 1;
            var refColors = new Color[][]
            {
                models.Pipelines[0].Image.GetPixelColors(LayerMipmapSlice.Layer0),
                models.Pipelines[0].Image.GetPixelColors(LayerMipmapSlice.Layer1),
                models.Pipelines[0].Image.GetPixelColors(LayerMipmapSlice.Layer2),
                models.Pipelines[0].Image.GetPixelColors(LayerMipmapSlice.Layer3),
                models.Pipelines[0].Image.GetPixelColors(LayerMipmapSlice.Layer4),
                models.Pipelines[0].Image.GetPixelColors(LayerMipmapSlice.Layer5),
            };

            // gray out all layers except layer 1
            for (int layer = 0; layer < 6; ++layer)
            {
                if(layer == 1) continue;
                for (var index = 0; index < refColors[layer].Length; index++)
                {
                    refColors[layer][index] = refColors[layer][index].Scale(0.5f, Color.Channel.Rgb);
                }
            }

            TestData.CompareColors(refColors[0], GetColorAfterExport(models, 0));
            TestData.CompareColors(refColors[1], GetColorAfterExport(models, 1));
            TestData.CompareColors(refColors[2], GetColorAfterExport(models, 2));
            TestData.CompareColors(refColors[3], GetColorAfterExport(models, 3));
            TestData.CompareColors(refColors[4], GetColorAfterExport(models, 4));
            TestData.CompareColors(refColors[5], GetColorAfterExport(models, 5));
            
        }

        private static Color[] GetColorAfterExport(Models models, int layer = 0)
        {
            ExportTest.Init(null);
            models.Export.Export(new ExportDescription(models.Pipelines[0].Image, ExportTest.ExportDir + "crop_test", "dds")
            {
                FileFormat = GliFormat.RGBA32_SFLOAT,
                Layer = layer,
                Overlay = models.Overlay.Overlay
            });
            var imported = IO.LoadImageTexture(ExportTest.ExportDir + "crop_test.dds");
            return imported.GetPixelColors(LayerMipmapSlice.Mip0);
        }
    }
}
