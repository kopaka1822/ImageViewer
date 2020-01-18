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
using ImageFramework.Model.Shader;
using ImageFramework.Utility;

namespace ImageFramework.Model
{
    public class ImagePipeline : INotifyPropertyChanged, IDisposable
    {
        public ImagePipeline(int defaultImage)
        {
            Color = new FormulaModel(defaultImage);
            Alpha = new FormulaModel(defaultImage);
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

        internal class UpdateImageArgs
        {
            public Models Models;
            public List<FilterModel> Filters;
        }

        internal async Task UpdateImageAsync(UpdateImageArgs args, CancellationToken ct)
        {
            Debug.Assert(HasChanges);
            Debug.Assert(IsValid);
            Debug.Assert(Color.MaxImageId < args.Models.Images.NumImages);
            Debug.Assert(Alpha.MaxImageId < args.Models.Images.NumImages);

            args.Models.Progress.Progress = 0.0f;
            args.Models.Progress.What = "resolving equation";

            if (TryMatchingInputImage(args)) return;

            try
            {
                // combine according to color and alpha formula
                ExecuteImageCombineShader(args);

                // next, apply filter
                if (UseFilter && args.Filters.Count != 0)
                {
                    await ExecuteFilter(args, ct);
                }

                // compute mipmaps
                args.Models.Progress.Progress = 1.0f;
                if (args.Models.Images.NumMipmaps > 1)
                {
                    args.Models.Progress.What = "Generating Mipmaps";
                    args.Models.Scaling.WriteMipmaps(Image);
                }

                HasChanges = false;
                OnPropertyChanged(nameof(Image));
            }
            catch (OperationCanceledException)
            {
                // changes remain true
                IsValid = false;
                throw;
            }
        }

        private bool TryMatchingInputImage(UpdateImageArgs args)
        {
            // early out if color and alpha are from an input image
            if (Color.Formula.Length == 2 && Color.Formula.StartsWith("I") && Alpha.Formula == Color.Formula && (!UseFilter || args.Filters.Count == 0))
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

                shader.Run(args.Models.Images, args.Models.SharedModel.Upload, texSrc);

                Image = texSrc;
            }
        }

        private async Task ExecuteFilter(UpdateImageArgs args, CancellationToken ct)
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
                        await DoFilterIterationAsync(args, index, tex[srcIdx], tex[1 - srcIdx], iteration, ct);
                        srcIdx = 1 - srcIdx;
                    }
                }
                // last chance before mipmap generation
                ct.ThrowIfCancellationRequested();
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


        private Task DoFilterIterationAsync(UpdateImageArgs args, int index, ITexture src, ITexture dst, int iteration, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            var filter = args.Filters[index];

            filter.Shader.Run(args.Models.Images, src, dst, args.Models.SharedModel.Upload, iteration);
            args.Models.SharedModel.Sync.Set();

            var step = 1.0f / args.Filters.Count;
            args.Models.Progress.Progress = index * step + iteration * step * 0.5f;
            args.Models.Progress.What = filter.Name;

            return args.Models.SharedModel.Sync.WaitForGpuAsync(ct);
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
