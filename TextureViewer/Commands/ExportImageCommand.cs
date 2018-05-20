using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Win32;
using OpenTK.Graphics.OpenGL4;
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

            // open export dialog
            var dia = new ExportDialog(models, sfd.FileName, pixelFormat, format);
            if (dia.ShowDialog() != true) return;


        }

        public event EventHandler CanExecuteChanged
        {
            add { }
            remove { }
        }
    }
}
