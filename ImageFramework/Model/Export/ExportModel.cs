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

        private readonly Models models;

        internal ExportModel(Models models)
        {
            this.models = models;

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

        private Size3 cropStart = new Size3(0);
        public Size3 CropStart
        {
            get => cropStart;
            set
            {
                if(value == cropStart) return;
                cropStart = value;
                OnPropertyChanged(nameof(CropStart));
            }
        }

        private Size3 cropEnd = new Size3(0);

        public Size3 CropEnd
        {
            get => cropEnd;
            set
            {
                if (value == cropEnd) return;
                cropEnd = value;
                OnPropertyChanged(nameof(CropEnd));
            }
        }

        public void Export(ITexture image, ExportDescription desc)
        {
            ExportAsync(image, desc);
            models.Progress.WaitForTask();
            if(!String.IsNullOrEmpty(models.Progress.LastError))
                throw new Exception(models.Progress.LastError);
        }

        // does export asynchronously => the task will be passed to the ProgressModel
        public void ExportAsync(ITexture image, ExportDescription desc)
        {
            var cts = new CancellationTokenSource();
            models.Progress.AddTask(ExportAsync(image, desc, cts.Token), cts);
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

                if((CropStart < Size3.Zero).AnyTrue() || (CropStart >= mipDim).AnyTrue())
                    throw new Exception("export crop start out of range: " + CropStart);

                if ((CropEnd < Size3.Zero).AnyTrue() || (CropEnd >= mipDim).AnyTrue())
                    throw new Exception("export crop end out of range: " + CropEnd);

                // end >= max
                if((CropStart > CropEnd).AnyTrue())
                    throw new Exception("export crop start must be smaller or equal to crop end");

                // set cropping to active if the image was actually cropped
                if(CropStart != Size3.Zero) croppingActive = true;
                if (CropEnd != mipDim - new Size3(1))
                    croppingActive = true;
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
                !alignmentActive && models.Overlay.Overlay == null)
                return ExportTexture(image, desc, Mipmap, Layer, ct);
            
            // do some conversion before exporting
            using (var tmpTex = models.SharedModel.Convert.Convert(image, stagingFormat.DxgiFormat, Mipmap, Layer,
                desc.Multiplier, UseCropping, CropStart,
                CropEnd - CropStart + new Size3(1),
                new Size3(desc.FileFormat.GetAlignmentX(), desc.FileFormat.GetAlignmentY(), 0), models.Scaling, models.Overlay.Overlay))
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
