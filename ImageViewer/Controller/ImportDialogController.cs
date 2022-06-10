using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms.VisualStyles;
using ImageFramework.Annotations;
using ImageFramework.DirectX;
using ImageFramework.ImageLoader;
using ImageFramework.Model;
using ImageViewer.Models;
using ImageViewer.Models.Settings;

namespace ImageViewer.Controller
{
    public class ImportDialogController
    {
        private readonly ModelsEx models;

        public ImportDialogController(ModelsEx models)
        {
            this.models = models;
        }

        /// <summary>
        /// opens the file dialog for images
        /// </summary>
        /// <returns>string with filenames or null if aborted</returns>
        public string[] ShowImportImageDialog()
        {
            var ofd = new Microsoft.Win32.OpenFileDialog
            {
                Multiselect = true,
                InitialDirectory = models.Settings.ImagePath
            };

            if (ofd.ShowDialog(models.Window.TopmostWindow) != true) return null;

            // set new image path
            models.Settings.ImagePath = System.IO.Path.GetDirectoryName(ofd.FileName);

            return ofd.FileNames;
        }

        public static string ShowSingleFileImportImageDialog(ModelsEx models)
        {
            var ofd = new Microsoft.Win32.OpenFileDialog
            {
                Multiselect = false,
                InitialDirectory = models.Settings.ImagePath
            };

            if (ofd.ShowDialog(models.Window.TopmostWindow) != true) return null;

            // set new image path
            models.Settings.ImagePath = System.IO.Path.GetDirectoryName(ofd.FileName);
            Debug.Assert(ofd.FileNames.Length == 1);
            if(ofd.FileNames.Length == 0) return null;

            return ofd.FileNames[0];
        }

        public async Task ImportImageAsync(string file, [CanBeNull] string alias = null)
        {
            try
            {
                var img = await IO.LoadImageTextureAsync(file, models.Progress);
                
                ImportTexture(img.Texture, true, file, img.OriginalFormat, alias);
            }
            catch (Exception e)
            {
                if(!models.Progress.LastTaskCancelledByUser)
                    models.Window.ShowErrorDialog(e);
            }
        }

        public void ImportTexture(ITexture tex, string alias, GliFormat format)
        {
            ImportTexture(tex, false, alias, format, alias);
        }

        private void ImportTexture(ITexture tex, bool isFile, string file, GliFormat imgOriginalFormat, string alias)
        {
            try
            {
                models.Images.AddImage(tex, isFile, file, imgOriginalFormat, alias);
                tex = null; // images is now owner
            }
            catch (ImagesModel.MipmapMismatch e)
            {
                // silently generate mipmaps and import
                if (models.Images.NumMipmaps > 1 && tex.NumMipmaps == 1)
                {
                    var tmp = tex.CloneWithMipmaps(models.Images.NumMipmaps);
                    models.Scaling.WriteMipmaps(tmp);
                    ImportTexture(tmp, isFile, file, imgOriginalFormat, alias);
                }
                else
                {
                    // don't just discard the mipmaps
                    models.Window.ShowErrorDialog(e);
                }
            }
            catch (Exception e)
            {
                models.Window.ShowErrorDialog(e);
            }
            finally
            {
                tex?.Dispose();
            }
        }
    }
}
