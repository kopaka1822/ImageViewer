using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using ImageFramework.Annotations;
using ImageFramework.DirectX;
using ImageFramework.Model.Scaling.Down;
using ImageFramework.Utility;

namespace ImageFramework.Model.Scaling
{
    public class ScalingModel : IDisposable, INotifyPropertyChanged
    {
        private readonly Models models;
        private ITextureCache customTexCache = null;
        private IDownscalingShader boxMinify = null;
        private IDownscalingShader triangleMinify = null;
        private IDownscalingShader lanzosMinify = null;
        private IDownscalingShader detailPreservingMinify = null;
        private IDownscalingShader veryDetailPreservingMinify = null;

        internal ScalingModel(Models models)
        {
            this.models = models;
        }

        public enum MagnifyFilters
        {
            MitchellNetravali,
        }


        public enum MinifyFilters
        {
            Box,
            Triangle,
            Lanzos,
            DetailPreserving, // Rapid, Detail-Preserving Image Downscaling 2016 with y = 0.5
            VeryDetailPreserving, // Rapid, Detail-Preserving Image Downscaling 2016 with y = 1.0
        }

        private MagnifyFilters magnify = MagnifyFilters.MitchellNetravali;

        /// filter usued for upscaling
        public MagnifyFilters Magnify
        {
            get => magnify;
            set
            {
                if (value == magnify) return;
                magnify = value;
                OnPropertyChanged(nameof(Magnify));
            }
        }

        private MinifyFilters minify = MinifyFilters.Box;

        /// filter used for downscaling
        public MinifyFilters Minify
        {
            get => minify;
            set
            {
                if (value == minify) return;
                minify = value;
                OnPropertyChanged(nameof(Minify));
            }
        }

        public void Scale(ITexture src, ITexture dst, int dstMipmap)
        {
            var cache = GetMinifyTextureCache(src);

            // TODO start with minify => then maxify
            
            throw new NotImplementedException();
        }

        internal async Task WriteMipmapsAsync(ITexture tex, CancellationToken ct)
        {
            var shader = GetMinify();
            var cache = GetMinifyTextureCache(tex);
            var hasAlpha = models.Stats.GetStatisticsFor(tex).HasAlpha;

            models.Progress.Progress = 0.0f;
            models.Progress.What = "Generating Mipmaps";

            for (int curMip = 1; curMip < tex.NumMipmaps; ++curMip)
            {
                // don't use the previous mipmaps (too much error) => using 16 times bigger is okay
                var srcMip = Math.Max(0, curMip - 4);
                shader.Run(tex, tex, srcMip, curMip, hasAlpha, models.SharedModel.Upload, cache);

                models.SharedModel.Sync.Set();
                await models.SharedModel.Sync.WaitForGpuAsync(ct);
                models.Progress.Progress = curMip / (float)(tex.NumMipmaps - 1);
            }
        }

        public void WriteMipmaps(ITexture tex)
        {
            var cts = new CancellationTokenSource();
            models.Progress.AddTask(Task.Run(async () => await WriteMipmapsAsync(tex, cts.Token)), cts);
            models.Progress.WaitForTask();
            if (!String.IsNullOrEmpty(models.Progress.LastError))
                throw new Exception(models.Progress.LastError);
        }

        private IDownscalingShader GetMinify()
        {
            switch (Minify)
            {
                case MinifyFilters.Box:
                    return boxMinify ?? (boxMinify = new BoxScalingShader(models.SharedModel.QuadShader));
                case MinifyFilters.Triangle:
                    return triangleMinify ?? (triangleMinify = new TriangleScalingShader(models.SharedModel.QuadShader));
                case MinifyFilters.Lanzos:
                    return lanzosMinify ?? (lanzosMinify = new LanzosScalingShader(models.SharedModel.QuadShader));
                case MinifyFilters.DetailPreserving:
                    if(boxMinify == null) boxMinify = new BoxScalingShader(models.SharedModel.QuadShader);
                    return detailPreservingMinify ?? (detailPreservingMinify = new DetailPreservingDownscalingShader(boxMinify, false, models.SharedModel.QuadShader));
                case MinifyFilters.VeryDetailPreserving:
                    if (boxMinify == null) boxMinify = new BoxScalingShader(models.SharedModel.QuadShader);
                    return veryDetailPreservingMinify ?? (veryDetailPreservingMinify = new DetailPreservingDownscalingShader(boxMinify, true, models.SharedModel.QuadShader));
            }

            throw new Exception($"invalid minify filter specified: {minify}");
        }

        private ITextureCache GetMinifyTextureCache(ITexture src)
        {
            // determine which texture cache to use
            ITextureCache cache = customTexCache;
            if (models.TextureCache.IsCompatibleWith(src))
            {
                cache = models.TextureCache;
            }
            else if (customTexCache == null || !customTexCache.IsCompatibleWith(src))
            {
                customTexCache?.Dispose();
                cache = customTexCache = new TextureCache(src);
            }

            return cache;
        }

        public void Dispose()
        {
            customTexCache?.Dispose();
            boxMinify?.Dispose();
            triangleMinify?.Dispose();
            lanzosMinify?.Dispose();
            detailPreservingMinify?.Dispose();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
