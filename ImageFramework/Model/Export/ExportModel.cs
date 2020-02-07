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
        private readonly Models models;

        internal ExportModel(Models models)
        {
            this.models = models;
        }

        public void Export(ExportDescription desc)
        {
            ExportAsync(desc);
            models.Progress.WaitForTask();
            if(!String.IsNullOrEmpty(models.Progress.LastError))
                throw new Exception(models.Progress.LastError);
        }

        // does export asynchronously => the task will be passed to the ProgressModel
        public void ExportAsync(ExportDescription desc)
        {
            var cts = new CancellationTokenSource();
            models.Progress.AddTask(ExportAsync(desc, cts.Token), cts);
        }

        private Task ExportAsync(ExportDescription desc, CancellationToken ct)
        {
            Debug.Assert(desc.Texture != null);

            desc.Verify();

            var mipIdx = Math.Max(desc.Mipmap, 0);
            // general boundaries
            var mipDim = desc.Texture.Size.GetMip(mipIdx);

            // default crop (entire screen)
            var cropStart = Size3.Zero;
            var cropEnd = mipDim;

            // test cropping dimensions
            bool croppingActive = false;
            if (desc.UseCropping)
            {
                desc.GetCropRect(out cropStart, out cropEnd);

                // set cropping to active if the image was actually cropped
                if(cropStart != Size3.Zero) croppingActive = true;
                if (cropEnd != mipDim - new Size3(1))
                    croppingActive = true;
            }

            // image is ready for export!
            var stagingFormat = desc.StagingFormat;
            
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if (desc.Texture.Format == stagingFormat.DxgiFormat && !croppingActive && desc.Multiplier == 1.0f &&
                !desc.RequiresAlignment && desc.Overlay == null && desc.Scale == 1)
                return ExportTexture(desc.Texture, desc, desc.Mipmap, desc.Layer, ct);
            
            // do some conversion before exporting
            using (var tmpTex = models.SharedModel.Convert.Convert(desc.Texture, stagingFormat.DxgiFormat, desc.Mipmap, desc.Layer,
                desc.Multiplier, desc.UseCropping, cropStart,
                cropEnd - cropStart + new Size3(1),
                new Size3(desc.FileFormat.GetAlignmentX(), desc.FileFormat.GetAlignmentY(), 0), models.Scaling, desc.Overlay, desc.Scale))
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
                    IO.SaveImage(img, desc.Filename, desc.Extension, desc.FileFormat, desc.Quality);
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
