using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ImageFramework.Annotations;
using ImageFramework.Controller;
using ImageFramework.DirectX;
using ImageFramework.ImageLoader;
using ImageFramework.Model.Export;
using ImageFramework.Model.Filter;
using ImageFramework.Model.Scaling;
using ImageFramework.Model.Shader;
using ImageFramework.Model.Statistics;
using ImageFramework.Utility;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using Device = ImageFramework.DirectX.Device;
using Texture3D = ImageFramework.DirectX.Texture3D;

namespace ImageFramework.Model
{
    public class Models : IDisposable, INotifyPropertyChanged
    {
        public static readonly CultureInfo Culture = new CultureInfo("en-US");

        public readonly int NumPipelines;

        #region Required Components


        public ImagesModel Images { get; }

        public FiltersModel Filter { get; }

        private List<ImagePipeline> pipelines = new List<ImagePipeline>();
        public IReadOnlyList<ImagePipeline> Pipelines { get; }

        public int NumEnabled => Pipelines.Count(pipe => pipe.IsEnabled);

        private ExportModel export = null;
        public ExportModel Export => export ?? (export = new ExportModel(SharedModel, Progress));

        // soft reset that clears images, filters and resets formulas
        public event EventHandler SoftReset;

        private GifModel gif = null;
        public GifModel Gif => gif ?? (gif = new GifModel(SharedModel.QuadShader, SharedModel.Upload, Progress));

        public ProgressModel Progress { get; }

        public SharedModel SharedModel { get; }
        
        internal ITextureCache TextureCache { get; }

        private readonly PipelineController pipelineController;

        #endregion

        #region On Demand Properties

        private ThumbnailModel thumbnail = null;
        private ThumbnailModel Thumbnail => thumbnail ?? (thumbnail = new ThumbnailModel(SharedModel.QuadShader));

        private PixelValueShader pixelValueShader = null;

        private PixelValueShader PixelValueShader =>
            pixelValueShader ?? (pixelValueShader = new PixelValueShader(SharedModel));

        private StatisticsModel stats = null;
        public StatisticsModel Stats => stats ?? (stats = new StatisticsModel(SharedModel));

        private ConvertPolarShader polarConvertShader = null;

        private ConvertPolarShader PolarConvertShader =>
            polarConvertShader ?? (polarConvertShader = new ConvertPolarShader(SharedModel.QuadShader));

        private ScalingModel scaling = null;
        public ScalingModel Scaling => scaling ?? (scaling = new ScalingModel(this));

        #endregion

        public Models(int numPipelines = 1)
        {
            NumPipelines = numPipelines;
            CheckDeviceCapabilities();

            SharedModel = new SharedModel();
            Images = new ImagesModel(SharedModel.ScaleShader);
            TextureCache = new ImageModelTextureCache(Images);

            Filter = new FiltersModel(Images);
            Progress = new ProgressModel();

            for (int i = 0; i < numPipelines; ++i)
            {
                pipelines.Add(new ImagePipeline(i));
                pipelines.Last().PropertyChanged += PipeOnPropertyChanged;
            }
            Pipelines = pipelines;

            // pipeline controller
            pipelineController = new PipelineController(this);
        }

        // removes all images, filters and resets equations
        public void Reset()
        {
            Images.Clear();
            Filter.Clear();
            int id = 0;
            foreach (var pipe in Pipelines)
            {
                pipe.Color.Formula = "I" + id;
                pipe.Alpha.Formula = "I" + id;
                ++id;
            }
            OnSoftReset();
        }

        

