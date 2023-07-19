using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageFramework.ImageLoader;
using ImageFramework.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharpDX.DXGI;

namespace FrameworkTests.ImageLoader
{
    [TestClass]
    public class KtxSamples
    {
        public static string ImportDir = TestData.Directory + "ktx-software\\testimages\\";

        public static string ImportBadDir = TestData.Directory + "ktx-software\\badktx2\\";

        [TestMethod]
        public void ImportTestImagesKtx()
        {
            // get all files in the directory
            var files = System.IO.Directory.GetFiles(ImportDir, "*.ktx", System.IO.SearchOption.TopDirectoryOnly);
            // filter files so they only end with .ktx (not ktx2)
            files = files.Where(f => f.EndsWith(".ktx")).ToArray();
            TryImportAllFiles(files);
        }

        [TestMethod]
        public void ImportTestImagesKtx2()
        {
            // get all files in the directory
            var files = System.IO.Directory.GetFiles(ImportDir, "*.ktx2", System.IO.SearchOption.TopDirectoryOnly);
            TryImportAllFiles(files);
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
