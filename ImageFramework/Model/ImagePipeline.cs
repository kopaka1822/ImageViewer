using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Documents;
using ImageFramework.Annotations;
using ImageFramework.DirectX;
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
                if(value == useFilter) return;
                useFilter = value;
                OnPropertyChanged(nameof(UseFilter));
                HasChanges = true;
            }
        }

        /// <summary>
        /// indicates if the Image was taken from a texture cache or lend from the images model
        /// </summary>
        private bool cachedTexture = true;
        public TextureArray2D Image { get; internal set; }

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
                if(value == isValid) return;
                isValid = value;
                OnPropertyChanged(nameof(IsValid));
                if (!isValid) // final image must be recomputed if formula got invalid
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
                if(value == isEnabled) return;
                isEnabled = value;
                OnPropertyChanged(nameof(IsEnabled));
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
                if(value == hasChanges) return;
                hasChanges = value;
                OnPropertyChanged(nameof(HasChanges));
            }
        }

        internal class UpdateImageArgs
        {
            public ImagesModel Images;
            public ProgressModel Progress;
            public TextureCache TextureCache;
            public UploadBuffer<LayerLevelFilter> LayerLevelBuffer;
            public List<FilterModel> Filters;
            public SyncQuery Sync;
        }

        internal async Task UpdateImageAsync(UpdateImageArgs args, CancellationToken ct)
        {
            Debug.Assert(HasChanges);
            Debug.Assert(IsValid);
            Debug.Assert(Color.MaxImageId < args.Images.NumImages);
            Debug.Assert(Alpha.MaxImageId < args.Images.NumImages);

            args.Progress.IsProcessing = true;
            args.Progress.Progress = 0.0f;
            args.Progress.What = "resolving equation";

            // early out if color and alpha are from an input image
            if (Color.Formula.Length == 2 && Color.Formula.StartsWith("I") && Alpha.Formula == Color.Formula && (!UseFilter || args.Filters.Count == 0))
            {
                // just reference the input image
                if (int.TryParse(Color.Formula.Substring(1), out var imgId))
                {
                    Image = args.Images.Images[imgId].Image;
                    cachedTexture = false; // image was not taken from the image cache
                    HasChanges = false;
                    args.Progress.IsProcessing = false;
                    OnPropertyChanged(nameof(Image));
                    return;
                }
            }

            try
            {
                // first, use the combine shader
                using (var shader = new ImageCombineShader(Color.Converted, Alpha.Converted, args.Images.NumImages))
                {
                    var texSrc = args.TextureCache.GetTexture();

                    shader.Run(args.Images, args.LayerLevelBuffer, texSrc);

                    Image = texSrc;
                }

                // next, apply filter
                if (UseFilter && args.Filters.Count != 0)
                {
                    Debug.Assert(args.Filters != null);

                    // get a second texture and swap between source and destination image
                    TextureArray2D[] tex = new TextureArray2D[2];
                    tex[0] = Image;
                    Image = null;
                    tex[1] = args.TextureCache.GetTexture();
                    int srcIdx = 0;

                    try
                    {
                        for (var index = 0; index < args.Filters.Count; index++)
                        {
                            var filter = args.Filters[index];
                            // TODO update parameters only on change
                            filter.Shader.UpdateParamBuffer();

                            await DoFilterIterationAsync(args, index, tex[srcIdx], tex[1 - srcIdx], 0, ct);

                            if (filter.IsSepa)
                                await DoFilterIterationAsync(args, index, tex[1 - srcIdx], tex[srcIdx], 1, ct);
                            else
                                srcIdx = 1 - srcIdx;
                        }
                    }
                    catch(Exception)
                    {
                        args.TextureCache.StoreTexture(tex[0]);
                        args.TextureCache.StoreTexture(tex[1]);
                        throw;
                    }

                    // all filter were executed
                    Image = tex[srcIdx];
                    args.TextureCache.StoreTexture(tex[1 - srcIdx]);
                }

                args.Progress.Progress = 1.0f;
                HasChanges = false;
                OnPropertyChanged(nameof(Image));
            }
            catch (OperationCanceledException)
            {
                // changes remain true
            }
            catch (Exception)
            {
                IsValid = false;
                throw;
            }
            finally
            {
                args.Progress.IsProcessing = false;
            }
        }

        private Task DoFilterIterationAsync(UpdateImageArgs args, int index, TextureArray2D src, TextureArray2D dst, int iteration, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            var filter = args.Filters[index];

            filter.Shader.Run(args.Images, src, dst, args.LayerLevelBuffer, iteration);
            args.Sync.Set();

            var step = 1.0f / args.Filters.Count;
            args.Progress.Progress = index * step + iteration * step * 0.5f;
            args.Progress.What = filter.Name;

            return args.Sync.WaitForGpuAsync(ct);
        }

        internal void ResetImage(TextureCache cache)
        {
            if (Image != null)
            {
                if(cachedTexture)
                    cache.StoreTexture(Image); // taken from the cache
                // otherwise it belongs to the image model

                Image = null;
                cachedTexture = true;
                OnPropertyChanged(nameof(Image));
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
            Image?.Dispose();
        }
    }
}