        private void PipeOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(ImagePipeline.IsEnabled):
                    OnPropertyChanged(nameof(NumEnabled));
                    break;
            }
        }

        /// <summary>
        /// creates a new filter from a file.
        /// this filter is not added to the filter list
        /// </summary>
        /// <param name="filename">filter filename</param>
        /// <returns></returns>
        public FilterModel CreateFilter(string filename)
        {
            var loader = new FilterLoader(filename, Filter.CurrentTarget);
            
            return new FilterModel(loader, NumPipelines);
        }
       
        /// <inheritdoc cref="PixelValueShader.Run"/>
        public Color GetPixelValue(ITexture image, Size3 coord, int layer = 0, int mipmap = 0, int radius = 0)
        {
            return PixelValueShader.Run(image, coord, layer, mipmap, radius);
        }

        /// <inheritdoc cref="ThumbnailModel.CreateThumbnail"/>
        /// Image format will be BGRA8 because this is the format expected for windows bitmaps
        public TextureArray2D CreateThumbnail(int size, ITexture texture, int layer = 0)
        {
            return Thumbnail.CreateThumbnail(size, texture, Format.B8G8R8A8_UNorm_SRgb, layer);
        }

        /// <summary>
        /// loads image and adds it to Images 
        /// </summary>
        /// <param name="filename"></param>
        /// <exception cref="Exception"></exception>
        public void AddImageFromFile(string filename)
        {
            var tex = IO.LoadImageTexture(filename, out var originalFormat);
            try
            {
                Images.AddImage(tex, filename, originalFormat);
            }
            catch (Exception)
            {
                tex.Dispose();
                throw;
            }
        }

        /// <summary>
        /// takes a list of textures and combines them into an array.
        /// </summary>
        /// <param name="textures">list of images with same dimensions and no layers</param>
        /// <returns>combined image</returns>
        public TextureArray2D CombineToArray(List<TextureArray2D> textures)
        {
            if (textures.Count == 0) return null;
            var first = textures[0];
            var tex = new TextureArray2D(textures.Count, first.NumMipmaps, first.Size, Format.R32G32B32A32_Float, false);
            for(int i = 0; i < textures.Count; ++i)
            {
                for (int curMip = 0; curMip < first.NumMipmaps; ++curMip)
                {
                    SharedModel.Convert.CopyLayer(textures[i], 0, curMip, tex, i, curMip);
                }
            }

            return tex;
        }

        /// converts lat long to cubemap
        public TextureArray2D ConvertToCubemap(TextureArray2D latLong, int resolution)
        {
            return PolarConvertShader.ConvertToCube(latLong, resolution);
        }

        /// converts cubemap to lat long
        public TextureArray2D ConvertToLatLong(TextureArray2D cube, int resolution)
        {
            return PolarConvertShader.ConvertToLatLong(cube, resolution);
        }

        /// <summary>
        /// exports a pipeline image with the given format and extension.
        /// Apply will be called by this method if required
        /// </summary>
        /// <param name="filename"></param>
        /// <param name="extension"></param>
        /// <param name="format"></param>
        /// <param name="pipelineId"></param>
        public void ExportPipelineImage(string filename, string extension, GliFormat format, int pipelineId = 0)
        {
            var desc = new ExportDescription(filename, extension, Export) { FileFormat = format };
            if (!Pipelines[pipelineId].IsValid)
                throw new Exception($"current image formula is invalid. At least " +
                                    $"{Math.Max(Math.Max(Pipelines[pipelineId].Color.MaxImageId, Pipelines[pipelineId].Alpha.MaxImageId), 1)} " +
                                    $"images are required for it to be valid");

            // apply changes before exporting
            Apply();
            Export.Export(Pipelines[pipelineId].Image, desc);
        }

        /// <summary>
        /// returns ids of all enabled pipeline
        /// </summary>
        public List<int> GetEnabledPipelines()
        {
            var res = new List<int>();
            for (int i = 0; i < Pipelines.Count; ++i)
                if (Pipelines[i].IsEnabled)
                    res.Add(i);

            return res;
        }

        /// <summary>
        /// returns the id of the first enabled pipeline
        /// throws exception if nothing is visible
        /// </summary>
        public int GetFirstEnabledPipeline()
        {
            for (int i = 0; i < Pipelines.Count; ++i)
                if (Pipelines[i].IsEnabled)
                    return i;
            throw new Exception("no pipeline enabled");
        }

        /// <summary>
        /// Forces all pending pipeline changes to be computed
        /// </summary>
        public void Apply()
        {
            ApplyAsync();
            Progress.WaitForTask();
        }

        public void ApplyAsync()
        {
            var cts = new CancellationTokenSource();
            Progress.AddTask(pipelineController.UpdateImagesAsync(cts.Token), cts);
        }

        /// <summary>
        /// checks if this graphics card supports the relevant features
        /// </summary>
        private void CheckDeviceCapabilities()
        {
            var dev = Device.Get();
            // check supported format capabilities
            foreach (var f in IO.SupportedFormats)
            {
                var sup = dev.CheckFormatSupport(f);

                // TODO this should depend on the type
                if ((sup & FormatSupport.Texture2D) == 0)
                    throw new Exception($"Texture2D support for {f} is required");
                if((sup & FormatSupport.Texture3D) == 0)
                    throw new Exception($"Texture3D support for {f} is required");

                // TODO this can be optional
                if ((sup & FormatSupport.MipAutogen) == 0)
                    throw new Exception($"MipAutogen support for {f} is required");

                if ((sup & FormatSupport.RenderTarget) == 0)
                    throw new Exception($"RenderTarget support for {f} is required");

                if (f == Format.R32G32B32A32_Float)
                {
                    if ((sup & FormatSupport.TypedUnorderedAccessView) == 0)
                        throw new Exception($"TypesUnorderedAccess support for {f} is required");
                    if((sup & FormatSupport.TextureCube) == 0)
                        throw new Exception($"TextureCube support for {f} is required");
                }
            }
        }

        public virtual void Dispose()
        {
            Progress?.Dispose();
            thumbnail?.Dispose();
            stats?.Dispose();
            gif?.Dispose();
            scaling?.Dispose();

            Images?.Dispose();
            Filter?.Dispose();
            polarConvertShader?.Dispose();
            foreach (var imagePipeline in pipelines)
            {
                imagePipeline.Dispose();
            }
            pixelValueShader?.Dispose();
            SharedModel?.Dispose();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected virtual void OnSoftReset()
        {
            SoftReset?.Invoke(this, EventArgs.Empty);
        }
    }
}
