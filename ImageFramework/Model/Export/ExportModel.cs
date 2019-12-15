using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Threading;
using System.Threading.Tasks;
using ImageFramework.Annotations;
using ImageFramework.DirectX;
using ImageFramework.ImageLoader;
using ImageFramework.Model.Shader;
using ImageFramework.Utility;

namespace ImageFramework.Model.Export
{
    public class ExportModel : INotifyPropertyChanged
    {
        public IReadOnlyList<ExportFormatModel> Formats { get; }

        internal readonly ConvertFormatShader convert;
        private readonly ProgressModel progress;

        internal ExportModel(SharedModel shared, ProgressModel progress)
        {
            this.progress = progress;
            convert = shared.Convert;

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

        private int cropStartZ = 0;

        public int CropStartZ
        {
            get => cropStartZ;
            set
            {
                if (value == cropStartZ) return;
                cropStartZ = value;
                OnPropertyChanged(nameof(CropStartZ));
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

        private int cropEndZ = 0;

        public int CropEndZ
        {
            get => cropEndZ;
            set
            {
                if (value == cropEndZ) return;
                cropEndZ = value;
                OnPropertyChanged(nameof(CropEndZ));
            }
        }

        public void Export(ITexture image, ExportDescription desc)
        {
            ExportAsync(image, desc);
            progress.WaitForTask();
            if(!String.IsNullOrEmpty(progress.LastError))
                throw new Exception(progress.LastError);
        }

        // does export asynchronously => the task will be passed to the ProgressModel
        public void ExportAsync(ITexture image, ExportDescription desc)
        {
            var cts = new CancellationTokenSource();
            progress.AddTask(ExportAsync(image, desc, cts.Token), cts);
        }

        private Task ExportAsync(ITexture image, ExportDescription desc, CancellationToken ct)
        {
            Debug.Assert(image != null);

            // verify mipmaps etc.
            if (Mipmap >= image.NumMipmaps)
                throw new Exception("export mipmap out of range");
            if(Layer >= image.NumLayers)
                throw new Exception("export layer out of range");

            // test cropping dimensions
            bool croppingActive = false;
            if (UseCropping)
            {
                var mipIdx = Math.Max(Mipmap, 0);
               
                // general boundaries
                var mipDim = image.Size.GetMip(mipIdx);

                if(CropStartX < 0 || CropStartX >= mipDim.Width)
                    throw new Exception("export crop start x out of range: " + CropStartX);
                if(CropStartY < 0 || CropStartY >= mipDim.Height)
                    throw new Exception("export crop start y out of range: " + CropStartY);
                if(CropStartZ < 0 || CropStartZ >= mipDim.Depth)
                    throw new Exception("export crop start z out of range: " + CropStartZ);

                if (CropEndX < 0 || CropEndX >= mipDim.Width)
                    throw new Exception("export crop end x out of range: " + CropEndX);
                if(CropEndY < 0 || CropEndY >= mipDim.Height)
                    throw new Exception("export crop end y out of range: " + CropEndY);
                if(CropEndZ < 0 || CropEndZ >= mipDim.Depth)
                    throw new Exception("export crop end z out of range: " + CropEndZ);

                // end >= max
                if(CropStartX > CropEndX)
                    throw new Exception("export crop start x must be smaller or equal to crop end x");
                if (CropStartY > CropEndY)
                    throw new Exception("export crop start y must be smaller or equal to crop end y");
                if(CropStartZ > CropEndZ)
                    throw new Exception("export crop start z must be smaller or equal to crop end z");

                // set cropping to active if the image was actually cropped
                if (CropStartX != 0 || CropStartY != 0 || CropStartZ != 0) croppingActive = true;
                if (CropEndX != mipDim.Width - 1) croppingActive = true;
                if (CropEndY != mipDim.Height - 1) croppingActive = true;
                if (CropEndZ != mipDim.Depth - 1) croppingActive = true;
            }

            bool alignmentActive = false;
            if (desc.FileFormat.IsCompressed())
            {
                var mipIdx = Math.Max(Mipmap, 0);
                if (image.Size.GetMip(mipIdx).Width % desc.FileFormat.GetAlignmentX() != 0)
                    alignmentActive = true;
                if (image.Size.GetMip(mipIdx).Height % desc.FileFormat.GetAlignmentY() != 0)
                    alignmentActive = true;
            }

            // image is ready for export!
            var stagingFormat = desc.StagingFormat;
            
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if (image.Format == stagingFormat.DxgiFormat && !croppingActive && desc.Multiplier == 1.0f &&
                !alignmentActive)
                return ExportTexture(image, desc, Mipmap, Layer, ct);
            
            // do some conversion before exporting
            using (var tmpTex = convert.Convert(image, stagingFormat.DxgiFormat, Mipmap, Layer,
                desc.Multiplier, UseCropping, new Size3(CropStartX, CropStartY, CropStartZ),
                new Size3(CropEndX - CropStartX + 1, CropEndY - CropStartY + 1, CropEndZ - CropStartZ + 1),
                new Size3(desc.FileFormat.GetAlignmentX(), desc.FileFormat.GetAlignmentY(), 0)))
            {
                // the final texture only has the relevant layers and mipmaps
                return ExportTexture(tmpTex, desc, -1, -1, ct);
            }
        }

        private Task ExportTexture(ITexture texture, ExportDescription desc, int mipmap, int layer, CancellationToken ct)
        {
            Debug.Assert(desc.StagingFormat.DxgiFormat == texture.Format);

            int firstMipmap = Math.Max(mipmap, 0);
            int nMipmaps = mipmap == -1 ? texture.NumMipmaps : 1;
            int firstLayer = Math.Max(layer, 0);
            int nLayer = layer == -1 ? texture.NumLayers : 1;

            var img = IO.CreateImage(desc.StagingFormat, texture.Size.GetMip(firstMipmap), nLayer, nMipmaps);
            
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

            return Task.Run(() =>
            {
                using (img)
                {
                    IO.SaveImage(img, desc.Filename, desc.Extension, desc.FileFormat, quality);
                }
            }, ct);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
