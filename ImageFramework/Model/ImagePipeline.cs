using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ImageFramework.Annotations;
using ImageFramework.DirectX;
using ImageFramework.DirectX.Query;
using ImageFramework.DirectX.Structs;
using ImageFramework.Model.Equation;
using ImageFramework.Model.Filter;
using ImageFramework.Model.Progress;
using ImageFramework.Model.Shader;
using ImageFramework.Utility;

namespace ImageFramework.Model
{
    public class ImagePipeline : INotifyPropertyChanged, IDisposable
    {
        public enum ChannelFilters
        {
            Red, // => rrr1
            Green, // => ggg1
            Blue, // => bbb1
            Alpha, // => aaa1
            RGB, // => rgb1
            RGBA // => rgba (no filter)
        }

        public ImagePipeline(int defaultImage)
        {
            Color = new FormulaModel(defaultImage);
            Alpha = new FormulaModel(defaultImage);
        }

        // clones the settings of the pipeline with a deep copy for image formulas
        public ImagePipeline Clone()
        {
            var pipe = new ImagePipeline(0);
            pipe.Color.Formula = Color.Formula;
            pipe.Alpha.Formula = Alpha.Formula;
            pipe.useFilter = useFilter;
            pipe.channelFilter = channelFilter;
            pipe.recomputeMipmaps = recomputeMipmaps;
            return pipe;
        }

        public FormulaModel Color { get; }
        public FormulaModel Alpha { get; }

        private bool useFilter = true;
        public bool UseFilter
        {
            get => useFilter;
            set
            {
                if (value == useFilter) return;
                useFilter = value;
                OnPropertyChanged(nameof(UseFilter));
                HasChanges = true;
            }
        }

        private ChannelFilters channelFilter = ChannelFilters.RGBA;
        public ChannelFilters ChannelFilter
        {
            get => channelFilter;
            set
            {
                if (value == channelFilter) return;
                channelFilter = value;
                OnPropertyChanged(nameof(ChannelFilter));
                HasChanges = true;
            }
        }

        private bool RequireChannelFilter => ChannelFilter != ChannelFilters.RGBA;

        private bool recomputeMipmaps = false;
        /// <summary>
        /// indicates if mipmaps will be recomputed in the end.
        /// if enabled: image combination and filters will only be executed on the upper layer => then mipmaps will be generated
        /// if disabled: image combination and filters will be executed on all layers => no mipmap recalculation
        /// </summary>
        public bool RecomputeMipmaps
        {
            get => recomputeMipmaps;
            set
            {
                if(value == recomputeMipmaps) return;
                recomputeMipmaps = value;
                OnPropertyChanged(nameof(RecomputeMipmaps));
                HasChanges = true;
            }
        }
        

        private bool isEnabled = true;

        /// <summary>
        /// indicates if this formula will be updated during a call to apply
        /// </summary>
        public bool IsEnabled
        {
            get => isEnabled;
            set
            {
                if (value == isEnabled) return;
                isEnabled = value;
                OnPropertyChanged(nameof(IsEnabled));
            }
        }

        private bool isValid = false;

        /// <summary>
        /// true if all formulas can be used. Will be set by the pipeline controller.
        /// setting this to false also causes HasChanges to become true
        /// </summary>
        public bool IsValid
        {
            get => isValid;
            internal set
            {
                if (value == isValid) return;
                isValid = value;
                OnPropertyChanged(nameof(IsValid));
                if (!isValid) // final image must be recomputed if formula got invalid
                    HasChanges = true;
            }
        }

        private bool hasChanges = true;

        /// <summary>
        /// true if the image must be recomputed due to formula changes or sth else
        /// </summary>
        internal bool HasChanges
        {
            get => hasChanges;
            set
            {
                if (value == hasChanges) return;
                hasChanges = value;
                OnPropertyChanged(nameof(HasChanges));
            }
        }

        /// <summary>
        /// indicates if the Image was taken from a texture cache or lend from the images model
        /// </summary>
        private bool cachedTexture = true;

        public ITexture Image { get; private set; }

        // ejects image without invoking on property changed (can be used to get ownership of the image before changing the equation)
        public ITexture EjectImage()
        {
            var res = Image;
            Image = null;
            return res;
        }

        /// <summary>
        /// returns the first image id that occurs in the color or alpha formula
        /// </summary>
        public int GetFirstImageId()
        {
            if (Color.HasImages)
                return Color.FirstImageId;
            return Alpha.FirstImageId;
        }

        internal class UpdateImageArgs
        {
            public Models Models;
            public List<FilterModel> Filters;
        }

