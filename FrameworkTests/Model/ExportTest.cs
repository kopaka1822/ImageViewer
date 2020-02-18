using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FrameworkTests.DirectX;
using FrameworkTests.Model.Equation;
using ImageFramework.DirectX;
using ImageFramework.ImageLoader;
using ImageFramework.Model;
using ImageFramework.Model.Export;
using ImageFramework.Model.Shader;
using ImageFramework.Utility;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using Device = ImageFramework.DirectX.Device;
using Texture3D = ImageFramework.DirectX.Texture3D;

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
            model.Apply();

            model.ExportPipelineImage(ExportDir + "unorm", "bmp", GliFormat.RGB8_UNORM);

            TestData.CompareWithSmall(IO.LoadImage(ExportDir + "unorm.bmp"), Color.Channel.Rgb);
        }

        [TestMethod]
        public void ExportCroppedImage()
        {
            var model = new Models(1);
            model.AddImageFromFile(TestData.Directory + "checkers.dds");
            model.Apply();

            model.Export.Export(new ExportDescription(model.Pipelines[0].Image, ExportDir + "cropped", "dds")
            {
                FileFormat = GliFormat.RGBA8_SRGB,
                Mipmap = 0,
                UseCropping = true,
                CropStart = new Size3(1, 1, 0).ToCoords(model.Images.Size),
                CropEnd = new Size3(2, 2, 0).ToCoords(model.Images.Size)
            });
            var newTex = new TextureArray2D(IO.LoadImage(ExportDir + "cropped.dds"));

            TestData.TestCheckersLevel1(newTex.GetPixelColors(LayerMipmapSlice.Mip0));
        }

        [TestMethod]
        public void ExportCroppedImage3D()
        {
            var model = new Models(1);
            model.AddImageFromFile(TestData.Directory + "checkers3d.dds");
            model.Apply();
            
            TestData.TestCheckers3DLevel0(model.Pipelines[0].Image.GetPixelColors(LayerMipmapSlice.Mip0));

            model.Export.Export(new ExportDescription(model.Pipelines[0].Image, ExportDir + "cropped", "dds")
            {
                FileFormat = GliFormat.RGBA8_SRGB,
                Mipmap = 0,
                UseCropping = true,
                CropStart = new Size3(1, 0, 2).ToCoords(model.Images.Size),
                CropEnd = new Size3(2, 1, 3).ToCoords(model.Images.Size)
        });

            var newTex = new Texture3D(IO.LoadImage(ExportDir + "cropped.dds"));

            var colors = newTex.GetPixelColors(0);

            Assert.AreEqual(2 * 2 * 2, colors.Length);
            Assert.IsTrue(Color.White.Equals(colors[0], Color.Channel.Rgb));
            Assert.IsTrue(Color.Black.Equals(colors[1], Color.Channel.Rgb));
            Assert.IsTrue(Color.White.Equals(colors[2], Color.Channel.Rgb));
            Assert.IsTrue(Color.Black.Equals(colors[3], Color.Channel.Rgb));

            Assert.IsTrue(Color.White.Equals(colors[4], Color.Channel.Rgb));
            Assert.IsTrue(Color.Black.Equals(colors[5], Color.Channel.Rgb));
            Assert.IsTrue(Color.White.Equals(colors[6], Color.Channel.Rgb));
            Assert.IsTrue(Color.Black.Equals(colors[7], Color.Channel.Rgb));
        }

        [TestMethod]
        public void ExportCroppedWithMipmaps()
        {
            var model = new Models(1);
            model.AddImageFromFile(TestData.Directory + "checkers.dds");
            model.Apply();

            model.Export.Export(new ExportDescription(model.Pipelines[0].Image, ExportDir + "cropped", "dds")
            {
                FileFormat = GliFormat.RGBA8_SRGB,
                Mipmap = -1,
                UseCropping = true,
                CropStart = new Size3(1, 1, 0).ToCoords(model.Images.Size),
                CropEnd = new Size3(2, 2, 0).ToCoords(model.Images.Size)
            });
            var newTex = new TextureArray2D(IO.LoadImage(ExportDir + "cropped.dds"));

            Assert.AreEqual(2, newTex.Size.Width);
            Assert.AreEqual(2, newTex.Size.Height);
            Assert.AreEqual(2, newTex.NumMipmaps);

            TestData.TestCheckersLevel1(newTex.GetPixelColors(LayerMipmapSlice.Mip0));
            TestData.TestCheckersLevel2(newTex.GetPixelColors(LayerMipmapSlice.Mip1));
        }

        [TestMethod]
        public void Export3DSimple()
        {
            var model = new Models(1);
            var orig = IO.LoadImageTexture(TestData.Directory + "checkers3d.dds", out var format);
            model.Images.AddImage(orig, "tsts", format);
            model.Apply();
            

            model.ExportPipelineImage(ExportDir + "tmp3d", "dds", format);

            var newTex = IO.LoadImageTexture(ExportDir + "tmp3d.dds", out var newFormat);
            Assert.AreEqual(format, newFormat);
            Assert.AreEqual(orig.Size, newTex.Size);
            Assert.AreEqual(orig.NumLayers, newTex.NumLayers);
            Assert.AreEqual(orig.NumMipmaps, newTex.NumMipmaps);

            TestData.CompareColors(orig.GetPixelColors(LayerMipmapSlice.Mip0), newTex.GetPixelColors(LayerMipmapSlice.Mip0));
            TestData.CompareColors(orig.GetPixelColors(LayerMipmapSlice.Mip1), newTex.GetPixelColors(LayerMipmapSlice.Mip1));
            TestData.CompareColors(orig.GetPixelColors(LayerMipmapSlice.Mip2), newTex.GetPixelColors(LayerMipmapSlice.Mip2));
        }

        [TestMethod]
        public void Export3DChangeFormat() // only one mipmap + different format
        {
            var model = new Models(1);
            var orig = IO.LoadImageTexture(TestData.Directory + "checkers3d.dds", out var format);
            model.Images.AddImage(orig, "tsts", format);
            model.Apply();

            model.Export.Export(new ExportDescription(model.Pipelines[0].Image, ExportDir + "tmp3d", "dds")
            {
                FileFormat = GliFormat.RGBA32_SFLOAT,
                Mipmap = 0
            });

            var newTex = IO.LoadImageTexture(ExportDir + "tmp3d.dds", out var newFormat);
            Assert.AreEqual(GliFormat.RGBA32_SFLOAT, newFormat);
            Assert.AreEqual(orig.Size, newTex.Size);
            Assert.AreEqual(orig.NumLayers, newTex.NumLayers);
            Assert.AreEqual(1, newTex.NumMipmaps);

            TestData.CompareColors(orig.GetPixelColors(LayerMipmapSlice.Mip0), newTex.GetPixelColors(LayerMipmapSlice.Mip0));
        }
        

        [TestMethod]
        public void ExportAllJpg()
        {
            TryExportAllFormats(TestData.Directory + "small.pfm", ExportDir + "tmp", "jpg");
        }

        [TestMethod]
        public void GrayTestAllJpg()
        {
            TryExportAllFormatsAndCompareGray("jpg", true);
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
            TryExportAllFormatsAndCompareGray("bmp", true);
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
        public void ExportAllUncompressedDds()
        {
            TryExportAllFormats(TestData.Directory + "small.pfm", ExportDir + "tmp", "dds", FormatFilter.Uncompressed);
        }

        [TestMethod]
        public void ExportAllCompressedDds()
        {
            TryExportAllFormats(TestData.Directory + "small_scaled.png", ExportDir + "tmp", "dds", FormatFilter.Compressed);
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
        public void ExportAllUncompressedKtx()
        {
            TryExportAllFormats(TestData.Directory + "small.pfm", ExportDir + "tmp", "ktx", FormatFilter.Uncompressed);
        }

        [TestMethod]
        public void ExportAllCompressedKtx()
        {
            TryExportAllFormats(TestData.Directory + "small_scaled.png", ExportDir + "tmp", "ktx", FormatFilter.Compressed);
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
            var origTex = (TextureArray2D)model.Pipelines[0].Image;
            // normal colors
            var origColors = origTex.GetPixelColors(LayerMipmapSlice.Mip0);
            
            // colors multiplied by 100 for integer precision formats
            model.Pipelines[0].Color.Formula = "I0 * 100";
            model.Apply();
            var integerTex = (TextureArray2D)model.Pipelines[0].Image;
            var origColors100 = integerTex.GetPixelColors(LayerMipmapSlice.Mip0);

            Color[] newColors = null;

            var eFmt = ExportDescription.GetExportFormat("dds");
            string errors = "";
            int numErrors = 0;

            foreach (var format in eFmt.Formats)
            {
                try
                {
                    // export to dds
                    var isIntegerPrecision = IsIntegerPrecisionFormat(format);
                    var desc = new ExportDescription(origTex, ExportDir + "tmp", "dds");
                    desc.FileFormat = format;
                    desc.Quality = 100;
                    if (isIntegerPrecision)
                    {
                        desc.Multiplier = 100.0f;
                    }
                    model.Export.Export(desc);

                    // load with directx dds loader
                    DDSTextureLoader.CreateDDSTextureFromFile(Device.Get().Handle, Device.Get().ContextHandle, ExportDir + "tmp.dds",
                        out var resource, out var resourceView, 0, out var alphaMode);

                    // convert to obtain color data
                    using (var newTex = model.SharedModel.Convert.ConvertFromRaw(resourceView, origTex.Size, Format.R32G32B32A32_Float, isIntegerPrecision))
                    {
                        resourceView.Dispose();
                        resource.Dispose();

                        newColors = newTex.GetPixelColors(LayerMipmapSlice.Mip0);
                        // only compare with red channel since some formats only store red
                        if (isIntegerPrecision)
                        {
                            TestData.CompareColors(origColors100, newColors, Color.Channel.R, 1.0f);
                        }
                        else
                        {
                            TestData.CompareColors(origColors, newColors, Color.Channel.R, 0.1f);
                        }
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

        /// <summary>
        /// tests if all dds formats actually run on gpu
        /// </summary>
        [TestMethod]
        public void Dds3DDirectXCompability()
        {
            var model = new Models(1);
            model.AddImageFromFile(TestData.Directory + "checkers3d.dds");
            model.Apply();
            var origTex = (Texture3D)model.Pipelines[0].Image;
            var origColors = origTex.GetPixelColors(LayerMipmapSlice.Mip0);
            Color[] newColors = null;

            // colors multiplied by 100 for integer precision formats
            model.Pipelines[0].Color.Formula = "I0 * 100";
            model.Apply();
            var integerTex = (Texture3D)model.Pipelines[0].Image;
            var origColors100 = integerTex.GetPixelColors(LayerMipmapSlice.Mip0);

            var eFmt = ExportDescription.GetExportFormat("dds");
            string errors = "";
            int numErrors = 0;
            int nFormats = 0;

            foreach (var format in eFmt.Formats)
            {
                if (format.IsExcludedFrom3DExport()) continue;
                nFormats++;
                try
                {
                    // export to dds
                    var isIntegerPrecision = IsIntegerPrecisionFormat(format);
                    var desc = new ExportDescription(origTex, ExportDir + "tmp", "dds");
                    desc.FileFormat = format;
                    desc.Quality = 100;
                    if (isIntegerPrecision)
                        desc.Multiplier = 100.0f;
                    model.Export.Export(desc);

                    // load with directx dds loader
                    DDSTextureLoader.CreateDDSTextureFromFile(Device.Get().Handle, Device.Get().ContextHandle, ExportDir + "tmp.dds",
                        out var resource, out var resourceView, 0, out var alphaMode);

                    // convert to obtain color data
                    using (var newTex = model.SharedModel.Convert.ConvertFromRaw3D(resourceView, origTex.Size, Format.R32G32B32A32_Float, isIntegerPrecision))
                    {
                        resourceView.Dispose();
                        resource.Dispose();

                        newColors = newTex.GetPixelColors(LayerMipmapSlice.Mip0);
                        // only compare with red channel since some formats only store red
                        if(isIntegerPrecision)
                            TestData.CompareColors(origColors100, newColors, Color.Channel.R, 1.0f);
                        else
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
                throw new Exception($"directX compability failed for {numErrors}/{nFormats} formats:\n" + errors);
        }


        /// <summary>
        /// tests if the dds loader recognizes cubemaps
        /// </summary>
        [TestMethod]
        public void DdsDirectXCubemapCompability()
        {
            // load cubemap
            var model = new Models(1);
            model.AddImageFromFile(TestData.Directory + "cubemap.dds");
            model.Apply();

            // export
            var desc = new ExportDescription(model.Pipelines[0].Image, ExportDir + "tmp", "dds");
            desc.FileFormat = GliFormat.RGBA8_SRGB;
            model.Export.Export(desc);

            // try loading with dds loader
            DDSTextureLoader.CreateDDSTextureFromFile(Device.Get().Handle, Device.Get().ContextHandle, ExportDir + "tmp.dds",
                out var resource, out var resourceView, 0, out var alphaMode);

            Assert.AreEqual(ResourceDimension.Texture2D, resource.Dimension);
            Assert.IsTrue(resource is Texture2D);
            var tex2D = resource as Texture2D;
            Assert.AreEqual(model.Images.Size.Width, tex2D.Description.Width);
            Assert.AreEqual(model.Images.Size.Height, tex2D.Description.Height);
            Assert.AreEqual(model.Images.NumLayers, tex2D.Description.ArraySize);
            Assert.AreEqual(model.Images.NumMipmaps, tex2D.Description.MipLevels);
        }

        private void CompareAfterExport(string inputImage, string outputImage, string outputExtension, GliFormat format, Color.Channel channels = Color.Channel.Rgb, float tolerance = 0.01f)
        {
            var model = new Models(1);
            model.AddImageFromFile(inputImage);
            model.Apply();
            var origTex = (TextureArray2D)model.Pipelines[0].Image;

            model.Export.Export(new ExportDescription(origTex, outputImage, outputExtension)
            {
                FileFormat = format,
                Quality = 100
            });
            var expTex = new TextureArray2D(IO.LoadImage(outputImage + "." + outputExtension));

            foreach (var lm in origTex.LayerMipmap.Range)
            {
                var origColors = origTex.GetPixelColors(lm);
                var expColor = expTex.GetPixelColors(lm);

                TestData.CompareColors(origColors, expColor, channels, tolerance);
            }
        }

        [Flags]
        enum FormatFilter
        {
            Uncompressed = 1,
            Compressed = 1 << 1,
            All = 0xFFFFFFF
        }

        private void TryExportAllFormats(string inputImage, string outputImage, string outputExtension, FormatFilter filter = FormatFilter.All)
        {
            var model = new Models(1);
            model.AddImageFromFile(inputImage);
            model.Apply();
            var tex = (TextureArray2D)model.Pipelines[0].Image;

            var eFmt = ExportDescription.GetExportFormat(outputExtension);
            string errors = "";
            int numErrors = 0;

            foreach (var format in eFmt.Formats)
            {
                if (format.IsCompressed())
                {
                    if ((FormatFilter.Compressed & filter) == 0) continue;
                }
                else if((FormatFilter.Uncompressed & filter) == 0) continue;       

                try
                {
                    var desc = new ExportDescription(tex, outputImage, outputExtension);
                    desc.FileFormat = format;
                    model.Export.Export(desc);
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

        private static bool IsIntegerPrecisionFormat(GliFormat format)
        {
            switch (format.GetDataType())
            {
                case PixelDataType.SInt:
                case PixelDataType.UInt:
                case PixelDataType.SScaled:
                case PixelDataType.UScaled:
                    return true;
            }

            return false;
        }

        private void TryExportAllFormatsAndCompareGray(string outputExtension, bool onlySrgb = false)
        {
            var model = new Models(2);
            model.AddImageFromFile(TestData.Directory + "gray.png");
            model.Apply();
            var tex = (TextureArray2D)model.Pipelines[0].Image;

            var eFmt = ExportDescription.GetExportFormat(outputExtension);

            string errors = "";
            int numErrors = 0;

            var lastTexel = tex.Size.Product - 1;
            Color[] colors = null;
            var i = 0;
            foreach (var format in eFmt.Formats)
            {
                if(onlySrgb && format.GetDataType() != PixelDataType.Srgb) continue;
                try
                {
                    int numTries = 0;
                    while(true)
                    try
                    {
                        var integerPrecision = IsIntegerPrecisionFormat(format);
                        var desc = new ExportDescription(tex, ExportDir + "gray" + ++i, outputExtension);
                        desc.FileFormat = format;
                        desc.Quality = 100;
                        if (integerPrecision)
                            desc.Multiplier = 100.0f;

                        model.Export.Export(desc);
                        Thread.Sleep(1);

                        // load and compare gray tone
                        using (var newTex = new TextureArray2D(IO.LoadImage($"{ExportDir}gray{i}.{outputExtension}")))
                        {
                            Assert.AreEqual(8, newTex.Size.Width);
                            Assert.AreEqual(4, newTex.Size.Height);
                            colors = newTex.GetPixelColors(LayerMipmapSlice.Mip0);
                            // compare last texel
                            var grayColor = colors[lastTexel];

                            float tolerance = 0.01f;
                            if (format.IsLessThan8Bit())
                                tolerance = 0.1f;

                            // some formats don't write to red
                            // ReSharper disable once CompareOfFloatsByEqualityOperator
                            //if(grayColor.Red != 0.0f) Assert.AreEqual(TestData.Gray, grayColor.Red, tolerance);
                            if (integerPrecision)
                            {
                                Assert.AreEqual(TestData.Gray * 100.0f, grayColor.Red, 1.0f);
                            }
                            else
                            {
                                Assert.AreEqual(TestData.Gray, grayColor.Red, tolerance);
                            }
                            //else Assert.AreEqual(TestData.Gray, grayColor.Green, tolerance);
                            break;
                        }
                    }
                    catch (Exception)
                    {
                        ++numTries;
                        if (numTries > 3) throw;
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
