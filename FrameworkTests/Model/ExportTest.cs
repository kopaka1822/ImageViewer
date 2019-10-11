using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageFramework.DirectX;
using ImageFramework.ImageLoader;
using ImageFramework.Model;
using ImageFramework.Model.Export;
using ImageFramework.Utility;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharpDX.Direct3D11;

namespace FrameworkTests.Model
{
    [TestClass]
    public class ExportTest
    {
        public static string ExportDir = TestData.Directory + "export/";

        [ClassInitialize]
        public static void Init(TestContext context)
        {
            if (Directory.Exists(ExportDir))
                Directory.Delete(ExportDir, true);

            // create temporary folder for export data
            Directory.CreateDirectory(ExportDir);
        }

        [TestMethod]
        public void ExportLdr()
        {
            CompareAfterExport(TestData.Directory + "small.bmp", ExportDir + "small", "bmp", GliFormat.RGB8_SRGB);
            CompareAfterExport(TestData.Directory + "small.bmp", ExportDir + "small", "png", GliFormat.RGB8_SRGB);
            CompareAfterExport(TestData.Directory + "small.bmp", ExportDir + "small", "jpg", GliFormat.RGB8_SRGB);
        }

        [TestMethod]
        public void ExportLdrUnorm()
        {
            var model = new Models(1);
            model.AddImageFromFile(TestData.Directory + "small.bmp");
            // set formula to srgb but export with ldr mode unorm
            model.Pipelines[0].Color.Formula = "tosrgb(I0)";
            model.Export.LdrExportMode = ExportModel.LdrMode.UNorm;
            model.Apply();

            model.ExportPipelineImage(ExportDir + "unorm", "bmp", GliFormat.RGB8_SRGB);

            TestData.CompareWithSmall(IO.LoadImage(ExportDir + "unorm.bmp"), Color.Channel.Rgb);
        }

        [TestMethod]
        public void ExportCroppedImage()
        {
            var model = new Models(1);
            model.AddImageFromFile(TestData.Directory + "checkers.dds");
            model.Export.Mipmap = 0;
            model.Export.UseCropping = true;
            model.Export.CropStartX = 1;
            model.Export.CropStartY = 1;
            model.Export.CropEndX = 2;
            model.Export.CropEndY = 2;
            model.Apply();

            model.ExportPipelineImage(ExportDir + "cropped", "dds", GliFormat.RGBA8_SRGB);
            var newTex = new TextureArray2D(IO.LoadImage(ExportDir + "cropped.dds"));

            TestData.TestCheckersLevel1(newTex.GetPixelColors(0, 0));
        }

        [TestMethod]
        public void ExportPfm()
        {
            CompareAfterExport(TestData.Directory + "small.pfm", ExportDir + "small", "pfm", GliFormat.RGB32_SFLOAT);
        }

        [TestMethod]
        public void ExportHdr()
        {
            CompareAfterExport(TestData.Directory + "small.hdr", ExportDir + "small", "hdr", GliFormat.RGB32_SFLOAT);
        }

        [TestMethod]
        public void ExportDds()
        {
            CompareAfterExport(TestData.Directory + "checkers.dds", ExportDir + "checkers", "dds", GliFormat.RGB8_SRGB);
        }

        [TestMethod]
        public void ExportKtx()
        {
            CompareAfterExport(TestData.Directory + "small.ktx", ExportDir + "small", "ktx", GliFormat.RGBA32_SFLOAT, Color.Channel.Rgba);
        }

        private void CompareAfterExport(string inputImage, string outputImage, string outputExtension, GliFormat format, Color.Channel channels = Color.Channel.Rgb, float tolerance = 0.01f)
        {
            var model = new Models(1);
            model.AddImageFromFile(inputImage);
            model.Apply();
            var origTex = model.Pipelines[0].Image;
            
            model.Export.Export(origTex, new ExportDescription(outputImage, outputExtension, model.Export){FileFormat = format});
            var expTex = new TextureArray2D(IO.LoadImage(outputImage + "." + outputExtension));

            for (int curLayer = 0; curLayer < origTex.NumLayers; ++curLayer)
            {
                for (int curMipmap = 0; curMipmap < origTex.NumMipmaps; ++curMipmap)
                {
                    var origColors = origTex.GetPixelColors(curLayer, curMipmap);
                    var expColor = origTex.GetPixelColors(curLayer, curMipmap);

                    TestData.CompareColors(origColors, expColor, channels, tolerance);
                }
            }
        }
    }
}