        internal async Task UpdateImageAsync(UpdateImageArgs args, IProgress progress)
        {
            Debug.Assert(HasChanges);
            Debug.Assert(IsValid);
            Debug.Assert(Color.MaxImageId < args.Models.Images.NumImages);
            Debug.Assert(Alpha.MaxImageId < args.Models.Images.NumImages);


            progress.What = "resolving equation";

            if (TryMatchingInputImage(args)) return;

            try
            {
                // combine according to color and alpha formula
                ExecuteImageCombineShader(args);

                var computeFilter = UseFilter && args.Filters.Count != 0;
                var computeMipmaps = RecomputeMipmaps && args.Models.Images.NumMipmaps > 1;

                var progFilter = computeFilter && computeMipmaps ? progress.CreateSubProgress(0.8f) : progress;

                // next, apply filter
                if (computeFilter)
                {
                    await ExecuteFilter(args, progFilter);
                }

                // compute mipmaps
                if (computeMipmaps)
                {
                    await args.Models.Scaling.WriteMipmapsAsync(Image, progFilter.CreateSubProgress(1.0f));
                }

                if (RequireChannelFilter)
                {
                    ExecuteChannelFilter(args);
                }

                HasChanges = false;
                OnPropertyChanged(nameof(Image));
            }
            catch (OperationCanceledException)
            {
                // changes remain true
                Console.WriteLine("ImagePipeline threw OperationCancelledException");
                throw;
            }
        }

        private bool TryMatchingInputImage(UpdateImageArgs args)
        {
            // early out if color and alpha are from an input image
            if (Color.Formula.Length == 2 && Color.Formula.StartsWith("I") && Alpha.Formula == Color.Formula // one of the input images
                && (!UseFilter || args.Filters.Count == 0) // no filters used
                && (!RecomputeMipmaps || args.Models.Images.NumMipmaps <= 1) // no mipmap re computation
                && !RequireChannelFilter // no channel filter required
            ) 
            {
                // just reference the input image
                if (int.TryParse(Color.Formula.Substring(1), out var imgId))
                {
                    Image = args.Models.Images.Images[imgId].Image;
                    cachedTexture = false; // image was not taken from the image cache
                    HasChanges = false;
                    OnPropertyChanged(nameof(Image));
                    return true;
                }
            }

            return false;
        }

        private void ExecuteImageCombineShader(UpdateImageArgs args)
        {
            // first, use the combine shader
            using (var shader = new ImageCombineShader(Color.Converted, Alpha.Converted, args.Models.Images.NumImages,
                ShaderBuilder.Get(args.Models.Images.ImageType)))
            {
                var texSrc = args.Models.TextureCache.GetTexture();

                // do for all mipmaps if no mipmap re computation is enabled
                var nMipmaps = RecomputeMipmaps ? 1 : args.Models.Images.NumMipmaps;
                shader.Run(args.Models.Images, args.Models.SharedModel.Upload, texSrc, nMipmaps);

                Image = texSrc;
            }
        }

        private void ExecuteChannelFilter(UpdateImageArgs args)
        {
            // overwrite Image via uav:
            args.Models.SharedModel.ChannelFilter.Convert(Image, ChannelFilter, args.Models.SharedModel.Upload);
        }

        private async Task ExecuteFilter(UpdateImageArgs args, IProgress progress)
        {
            // get a second texture and swap between source and destination image
            ITexture[] tex = new ITexture[2];
            tex[0] = Image;
            Image = null;
            tex[1] = args.Models.TextureCache.GetTexture();
            int srcIdx = 0;

            try
            {
                for (var index = 0; index < args.Filters.Count; index++)
                {
                    var filter = args.Filters[index];
                    // TODO update parameters only on change
                    filter.Shader.UpdateParamBuffer();

                    for (int iteration = 0; iteration < filter.NumIterations; ++iteration)
                    {
                        await DoFilterIterationAsync(args, index, tex[srcIdx], tex[1 - srcIdx], iteration, progress);
                        srcIdx = 1 - srcIdx;
                        // if filter.DoIterations => this indicates that no thread wants to continue and that iterations should be stopped
                        if (filter.Shader.AbortIterations) break; 
                    }
                }
                // last chance before mipmap generation
                progress.Token.ThrowIfCancellationRequested();
            }
            catch (Exception)
            {
                args.Models.TextureCache.StoreTexture(tex[0]);
                args.Models.TextureCache.StoreTexture(tex[1]);
                throw;
            }

            // all filter were executed
            args.Models.TextureCache.StoreTexture(tex[1 - srcIdx]);
            Image = tex[srcIdx];
        }


        private Task DoFilterIterationAsync(UpdateImageArgs args, int index, ITexture src, ITexture dst, int iteration, IProgress progress)
        {
            progress.Token.ThrowIfCancellationRequested();
            var filter = args.Filters[index];

            var step = 1.0f / args.Filters.Count;
            progress.Progress = index * step;
            string what = filter.Name;
            if (filter.DoIterations || filter.IsSepa) what = $"{what} iteration: {iteration}";
            progress.What = what;

            // do for all mipmaps if no mipmap re computation is enabled
            var nMipmaps = RecomputeMipmaps ? 1 : args.Models.Images.NumMipmaps;
            filter.Shader.Run(args.Models.Images, src, dst, iteration, nMipmaps);
            args.Models.SharedModel.Sync.Set();

            return args.Models.SharedModel.Sync.WaitForGpuAsync(progress.Token);
        }

        internal void ResetImage(ITextureCache cache)
        {
            if (Image != null)
            {
                if (cachedTexture)
                    cache.StoreTexture(Image); // taken from the cache
                // otherwise it belongs to the image model

                Image = null;
                cachedTexture = true;
                OnPropertyChanged(nameof(Image));
            }

        }

        public void Dispose()
        {
            Image?.Dispose();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
