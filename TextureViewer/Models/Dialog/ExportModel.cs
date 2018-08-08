using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL4;
using TextureViewer.Annotations;

namespace TextureViewer.Models.Dialog
{
    public class ExportModel : INotifyPropertyChanged
    {
        public enum FileFormat
        {
            Png,
            Bmp,
            Hdr,
            Pfm
        }

        private ImagesModel imagesModel;
        private readonly DisplayModel displayModel;

        // initializes the export model for a new export
        public void Init(string filename, PixelFormat pixelFromat, FileFormat format)
        {
            Filename = filename;
            PixelFormat = pixelFromat;
            Format = format;

            var supportedFormats = new List<PixelFormat>();
            switch (format)
            {
                case FileFormat.Png:
                    supportedFormats.Add(PixelFormat.Red);
                    supportedFormats.Add(PixelFormat.Green);
                    supportedFormats.Add(PixelFormat.Blue);
                    supportedFormats.Add(PixelFormat.Rg);
                    supportedFormats.Add(PixelFormat.Rgb);
                    supportedFormats.Add(PixelFormat.Rgba);
                    break;
                case FileFormat.Bmp:
                    supportedFormats.Add(PixelFormat.Red);
                    supportedFormats.Add(PixelFormat.Green);
                    supportedFormats.Add(PixelFormat.Blue);
                    supportedFormats.Add(PixelFormat.Rg);
                    supportedFormats.Add(PixelFormat.Rgb);
                    break;
                case FileFormat.Hdr:
                case FileFormat.Pfm:
                    supportedFormats.Add(PixelFormat.Red);
                    supportedFormats.Add(PixelFormat.Green);
                    supportedFormats.Add(PixelFormat.Blue);
                    supportedFormats.Add(PixelFormat.Rgb);
                    break;
            }

            Debug.Assert(supportedFormats.Contains(pixelFromat));

            SupportedFormats = supportedFormats;

            Layer = displayModel.ActiveLayer;
            Mipmap = displayModel.ActiveMipmap;
        }

        public ExportModel(ImagesModel imagesModel, DisplayModel displayModel)
        {
            this.imagesModel = imagesModel;
            this.displayModel = displayModel;
        }

        public string Filename { get; private set; }

        public FileFormat Format;

        public int Layer { get; set; }

        public int Mipmap { get; set; }

        public PixelFormat PixelFormat { get; set; }

        public IReadOnlyList<PixelFormat> SupportedFormats { get; private set; }

        public PixelType PixelType =>
            Format == FileFormat.Hdr || 
            Format == FileFormat.Pfm ? 
                PixelType.Float : PixelType.UnsignedByte;

        private bool useCropping = false;

        public bool UseCropping
        {
            get => useCropping;
            set
            {
                if (value == useCropping) return;
                useCropping = value;
                OnPropertyChanged(nameof(UseCropping));
            }
        }

        private int cropStartX = 0;
        private int cropStartY = 0;
        private int cropEndX;
        private int cropEndY;

        public int CropStartX
        {
            get => cropStartX;
            set
            {
                if (value == cropStartX) return;
                cropStartX = value;
                OnPropertyChanged(nameof(CropStartX));
            }
        }
        public int CropStartY
        {
            get => cropStartY;
            set
            {
                if (value == cropStartY) return;
                cropStartY = value;
                OnPropertyChanged(nameof(CropStartY));
            }
        }
        public int CropEndX
        {
            get => cropEndX;
            set
            {
                if (value == cropEndX) return;
                cropEndX = value;
                OnPropertyChanged(nameof(CropEndX));
            }
        }
        public int CropEndY
        {
            get => cropEndY;
            set
            {
                if (value == cropEndY) return;
                cropEndY = value;
                OnPropertyChanged(nameof(CropEndY));
            }
        }


        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
