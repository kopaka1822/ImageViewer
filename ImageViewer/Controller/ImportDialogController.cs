using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms.VisualStyles;
using ImageFramework.DirectX;
using ImageFramework.ImageLoader;
using ImageFramework.Model;
using ImageViewer.Models;

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

        public async Task ImportImageAsync(string file)
        {
            try
            {

                var img = await IO.LoadImageTextureAsync(file, models.Progress);
                
                ImportTexture(img.Texture, file, img.OriginalFormat);
            }
            catch (Exception e)
            {
                models.Window.ShowErrorDialog(e.Message);
            }
        }

        private void ImportTexture(ITexture tex, string file, GliFormat imgOriginalFormat)
        {
            try
            {
                models.Images.AddImage(tex, file, imgOriginalFormat);
                tex = null; // images is now owner
            }
            catch (ImagesModel.MipmapMismatch e)
            {
                // silently generate mipmaps and import
                if (models.Images.NumMipmaps > 1 && tex.NumMipmaps == 1)
                {
                    var tmp = tex.GenerateMipmapLevels(models.Images.NumMipmaps);
                    ImportTexture(tmp, file, imgOriginalFormat);
                }
                else
                {
                    // don't just discard the mipmaps
                    models.Window.ShowErrorDialog(e.Message);
                }
            }
            catch (Exception e)
            {
                models.Window.ShowErrorDialog(e.Message);
            }
            finally
            {
                tex?.Dispose();
            }
        }
    }
}
