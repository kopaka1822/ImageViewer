using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
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
            Pfm,
            Jpg
        }

        private ImagesModel imagesModel;
        private readonly DisplayModel displayModel;

        // initializes the export model for a new export
        public void Init(string filename, PixelFormat pixelFormat, FileFormat format)
        {
            Filename = filename;
            Format = format;
            HasQuality = false;

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
                case FileFormat.Jpg:
                    supportedFormats.Add(PixelFormat.Red);
                    supportedFormats.Add(PixelFormat.Green);
                    supportedFormats.Add(PixelFormat.Blue);
                    //supportedFormats.Add(PixelFormat.Rg);
                    supportedFormats.Add(PixelFormat.Rgb);
                    HasQuality = true;
                    MinQuality = 1;
                    MaxQuality = 100;
                    Quality = Properties.Settings.Default.JpgQuality;
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

            PixelFormat = pixelFormat;
            if (!supportedFormats.Contains(pixelFormat))
            {
                PixelFormat = supportedFormats.Last();
            }

            SupportedFormats = supportedFormats;

            Layer = displayModel.ActiveLayer;
            Mipmap = displayModel.ActiveMipmap;
        }

        public ExportModel(ImagesModel imagesModel, DisplayModel displayModel)
        {
            this.imagesModel = imagesModel;
            this.displayModel = displayModel;

            imagesModel.PropertyChanged += ImagesModelOnPropertyChanged;
        }

        private void ImagesModelOnPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            switch (args.PropertyName)
            {
                case nameof(ImagesModel.NumImages):
                    if (imagesModel.PrevNumImages == 0 || imagesModel.NumImages == 0)
                    {
                        Mipmap = 0;
                        OnPropertyChanged(nameof(CropMaxX));
                        OnPropertyChanged(nameof(CropMaxY));
                    }

                    if (imagesModel.PrevNumImages == 0)
                    {
                        // set default width, height
                        CropStartX = 0;
                        CropStartY = 0;
                        CropEndX = CropMaxX;
                        CropEndY = CropMaxY;
                    }

                    if (imagesModel.NumImages == 0)
                    {
                        // reset everything to 0
                        CropStartX = 0;
                        CropStartY = 0;
                        CropEndX = 0;
                        CropEndY = 0;
                        Layer = 0;
                    }
                    
                    break;
            }
        }

        private bool isExporting = false;
        // this indicates if the export dialog is open
        public bool IsExporting
        {
            get => isExporting;
            set
            {
                if (value == isExporting) return;
                isExporting = value;
                OnPropertyChanged(nameof(IsExporting));
            }
        }

        

        public string Filename { get; private set; }

        public FileFormat Format;

        private int layer = 0;
        public int Layer
        {
            get => layer;
            set
            {
                if (layer == value) return;
                layer = value;
                OnPropertyChanged(nameof(Layer));
            }
        }

        private int mipmap = 0;
        public int Mipmap
        {
            get => mipmap;
            set
            {
                if (mipmap == value) return;
                mipmap = value;
                OnPropertyChanged(nameof(Mipmap));
                // dimensions have changed
                OnPropertyChanged(nameof(CropMaxX));
                OnPropertyChanged(nameof(CropMaxY));
                // adjust other crop values
                if (CropStartX > CropMaxX) CropStartX = CropMaxX;
                if (CropStartY > CropMaxY) CropStartY = CropMaxY;
                if (CropEndX > CropMaxX) CropEndX = CropMaxX;
                if (CropEndY > CropMaxY) CropEndY = CropMaxY;
            }
        }

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

        public bool DisplayCropping => UseCropping && displayModel.ShowCropRectangle;

        public int CropMinX => 0;
        public int CropMinY => 0;
        public int CropMaxX => imagesModel.NumImages != 0 ? Math.Max(imagesModel.GetWidth(Mipmap) - 1, 0) : 0;
        public int CropMaxY => imagesModel.NumImages != 0 ? Math.Max(imagesModel.GetHeight(Mipmap) - 1, 0) : 0;

        private int cropStartX = 0;
        private int cropStartY = 0;
        private int cropEndX;
        private int cropEndY;

        public int CropStartX
        {
            get => cropStartX;
            set
            {
                var val = Math.Min(Math.Max(value, CropMinX), CropMaxX);
                if (val == cropStartX) return;
                cropStartX = val;
                OnPropertyChanged(nameof(CropStartX));
            }
        }
        public int CropStartY
        {
            get => cropStartY;
            set
            {
                var val = Math.Min(Math.Max(value, CropMinY), CropMaxY);
                if (val == cropStartY) return;
                cropStartY = val;
                OnPropertyChanged(nameof(CropStartY));
            }
        }
        public int CropEndX
        {
            get => cropEndX;
            set
            {
                var val = Math.Min(Math.Max(value, CropMinX), CropMaxX);
                if (val == cropEndX) return;
                cropEndX = val;
                OnPropertyChanged(nameof(CropEndX));
            }
        }
        public int CropEndY
        {
            get => cropEndY;
            set
            {
                var val = Math.Min(Math.Max(value, CropMinY), CropMaxY);
                if (val == cropEndY) return;
                cropEndY = val;
                OnPropertyChanged(nameof(CropEndY));
            }
        }

        private int quality = 0;
        public int Quality
        {
            get => quality;
            set
            {
                var val = Math.Min(Math.Max(value, MinQuality), MaxQuality);
                if (val == quality) return;
                quality = val;
                OnPropertyChanged(nameof(Quality));
                if (Format == FileFormat.Jpg)
                    Properties.Settings.Default.JpgQuality = quality;
            }
        }

        public int MinQuality { get; private set; }
        public int MaxQuality { get; private set; }
        // indicates if this format has a quality attribute
        public bool HasQuality { get; private set; }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public int GetCropWidth()
        {
            return CropEndX - CropStartX + 1;
        }

        public int GetCropHeight()
        {
            return CropEndY - CropStartY + 1;
        }

        public float GetCropStartXPercent()
        {
            return (float)CropStartX / (CropMaxX + 1);
        }

        public float GetCropEndXPercent()
        {
            return (float)(CropEndX + 1) / (CropMaxX + 1);
        }

        public float GetCropStartYPercent()
        {
            return (float)CropStartY / (CropMaxY + 1);
        }

        public float GetCropEndYPercent()
        {
            return (float)(CropEndY + 1) / (CropMaxY + 1);
        }
    }
}
