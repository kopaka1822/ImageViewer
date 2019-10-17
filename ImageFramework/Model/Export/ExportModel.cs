using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using ImageFramework.Annotations;
using ImageFramework.DirectX;
using ImageFramework.ImageLoader;
using ImageFramework.Model.Shader;

namespace ImageFramework.Model.Export
{
    public class ExportModel : INotifyPropertyChanged, IDisposable
    {
        public enum LdrMode
        {
            Undefined,
            Srgb,
            UNorm,
            SNorm
        }

        public IReadOnlyList<ExportFormatModel> Formats { get; }

        internal readonly ConvertFormatShader convert = new ConvertFormatShader();

        public ExportModel()
        {
            var formats = new List<ExportFormatModel>();
            formats.Add(new ExportFormatModel("png"));
            formats.Add(new ExportFormatModel("jpg"));
            formats.Add(new ExportFormatModel("bmp"));
            formats.Add(new ExportFormatModel("hdr"));
            formats.Add(new ExportFormatModel("pfm"));
            formats.Add(new ExportFormatModel("dds"));
            formats.Add(new ExportFormatModel("ktx"));
            Formats = formats;
        }

        private LdrMode ldrExportMode = LdrMode.Srgb;

        /// <summary>
        /// format of the pixel data for ldr (png, jpg, bmp) exports
        /// </summary>
        public LdrMode LdrExportMode
        {
            get => ldrExportMode;
            set
            {
                if(value == ldrExportMode) return;
                ldrExportMode = value;
                OnPropertyChanged(nameof(LdrExportMode));
            }
        }

        private int mipmap = -1;

        /// <summary>
        /// mipmap to export. -1 means all mipmaps
        /// </summary>
        public int Mipmap
        {
            get => mipmap;
            set
            {
                Debug.Assert(value >= -1);
                if(value == mipmap) return;
                mipmap = value;
                OnPropertyChanged(nameof(Mipmap));
            }
        }

        private int layer = -1;

        /// <summary>
        /// layer to export. -1 means all layers
        /// </summary>
        public int Layer
        {
            get => layer;
            set
            {
                Debug.Assert(layer >= -1);
                if(value == layer) return;
                layer = value;
                OnPropertyChanged(nameof(Layer));
            }
        }

        private int quality = 100;

        /// <summary>
        /// image quality for compressed formats. Currently only used with jpg.
        /// Range [1, 100] => [QualityMin, QualityMax]
        /// </summary>
        public int Quality
        {
            get => quality;
            set
            {
                Debug.Assert(quality <= QualityMax);
                Debug.Assert(quality >= QualityMin);
                if (value == quality) return;
                quality = value;
                OnPropertyChanged(nameof(Quality));
            }
        }

        public static int QualityMin = 1;
        public static int QualityMax = 100;

        private bool isExporting = false;

        /// <summary>
        /// indicates if the exporting dialog is open
        /// </summary>
        public bool IsExporting
        {
            get => isExporting;
            set
            {
                if(value == isExporting) return;
                isExporting = value;
                OnPropertyChanged(nameof(IsExporting));
            }
        }

        private bool useCropping = false;

        public bool UseCropping
        {
            get => useCropping;
            set
            {
                if(value == useCropping) return;
                useCropping = value;
                OnPropertyChanged(nameof(UseCropping));
            }
        }

        private int cropStartX = 0;
        public int CropStartX
        {
            get => cropStartX;
            set
            {
                //Debug.Assert(UseCropping);
                if (value == cropStartX) return;
                cropStartX = value;
                OnPropertyChanged(nameof(CropStartX));
            }
        }

        private int cropStartY = 0;

        public int CropStartY
        {
            get => cropStartY;
            set
            {
                //Debug.Assert(UseCropping);
                if (value == cropStartY) return;
                cropStartY = value;
                OnPropertyChanged(nameof(CropStartY));
            }
        }

        private int cropEndX = 0;

        public int CropEndX
        {
            get => cropEndX;
            set
            {
                //Debug.Assert(UseCropping);
                if (value == cropEndX) return;
                cropEndX = value;
                OnPropertyChanged(nameof(CropEndX));
            }
        }

