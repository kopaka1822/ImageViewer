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
using TextureViewer.Utility;
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
            var equationId = models.Equations.GetFirstVisible();
            var firstImageId = models.Equations.Get(equationId).ColorFormula.FirstImageId;
            var proposedFilename = firstImageId < models.Images.NumImages ?
                System.IO.Path.GetFileNameWithoutExtension(models.Images.GetFilename(firstImageId)) : "";

            // open save file dialog
            var sfd = new SaveFileDialog
            {
                Filter = "PNG (*.png)|*.png|BMP (*.bmp)|*.bmp|JPEG (*.jpg)|*.jpg|HDR (*.hdr)|*.hdr|Portable float map (*.pfm)|*.pfm|Khronos Texture (*.ktx)|*.ktx",
                InitialDirectory = Properties.Settings.Default.ExportPath,
                FileName = proposedFilename
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
            else if (sfd.FileName.EndsWith(".jpg"))
                format = ExportModel.FileFormat.Jpg;
            else if (sfd.FileName.EndsWith(".ktx"))
                format = ExportModel.FileFormat.Ktx;

            var texFormat = new ImageLoader.ImageFormat(PixelFormat.Rgb, PixelType.UnsignedByte, true);
            switch (format)
            {
                case ExportModel.FileFormat.Png:
                case ExportModel.FileFormat.Bmp:
                case ExportModel.FileFormat.Jpg:
                    if (models.Images.IsAlpha && format == ExportModel.FileFormat.Png)
                        texFormat.ExternalFormat = PixelFormat.Rgba;
                    if (models.Images.IsGrayscale)
                        texFormat.ExternalFormat = PixelFormat.Red;
                    break;

                case ExportModel.FileFormat.Hdr:
                case ExportModel.FileFormat.Pfm:
                    texFormat = new ImageLoader.ImageFormat(PixelFormat.Rgb, PixelType.Float, false);
                    if (models.Images.IsGrayscale)
                        texFormat.ExternalFormat = PixelFormat.Red;
                    break;
                case ExportModel.FileFormat.Ktx:
                    texFormat = new ImageLoader.ImageFormat(GliFormat.RGB8_SRGB_PACK8);
                    break;
            }

            models.Export.IsExporting = true;
            // open export dialog
            var dia = new ExportDialog(models, sfd.FileName, texFormat, format);
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
                    var texture = models.FinalImages.Get(equationId).Texture;
                    if (texture == null)
                        throw new Exception("texture is not computed");

                    if (info.Layer == -1 || info.Mipmap == -1)
                        SaveMultipleLevel(info, texture);
                    else
                        SaveSingleLevel(info, texture);
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

        private void SaveSingleLevel(ExportModel info, TextureArray2D texture)
        {
            var width = info.GetCropWidth();
            var height = info.GetCropHeight();
            Debug.Assert(width > 0);
            Debug.Assert(height > 0);

            var data = texture.GetData(info.Layer, info.Mipmap, info.TexFormat.Format.Format, info.TexFormat.Format.Type, info.TexFormat.Format.IsSrgb,
                info.UseCropping, info.CropStartX, info.CropStartY, ref width, ref height,
                models.GlData.ExportShader);

            if (data == null)
                throw new Exception("error retrieving image from gpu");

            var numComponents = 0;
            if(info.FileType != ExportModel.FileFormat.Ktx)
                numComponents = TextureArray2D.GetPixelFormatCount(info.TexFormat.Format.Format);

            switch (info.FileType)
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
                case ExportModel.FileFormat.Jpg:
                    ImageLoader.SaveJpg(info.Filename, width, height, numComponents, data, info.Quality);
                    break;
                case ExportModel.FileFormat.Ktx:
                    Debug.Assert(info.TexFormat.Format.HasGliFormat);
                    ImageLoader.CreateStorage(info.TexFormat.Format.GliFormat, width, height, 1, 1);
                    ImageLoader.StoreLevel(0, 0, data, (UInt64)data.Length);
                    ImageLoader.SaveKtx(info.Filename);
                    break;
            }
        }

        private void SaveMultipleLevel(ExportModel info, TextureArray2D texture)
        {
            Debug.Assert(info.FileType == ExportModel.FileFormat.Ktx);
            Debug.Assert(info.TexFormat.Format.HasGliFormat);

            var numLayer = info.Layer == -1 ? models.Images.NumLayers : 1;
            var numLevels = info.Mipmap == -1 ? models.Images.NumMipmaps : 1;
            var supportCropping = numLevels == 1;

            var width = info.GetCropWidth();
            var height = info.GetCropHeight();
            Debug.Assert(width > 0);
            Debug.Assert(height > 0);
            if(!supportCropping)
            {
                width = models.Images.Width;
                height = models.Images.Height;
            }

            // allocate
            ImageLoader.CreateStorage(info.TexFormat.Format.GliFormat, width, height, numLayer, numLevels);
            
            // store data
            for(var layerIdx = 0; layerIdx < numLayer; ++layerIdx)
            {
                for(var levelIdx = 0; levelIdx < numLevels; ++levelIdx)
                {
                    var data = texture.GetData(
                        numLayer == 1 ? info.Layer : layerIdx,
                        numLevels == 1 ? info.Mipmap : levelIdx,
                        info.TexFormat.Format.Format, info.TexFormat.Format.Type, info.TexFormat.Format.IsSrgb,
                        info.UseCropping && supportCropping, info.CropStartX, info.CropStartY, ref width, ref height,
                        models.GlData.ExportShader);

                    ImageLoader.StoreLevel(layerIdx, levelIdx, data, (UInt64)data.Length);
                }
            }

            // save texture
            ImageLoader.SaveKtx(info.Filename);
        }

        public event EventHandler CanExecuteChanged
        {
            add { }
            remove { }
        }
    }
}
