using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
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

        public void WriteMipmaps(ITexture tex)
        {
            var shader = GetMinify();
            var cache = GetMinifyTextureCache(tex);
            var hasAlpha = models.Stats.GetStatisticsFor(tex).HasAlpha;

            for (int curMip = 1; curMip < tex.NumMipmaps; ++curMip)
            {
                shader.Run(tex, tex, curMip, hasAlpha, models.SharedModel.Upload, cache);
            }
        }

        private IDownscalingShader GetMinify()
        {
            switch (Minify)
            {
                case MinifyFilters.Box:
                    return boxMinify ?? (boxMinify = new BoxScalingShader());
                case MinifyFilters.Triangle:
                    return triangleMinify ?? (triangleMinify = new TriangleScalingShader());
                case MinifyFilters.Lanzos:
                    return lanzosMinify ?? (lanzosMinify = new LanzosScalingShader());
                case MinifyFilters.DetailPreserving:
                    if(boxMinify == null) boxMinify = new BoxScalingShader();
                    return detailPreservingMinify ?? (detailPreservingMinify = new DetailPreservingDownscalingShader(boxMinify, false));
                case MinifyFilters.VeryDetailPreserving:
                    if (boxMinify == null) boxMinify = new BoxScalingShader();
                    return veryDetailPreservingMinify ?? (veryDetailPreservingMinify = new DetailPreservingDownscalingShader(boxMinify, true));
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
