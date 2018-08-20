using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Win32;
using OpenTK.Graphics.OpenGL4;
using TextureViewer.glhelper;
using TextureViewer.Models.Dialog;
using TextureViewer.Views;

namespace TextureViewer.Commands
{
    public class ExportImageCommand : ICommand
    {
        private readonly Models.Models models;

        public ExportImageCommand(Models.Models models)
        {
            this.models = models;
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            if(models.Images.NumImages == 0) return;

            // make sure only one image is visible
            if (models.Equations.NumVisible != 1)
            {
                App.ShowInfoDialog(models.App.Window, "Exactly one image equation should be visible when exporting.");
                return;
            }

            // get active final image

            // open save file dialog
            var sfd = new SaveFileDialog
            {
                Filter = "PNG (*.png)|*.png|BMP (*.bmp)|*.bmp|HDR (*.hdr)|*.hdr|Portable float map (*.pfm)|*.pfm",
                InitialDirectory = Properties.Settings.Default.ExportPath
            };

            if (sfd.ShowDialog() != true)
                return;

            Properties.Settings.Default.ExportPath = System.IO.Path.GetDirectoryName(sfd.FileName);

            // obtain file format
            var format = ExportModel.FileFormat.Png;
            if (sfd.FileName.EndsWith(".bmp"))
                format = ExportModel.FileFormat.Bmp;
            else if (sfd.FileName.EndsWith(".hdr"))
                format = ExportModel.FileFormat.Hdr;
            else if (sfd.FileName.EndsWith(".pfm"))
                format = ExportModel.FileFormat.Pfm;
            
            var pixelFormat = PixelFormat.Rgb;
            if (models.Images.IsAlpha)
                pixelFormat = PixelFormat.Rgba;
            if (models.Images.IsGrayscale)
                pixelFormat = PixelFormat.Red;

            models.Export.IsExporting = true;
            // open export dialog
            var dia = new ExportDialog(models, sfd.FileName, pixelFormat, format);
            dia.Owner = models.App.Window;
            dia.Closed += (sender, args) =>
            {
                models.Export.IsExporting = false;
                if (!dia.ExportResult) return;

                var info = models.Export;

                models.GlContext.Enable();
                try
                {
                    // obtain data from gpu
                    var visibleId = models.Equations.GetFirstVisible();

                    var texture = models.FinalImages.Get(visibleId).Texture;
                    if (texture == null)
                        throw new Exception("texture is not computed");

                    var width = info.GetCropWidth();
                    var height = info.GetCropHeight();
                    Debug.Assert(width > 0);
                    Debug.Assert(height > 0);

                    var convertToSrgb = format == ExportModel.FileFormat.Png || format == ExportModel.FileFormat.Bmp;
                    var data = texture.GetData(info.Layer, info.Mipmap, info.PixelFormat, info.PixelType, convertToSrgb,
                        info.UseCropping, info.CropStartX, info.CropStartY, ref width, ref height,
                        models.GlData.ExportShader);

                    if (data == null)
                        throw new Exception("error retrieving image from gpu");

                    var numComponents = TextureArray2D.GetPixelFormatCount(info.PixelFormat);

                    switch (format)
                    {
                        case ExportModel.FileFormat.Png:
                            ImageLoader.SavePng(info.Filename, width, height, numComponents, data);
                            break;
                        case ExportModel.FileFormat.Bmp:
                            ImageLoader.SaveBmp(info.Filename, width, height, numComponents, data);
                            break;
                        case ExportModel.FileFormat.Hdr:
                            ImageLoader.SaveHdr(info.Filename, width, height, numComponents, data);
                            break;
                        case ExportModel.FileFormat.Pfm:
                            ImageLoader.SavePfm(info.Filename, width, height, numComponents, data);
                            break;
                    }
                }
                catch (Exception e)
                {
                    App.ShowErrorDialog(models.App.Window, e.Message);
                }
                finally
                {
                    models.GlContext.Disable();
                }
            };

            dia.Show();
        }

        public event EventHandler CanExecuteChanged
        {
            add { }
            remove { }
        }
    }
}
