using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using ImageFramework.Annotations;
using ImageFramework.DirectX;
using ImageFramework.Model.Progress;
using ImageFramework.Model.Scaling.AlphaTest;
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

        private IPostprocess alphaScale = null;
        private IPostprocess alphaPyramid = null;
        private IPostprocess alphaConnectivity = null;

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
            Lanczos,
            DetailPreserving, // Rapid, Detail-Preserving Image Downscaling 2016 with y = 0.5
            VeryDetailPreserving, // Rapid, Detail-Preserving Image Downscaling 2016 with y = 1.0
        }

        public enum AlphaTestPostprocess
        {
            None, // no postprocessing
            AlphaScale, // Castano Alpha Scaling
            AlphaPyramid, // Cem Yuksels Alpha Distribution
            AlphaConnectivity // experimental
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

        private AlphaTestPostprocess alphaTestProcess = AlphaTestPostprocess.None;

        public AlphaTestPostprocess AlphaTestProcess
        {
            get => alphaTestProcess;
            set
            {
                if (value == alphaTestProcess) return;
                alphaTestProcess = value;
                OnPropertyChanged(nameof(AlphaTestProcess));
            }
        }

        public void Scale(ITexture src, ITexture dst, int dstMipmap)
        {
            var cache = GetMinifyTextureCache(src);

            // TODO start with minify => then maxify
            
            throw new NotImplementedException();
        }

        internal async Task WriteMipmapsAsync(ITexture tex, IProgress progress)
        {
            Debug.Assert(tex.HasUaViews || tex.HasRtViews);

            var shader = GetMinify();
            var cache = GetMinifyTextureCache(tex);
            var hasAlpha = models.Stats.GetStatisticsFor(tex).HasAlpha;

            ITexture dstTex = tex;
            if (!tex.HasUaViews) // cannot write directly into texture
                dstTex = cache.GetTexture(); // get texture from cache with unordered access view

            progress.What = "Generating Mipmaps";
            var curProg = progress.CreateSubProgress(tex.HasUaViews ? 1.0f : 0.9f);

            for (int curMip = 1; curMip < tex.LayerMipmap.Mipmaps; ++curMip)
            {
                // don't use the previous mipmaps (too much error) => using 16 times bigger is okay
                var srcMip = Math.Max(0, curMip - 4);
                shader.Run(srcMip == 0 ? tex : dstTex, dstTex, srcMip, curMip, hasAlpha, models.SharedModel.Upload, cache);

                models.SharedModel.Sync.Set();
                await models.SharedModel.Sync.WaitForGpuAsync(progress.Token);
                
                curProg.Progress = curMip / (float)(tex.LayerMipmap.Mipmaps - 1);
            }

            curProg.Progress = 1.0f;

            if(hasAlpha)
            {
                progress.What = "Post Processing Alpha";
                var postShader = GetAlphaPostprocess();
                if(postShader != null)
                {
                    // copy over mipmap 0 as well (was skipped by the previous step)
                    if (!ReferenceEquals(dstTex, tex))
                    {
                        for (int curLayer = 0; curLayer < tex.NumLayers; ++curLayer)
                        {
                            var lm = new LayerMipmapSlice(curLayer, 0);
                            models.SharedModel.Convert.CopyLayer(tex, lm, dstTex, lm);
                        }
                    }
                    postShader.Run(dstTex, hasAlpha, models.SharedModel.Upload, cache);

                    //models.SharedModel.Sync.Set();
                    //await models.SharedModel.Sync.WaitForGpuAsync(progress.Token);
                }
            }

            if (!tex.HasUaViews) // write back from dstTex to tex
            {
                progress.What = "Finalizing Mipmaps";
                for (int curLayer = 0; curLayer < tex.NumLayers; ++curLayer)
                {
                    for (int curMip = 1; curMip < tex.NumMipmaps; ++curMip)
                    {
                        var lm = new LayerMipmapSlice(curLayer, curMip);
                        models.SharedModel.Convert.CopyLayer(dstTex, lm, tex, lm);
                        models.SharedModel.Sync.Set();
                        await models.SharedModel.Sync.WaitForGpuAsync(progress.Token);
                    }
                }
                cache.StoreTexture(dstTex);
            }
        }

        public void WriteMipmaps(ITexture tex)
        {
            var cts = new CancellationTokenSource();
            models.Progress.AddTask(Task.Run(async () => await WriteMipmapsAsync(tex, models.Progress.GetProgressInterface(cts.Token))), cts, false);
            models.Progress.WaitForTask();
            if (!String.IsNullOrEmpty(models.Progress.LastError))
                throw new Exception(models.Progress.LastError);
        }

        private IDownscalingShader GetMinify()
        {
            switch (Minify)
            {
                case MinifyFilters.Box:
                    return boxMinify ?? (boxMinify = new BoxScalingShader());
                case MinifyFilters.Triangle:
                    return triangleMinify ?? (triangleMinify = new TriangleScalingShader());
                case MinifyFilters.Lanczos:
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

        private IPostprocess GetAlphaPostprocess()
        {
            switch(alphaTestProcess)
            {
                case AlphaTestPostprocess.None:
                    return null;
                case AlphaTestPostprocess.AlphaScale:
                    return alphaScale ?? (alphaScale = new AlphaScalePostprocess(models.Stats));
                // TODO add other cases        
            }

            throw new Exception($"invalid alpha postprocess filter specified: {alphaTestProcess}");
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

            alphaScale?.Dispose();
            alphaPyramid?.Dispose();
            alphaConnectivity?.Dispose();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
