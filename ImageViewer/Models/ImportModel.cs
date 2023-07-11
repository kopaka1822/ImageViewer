using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using ImageFramework.Annotations;
using ImageFramework.DirectX;
using ImageFramework.ImageLoader;
using ImageFramework.Model;
using ImageFramework.Model.Export;
using ImageFramework.Utility;
using ImageViewer.Models.Settings;
using ImageViewer.ViewModels.Dialog;
using ImageViewer.Views.Dialog;

namespace ImageViewer.Models
{
    // manages imports of images, videos and config files
    public class ImportModel
    {
        private readonly ModelsEx models;
        private ImportMovieViewModel movieViewModel = null;
        private ImportNpyViewModel npyViewModel = null;

        // indicates if the user intervened in the import (and potentially cancelled it)
        public bool CancelledByUser { get; private set; } = false;

        public ImportModel(ModelsEx models)
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

        public async Task ImportFileAsync(string file, [CanBeNull] string alias = null)
        {
            CancelledByUser = false; // reset this flag

            var extension = file.Substring(file.LastIndexOf('.') + 1).ToLower();
            if (extension == "icfg")
            {
                try
                {
                    var cfg = ViewerConfig.LoadFromFile(file);
                    await cfg.ApplyToModels(models);

                    models.Settings.AddRecentFile(file);
                }
                catch (Exception e)
                {
                    models.Window.ShowErrorDialog(e, "Could not load config");
                }

            }
            /*else if (ExportDescription.Formats.FirstOrDefault(element => element.Extension == extension) != null)
            {
                await ImportImageAsync(file);
            }*/
            else if(extension == "npy")
            {
                await ImportNpyAsync(file, alias);
            }
            else if (FFMpeg.Formats().Contains(extension))
            {
                await ImportMovieAsync(file, alias);
            }
            else // try as image
            {
                await ImportImageAsync(file, alias);
            }
            //else models.Window.ShowErrorDialog($"Unknown file extension \"{extension}\"");

        }

        private async Task ImportImageAsync(string file, [CanBeNull] string alias = null)
        {
            try
            {
                var img = await IO.LoadImageTextureAsync(file, models.Progress);

                ImportTexture(img.Texture, true, file, img.OriginalFormat, alias);

                models.Settings.AddRecentFile(file);
            }
            catch (Exception e)
            {
                if (!models.Progress.LastTaskCancelledByUser)
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

        private async Task ImportMovieAsync(string file, [CanBeNull] string alias = null)
        {
            if (!FFMpeg.IsAvailable())
            {
                models.Window.ShowFFMpegDialog();
                return;
            }

            try
            {
                var meta = await FFMpeg.GetMovieMetadataAsync(file, models);

                var firstFrame = 0;
                var frameCount = meta.FrameCount;
                var frameSkip = 0;
                var fps = meta.FramesPerSecond;

                if (models.Images.NumImages != 0 && models.Images.NumLayers > meta.FrameCount)
                {
                    // frame count of imported video is too small and cannot be adjusted
                    models.Window.ShowErrorDialog($"Video frame count {meta.FrameCount} does not match layer count of opened image {models.Images.NumLayers}");
                    return;
                }

                // indicated if the user needs a window to adjust the frame count
                var openFrameSelection =
                    (models.Images.NumImages == 0 && frameCount > 1) || // show on import of first video
                    (models.Images.NumImages != 0 && models.Images.NumLayers != meta.FrameCount); // show when imported video frame count does not match

                if (openFrameSelection)
                {
                    int? requiredCount = null;
                    if (models.Images.NumImages != 0) requiredCount = models.Images.NumLayers;
                    if (movieViewModel == null) movieViewModel = new ImportMovieViewModel(models);
                    movieViewModel.Init(meta, requiredCount);
                    var dia = new ImportMovieDialog(movieViewModel);
                    if (models.Window.ShowDialog(dia) != true)
                    {
                        CancelledByUser = true;
                        return;
                    }
                    // obtain results
                    movieViewModel.GetFirstFrameAndFrameCount(out firstFrame, out frameCount);
                    frameSkip = movieViewModel.FrameSkip;
                    fps = movieViewModel.FPS;
                }

                // image should be fully compatible
                var tex = await FFMpeg.ImportMovieAsync(meta, firstFrame, frameSkip, frameCount, models);

                if (models.Images.NumImages == 0)
                {
                    // use movie fps as reference
                    models.Settings.MovieFps = fps;
                }

                ImportTexture(tex, false, file, GliFormat.RGB8_SRGB, alias);

                if (models.Settings.MovieFps != fps)
                    models.Window.ShowInfoDialog($"The FPS count of the imported video ({meta.FramesPerSecond}) does not match the existing FPS count ({models.Settings.MovieFps}). The previous FPS count will be kept.");

                models.Settings.AddRecentFile(file);
            }
            catch (Exception e)
            {
                if (!models.Progress.LastTaskCancelledByUser)
                    models.Window.ShowErrorDialog(e);
            }
        }

        private async Task ImportNpyAsync(string file, [CanBeNull] string alias = null)
        {
            try
            {
                var shape = IO.NpyGetShape(file);

                if (npyViewModel == null) npyViewModel = new ImportNpyViewModel(models);
                var status = npyViewModel.Init(file, shape);
                if (!status.IsCompatible)
                    throw new Exception("Numpy Shape is incompatible. Found " + npyViewModel.Shape);

                if (status.IsConfigurable)
                {
                    // let the user configure
                    var dia = new ImportNpyDialog(npyViewModel);
                    if (models.Window.ShowDialog(dia) != true)
                    {
                        CancelledByUser = true;
                        return;
                    }
                }
                
                npyViewModel.ApplySettings();
            }
            catch (Exception e)
            {
                if (!models.Progress.LastTaskCancelledByUser)
                    models.Window.ShowErrorDialog(e);
            }

            // perform import via normal interface (the dialog is just for settings)
            await ImportImageAsync(file, alias);
        }
    }
}
