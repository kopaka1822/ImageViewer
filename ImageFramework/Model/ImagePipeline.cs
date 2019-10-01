using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using ImageFramework.Annotations;
using ImageFramework.DirectX;
using ImageFramework.DirectX.Structs;
using ImageFramework.Model.Equation;
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
            }
        }

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
            public UploadBuffer<LayerLevelData> LayerLevelBuffer;
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
                if (UseFilter)
                {
                    ct.ThrowIfCancellationRequested();
                    

                }

                HasChanges = false;
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

        internal void ResetImage(TextureCache cache)
        {
            if (Image != null)
            {
                cache.StoreTexture(Image);
                Image = null;
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
