using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using ImageFramework.DirectX;
using ImageFramework.ImageLoader;
using ImageFramework.Model;
using ImageFramework.Utility;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharpDX.DXGI;

namespace FrameworkTests.ImageLoader
{
    [TestClass]
    public class KtxSamples
    {
        public static string ImportDir = TestData.Directory + "ktx-software\\testimages\\";

        public static string ImportBadDir = TestData.Directory + "ktx-software\\badktx2\\";

        public static string ExportDir = TestData.Directory + "export/";

        [ClassInitialize]
        public static void Init(TestContext context)
        {
            TestData.CreateOutputDirectory(ExportDir);
        }

        private static string[] RemoveUnsupportedFile(string[] files)
        {
            // remove all files that have 'eac' or 'etc' in their name (these compressions are not supported yet)
            return files.Where(f => 
                !f.Contains("eac") && !f.Contains("EAC") &&
                !f.Contains("etc") && !f.Contains("ETC")
            ).ToArray();
        }

        [TestMethod]
        public void ImportTestImagesKtx()
        {
            // get all files in the directory
            var files = System.IO.Directory.GetFiles(ImportDir, "*.ktx", System.IO.SearchOption.TopDirectoryOnly);
            // filter files so they only end with .ktx (not ktx2)
            files = files.Where(f => f.EndsWith(".ktx")).ToArray();
            files = RemoveUnsupportedFile(files);
            TryImportAllFiles(files);
        }

        [TestMethod]
        public void ImportTestImagesKtx2()
        {
            // get all files in the directory
            var files = System.IO.Directory.GetFiles(ImportDir, "*.ktx2", System.IO.SearchOption.TopDirectoryOnly);
            files = RemoveUnsupportedFile(files);
            TryImportAllFiles(files);
        }

        [TestMethod]
        public void AlignmentKtx()
        {
            // this file uses the RGB format and the width of each row is not a multiple of 4 (this can be tricky to import/export)
            var filename = ImportDir + "hi_mark_sq.ktx";
            var model = new Models();
            model.AddImageFromFile(filename);

            // the following needs to be fulfilled:
            Assert.AreEqual(145, model.Images.GetWidth(0));
            Assert.AreEqual(model.Images.Images[0].OriginalFormat, GliFormat.RGB8_UNORM);

            // export and reimport
            var filename2 = ExportDir + "hi_mark_sq";
            model.ExportPipelineImage(filename2, "ktx", GliFormat.RGB8_UNORM);
            
            // reimport
            model.AddImageFromFile(filename2 + ".ktx");
            
            // compare colors
            var srcImg = model.Images.Images[0].Image as TextureArray2D;
            var expImg = model.Images.Images[1].Image as TextureArray2D;
            Assert.IsNotNull(srcImg);
            Assert.IsNotNull(expImg);

            var srcColors = srcImg.GetPixelColors(LayerMipmapSlice.Mip0);
            var expColors = expImg.GetPixelColors(LayerMipmapSlice.Mip0);

            TestData.CompareColors(srcColors, expColors);
        }

        void TryImportAllFiles(string[] files)
        {
            string errors = "";
            int numErrors = 0;
            var i = 0;

            foreach (var file in files)
            {
                int numTries = 0;
                try
                {
                    while (true)
                    {
                        try
                        {
                            ImportFile(file);
                            break;
                        }
                        catch (Exception)
                        {
                            ++numTries;
                            if (numTries > 3) throw;

                        }
                    }
                }
                catch (Exception e)
                {
                    errors += $"{i}: {file}: {e.Message}\n";
                    ++numErrors;
                }

                ++i;
            }

            if (errors.Length > 0)
                throw new Exception($"Import failed for {numErrors}/{i} formats:\n" + errors);

        }

        void ImportFile(string filename)
        {
            using (var img = IO.LoadImage(filename))
            {
                Assert.AreNotEqual(GliFormat.UNDEFINED, img.OriginalFormat);
                Assert.AreNotEqual(Format.Unknown, img.Format);
                Assert.IsTrue(img.Size.X > 0);
                Assert.IsTrue(img.Size.Y > 0);
            }
        }
    }
}
