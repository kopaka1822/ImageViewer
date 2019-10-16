using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FrameworkTests.DirectX;
using ImageFramework.DirectX;
using ImageFramework.ImageLoader;
using ImageFramework.Model;
using ImageFramework.Model.Export;
using ImageFramework.Utility;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using Device = ImageFramework.DirectX.Device;

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
        public void ExportJpg()
        {
            CompareAfterExport(TestData.Directory + "small.bmp", ExportDir + "small", "jpg", GliFormat.RGB8_SRGB, Color.Channel.Rgb, 0.1f);
        }

        [TestMethod]
        public void ExportBmp()
        {
            CompareAfterExport(TestData.Directory + "small.bmp", ExportDir + "small", "bmp", GliFormat.RGB8_SRGB);
        }

        [TestMethod]
        public void ExportPng()
        {
            CompareAfterExport(TestData.Directory + "small.bmp", ExportDir + "small", "png", GliFormat.RGB8_SRGB);
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
        public void ExportAllJpg()
        {
            TryExportAllFormats(TestData.Directory + "small.pfm", ExportDir + "tmp", "jpg");
        }

        [TestMethod]
        public void GrayTestAllJpg()
        {
            TryExportAllFormatsAndCompareGray("jpg");
        }

        [TestMethod]
        public void ExportAllPng()
        {
            TryExportAllFormats(TestData.Directory + "small.pfm", ExportDir + "tmp", "png");
        }

        [TestMethod]
        public void GrayTestAllPng()
        {
            TryExportAllFormatsAndCompareGray("png");
        }

        [TestMethod]
        public void ExportAllBmp()
        {
            TryExportAllFormats(TestData.Directory + "small.pfm", ExportDir + "tmp", "bmp");
        }

        [TestMethod]
        public void GrayTestAllBmp()
        {
            TryExportAllFormatsAndCompareGray("bmp");
        }

        [TestMethod]
        public void ExportPfm()
        {
            CompareAfterExport(TestData.Directory + "small.pfm", ExportDir + "small", "pfm", GliFormat.RGB32_SFLOAT);
        }

        [TestMethod]
        public void ExportAllPfm()
        {
            TryExportAllFormats(TestData.Directory + "small.pfm", ExportDir + "tmp", "pfm");
        }

        [TestMethod]
        public void GrayTestAllPfm()
        {
            TryExportAllFormatsAndCompareGray("pfm");
        }

        [TestMethod]
        public void ExportHdr()
        {
            CompareAfterExport(TestData.Directory + "small.hdr", ExportDir + "small", "hdr", GliFormat.RGB32_SFLOAT);
        }

        [TestMethod]
        public void ExportAllHdr()
        {
            TryExportAllFormats(TestData.Directory + "small.pfm", ExportDir + "tmp", "hdr");
        }

        [TestMethod]
        public void GrayTestAllHdr()
        {
            TryExportAllFormatsAndCompareGray("hdr");
        }

        [TestMethod]
        public void ExportDds()
        {
            CompareAfterExport(TestData.Directory + "small.pfm", ExportDir + "small", "dds", GliFormat.RGBA8_SRGB);
        }

        [TestMethod]
        public void ExportCompressedDds()
        {
            CompareAfterExport(TestData.Directory + "small_scaled.png", ExportDir + "small", "dds", GliFormat.RGBA_DXT1_SRGB);
        }

        [TestMethod]
        public void Dxt1()
        {
            CompareAfterExport(TestData.Directory + "small_scaled.png", ExportDir + "small", "dds", GliFormat.RGBA_DXT1_SRGB);
            CompareAfterExport(TestData.Directory + "small_scaled.png", ExportDir + "small", "dds", GliFormat.RGBA_DXT1_UNORM);
        }

        [TestMethod]
        public void Dxt3()
        {
            CompareAfterExport(TestData.Directory + "small_scaled.png", ExportDir + "small", "dds", GliFormat.RGBA_DXT3_SRGB);
            CompareAfterExport(TestData.Directory + "small_scaled.png", ExportDir + "small", "dds", GliFormat.RGBA_DXT3_UNORM);
        }

        [TestMethod]
        public void Dxt5()
        {
            CompareAfterExport(TestData.Directory + "small_scaled.png", ExportDir + "small", "dds", GliFormat.RGBA_DXT5_SRGB);
            CompareAfterExport(TestData.Directory + "small_scaled.png", ExportDir + "small", "dds", GliFormat.RGBA_DXT5_UNORM);
        }

        [TestMethod]
        public void Atin1()
        {
            CompareAfterExport(TestData.Directory + "small_scaled.png", ExportDir + "small", "dds", GliFormat.R_ATI1N_UNORM, Color.Channel.R);
            CompareAfterExport(TestData.Directory + "small_scaled.png", ExportDir + "small", "dds", GliFormat.R_ATI1N_SNORM, Color.Channel.R);
        }

        [TestMethod]
        public void Atin2()
        {
            CompareAfterExport(TestData.Directory + "small_scaled.png", ExportDir + "small", "dds", GliFormat.RG_ATI2N_UNORM, Color.Channel.R | Color.Channel.G);
            CompareAfterExport(TestData.Directory + "small_scaled.png", ExportDir + "small", "dds", GliFormat.RG_ATI2N_SNORM, Color.Channel.R | Color.Channel.G);
        }

        [TestMethod]
        public void BC6()
        {
            CompareAfterExport(TestData.Directory + "small_scaled.png", ExportDir + "small", "dds", GliFormat.RGB_BP_UFLOAT);
            CompareAfterExport(TestData.Directory + "small_scaled.png", ExportDir + "small", "dds", GliFormat.RGB_BP_SFLOAT);
        }

        [TestMethod]
        public void BC7()
        {
            CompareAfterExport(TestData.Directory + "small_scaled.png", ExportDir + "small", "dds", GliFormat.RGBA_BP_SRGB);
            CompareAfterExport(TestData.Directory + "small_scaled.png", ExportDir + "small", "dds", GliFormat.RGBA_BP_UNORM);
        }

        [TestMethod]
        public void ExportAllDds()
        {
            TryExportAllFormats(TestData.Directory + "small.pfm", ExportDir + "tmp", "dds");
        }

        [TestMethod]
        public void GrayTestAllDds()
        {
            TryExportAllFormatsAndCompareGray("dds");
        }

        [TestMethod]
        public void ExportKtx()
        {
            CompareAfterExport(TestData.Directory + "small.ktx", ExportDir + "small", "ktx", GliFormat.RGBA32_SFLOAT, Color.Channel.Rgba);
        }


        [TestMethod]
        public void ExportCompressedKtx()
        {
            CompareAfterExport(TestData.Directory + "small_scaled.png", ExportDir + "small", "ktx", GliFormat.RGBA_DXT1_SRGB);
        }

        [TestMethod]
        public void ExportAllKtx()
        {
            TryExportAllFormats(TestData.Directory + "small.pfm", ExportDir + "tmp", "ktx");
        }

        [TestMethod]
        public void GrayTestAllKtx()
        {
            TryExportAllFormatsAndCompareGray("ktx");
        }

        /// <summary>
        /// tests if all dds formats actually run on gpu
        /// </summary>
        [TestMethod]
        public void DdsDirectXCompability()
        {
            var model = new Models(1);
            model.AddImageFromFile(TestData.Directory + "small_scaled.png");
            model.Apply();
            model.Export.Quality = 100;
            var origTex = model.Pipelines[0].Image;
            var origColors = origTex.GetPixelColors(0, 0);
            Color[] newColors = null;

            var eFmt = model.Export.Formats.First(fmt => fmt.Extension == "dds");
            string errors = "";
            int numErrors = 0;

            foreach (var format in eFmt.Formats)
            {
                try
                {
                    // export to dds
                    var desc = new ExportDescription(ExportDir + "tmp", "dds", model.Export);
                    desc.FileFormat = format;
                    model.Export.Export(origTex, desc);

                    // load with directx dds loader
                    DDSTextureLoader.CreateDDSTextureFromFile(Device.Get().Handle, Device.Get().ContextHandle, ExportDir + "tmp.dds",
                        out var resource, out var resourceView, 0, out var alphaMode);

                    // convert to obtain color data
                    using (var newTex = model.Export.convert.ConvertFromRaw(resourceView, origTex.Width, origTex.Height,
                        Format.R32G32B32A32_Float))
                    {
                        resourceView.Dispose();
                        resource.Dispose();

                        newColors = newTex.GetPixelColors(0, 0);
                        // only compare with red channel since some formats only store red
                        TestData.CompareColors(origColors, newColors, Color.Channel.R, 0.1f);
                    }
                }
                catch (Exception e)
                {
                    errors += $"{format}: {e.Message}\n";
                    ++numErrors;
                }
            }

            if (errors.Length > 0)
                throw new Exception($"directX compability failed for {numErrors}/{eFmt.Formats.Count} formats:\n" + errors);
        }


        private void CompareAfterExport(string inputImage, string outputImage, string outputExtension, GliFormat format, Color.Channel channels = Color.Channel.Rgb, float tolerance = 0.01f)
        {
            var model = new Models(1);
            model.AddImageFromFile(inputImage);
            model.Apply();
            var origTex = model.Pipelines[0].Image;
            model.Export.Quality = 100;

            model.Export.Export(origTex, new ExportDescription(outputImage, outputExtension, model.Export){FileFormat = format});
            var expTex = new TextureArray2D(IO.LoadImage(outputImage + "." + outputExtension));

            for (int curLayer = 0; curLayer < origTex.NumLayers; ++curLayer)
            {
                for (int curMipmap = 0; curMipmap < origTex.NumMipmaps; ++curMipmap)
                {
                    var origColors = origTex.GetPixelColors(curLayer, curMipmap);
                    var expColor = expTex.GetPixelColors(curLayer, curMipmap);

                    TestData.CompareColors(origColors, expColor, channels, tolerance);
                }
            }
        }

        private void TryExportAllFormats(string inputImage, string outputImage, string outputExtension)
        {
            var model = new Models(1);
            model.AddImageFromFile(inputImage);
            model.Apply();
            var tex = model.Pipelines[0].Image;

            var eFmt = model.Export.Formats.First(fmt => fmt.Extension == outputExtension);
            string errors = "";
            int numErrors = 0;

            foreach (var format in eFmt.Formats)
            {
                try
                {
                    var desc = new ExportDescription(outputImage, outputExtension, model.Export);
                    desc.FileFormat = format;
                    model.Export.Export(tex, desc);
                }
                catch (Exception e)
                {
                    errors += $"{format}: {e.Message}\n";
                    ++numErrors;
                }
            }

            if (errors.Length > 0)
                throw new Exception($"export failed for {numErrors}/{eFmt.Formats.Count} formats:\n" + errors);
        }

        private void TryExportAllFormatsAndCompareGray(string outputExtension)
        {
            var model = new Models(1);
            model.AddImageFromFile(TestData.Directory + "gray.png");
            model.Apply();
            var tex = model.Pipelines[0].Image;
            model.Export.Quality = 100;

            var eFmt = model.Export.Formats.First(fmt => fmt.Extension == outputExtension);

            string errors = "";
            int numErrors = 0;

            var lastTexel = tex.Width * tex.Height - 1;
            Color[] colors = null;
            foreach (var format in eFmt.Formats)
            {
                try
                {
                    var desc = new ExportDescription(ExportDir + "gray", outputExtension, model.Export);
                    desc.FileFormat = format;
                    model.Export.Export(tex, desc);
                    // load and compare gray tone
                    using (var newTex = new TextureArray2D(IO.LoadImage(ExportDir + "gray." + outputExtension)))
                    {
                        Assert.AreEqual(3, newTex.Width);
                        Assert.AreEqual(2, newTex.Height);
                        colors = newTex.GetPixelColors(0, 0);
                        // compare last texel
                        var grayColor = colors[lastTexel];

                        float tolerance = 0.01f;
                        if (format.IsLessThan8Bit())
                            tolerance = 0.1f;

                        // some formats don't write to red
                        // ReSharper disable once CompareOfFloatsByEqualityOperator
                        //if(grayColor.Red != 0.0f) Assert.AreEqual(TestData.Gray, grayColor.Red, tolerance);
                        Assert.AreEqual(TestData.Gray, grayColor.Red, tolerance);
                        //else Assert.AreEqual(TestData.Gray, grayColor.Green, tolerance);
                    }
                }
                catch (Exception e)
                {
                    errors += $"{format.ToString()}: {e.Message}\n";
                    ++numErrors;
                }
            }

            if(errors.Length > 0)
                throw new Exception($"gray comparision failed for {numErrors}/{eFmt.Formats.Count} formats:\n" + errors);
        }

        
    }
}