        private int cropEndY = 0;

        public int CropEndY
        {
            get => cropEndY;
            set
            {
                //Debug.Assert(UseCropping);
                if (value == cropEndY) return;
                cropEndY = value;
                OnPropertyChanged(nameof(CropEndY));
            }
        }

        public void Export(TextureArray2D image, ExportDescription desc)
        {
            Debug.Assert(image != null);

            // verify mipmaps etc.
            if(Mipmap >= image.NumMipmaps)
                throw new Exception("export mipmap out of range");
            if(Layer >= image.NumLayers)
                throw new Exception("export layer out of range");

            // test cropping dimensions
            bool croppingActive = false;
            if (UseCropping)
            {
                var mipIdx = Math.Max(Mipmap, 0);
               
                // general boundaries
                if(CropStartX < 0 || CropStartX >= image.GetWidth(mipIdx))
                    throw new Exception("export crop start x out of range: " + CropStartX);
                if(CropStartY < 0 || CropStartY >= image.GetHeight(mipIdx))
                    throw new Exception("export crop start y out of range: " + CropStartY);
                if(CropEndX < 0 || CropEndX >= image.GetWidth(mipIdx))
                    throw new Exception("export crop end x out of range: " + CropEndX);
                if(CropEndY < 0 || CropEndY >= image.GetHeight(mipIdx))
                    throw new Exception("export crop end y out of range: " + CropEndY);

                // end >= max
                if(CropStartX > CropEndX)
                    throw new Exception("export crop start x must be smaller or equal to crop end x");
                if (CropStartY > CropEndY)
                    throw new Exception("export crop start y must be smaller or equal to crop end y");

                // set cropping to active if the image was actually cropped
                if (CropStartX != 0 || CropStartY != 0) croppingActive = true;
                if (CropEndX != image.GetWidth(mipIdx) - 1) croppingActive = true;
                if (CropEndY != image.GetHeight(mipIdx) - 1) croppingActive = true;
            }

            // image is ready for export!
            var stagingFormat = desc.StagingFormat;
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if (image.Format != stagingFormat.DxgiFormat || croppingActive || desc.Multiplier != 1.0f)
            {
                using (var tmpTex = convert.Convert(image, stagingFormat.DxgiFormat, Mipmap, Layer,
                    desc.Multiplier, UseCropping, CropStartX, CropStartY, CropEndX - CropStartX + 1, CropEndY - CropStartY + 1,
                    desc.FileFormat.GetAlignmentX(), desc.FileFormat.GetAlignmentY()))
                {
                    // the final texture only has the relevant layers and mipmaps
                    ExportTexture(tmpTex, desc, -1, -1);
                }
            }
            else
            {
                ExportTexture(image, desc, Mipmap, Layer);
            }
        }

        private void ExportTexture(TextureArray2D texture, ExportDescription desc, int mipmap, int layer)
        {
            Debug.Assert(desc.StagingFormat.DxgiFormat == texture.Format);

            int firstMipmap = Math.Max(mipmap, 0);
            int nMipmaps = mipmap == -1 ? texture.NumMipmaps : 1;
            int firstLayer = Math.Max(layer, 0);
            int nLayer = layer == -1 ? texture.NumLayers : 1;

            using (var img = IO.CreateImage(desc.StagingFormat, texture.GetWidth(firstMipmap),
                texture.GetHeight(firstMipmap), nLayer, nMipmaps))
            {
                // fill with data
                for (int curLayer = 0; curLayer < nLayer; ++curLayer)
                {
                    for (int curMipmap = 0; curMipmap < nMipmaps; ++curMipmap)
                    {
                        var mip = img.Layers[curLayer].Mipmaps[curMipmap];
                        // transfer image data
                        texture.CopyPixels(firstLayer + curLayer, firstMipmap + curMipmap, mip.Bytes, mip.Size);
                    }
                }

                IO.SaveImage(img, desc.Filename, desc.Extension, desc.FileFormat, quality);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void Dispose()
        {
            convert?.Dispose();
        }
    }
}
