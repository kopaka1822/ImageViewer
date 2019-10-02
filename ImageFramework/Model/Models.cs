using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ImageFramework.Controller;
using ImageFramework.DirectX;
using ImageFramework.ImageLoader;
using ImageFramework.Model.Export;
using ImageFramework.Model.Shader;
using ImageFramework.Utility;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using Device = ImageFramework.DirectX.Device;

namespace ImageFramework.Model
{
    public class Models : IDisposable
    {
        public static readonly CultureInfo Culture = new CultureInfo("en-US");
        public ImagesModel Images { get; }
        public IReadOnlyList<ImagePipeline> Pipelines { get; }

        public ExportModel Export { get; }

        public ProgressModel Progress { get; }

        private readonly List<ImagePipeline> pipelines = new List<ImagePipeline>();

        private readonly PipelineController pipelineController;

        public Models(int numPipelines = 1)
        {
            CheckDeviceCapabilities();

            // models
            Images = new ImagesModel();
            Export = new ExportModel();
            Progress = new ProgressModel();

            for (int i = 0; i < numPipelines; ++i)
            {
                pipelines.Add(new ImagePipeline(i));
            }
            Pipelines = pipelines;

            // pipeline controller
            pipelineController = new PipelineController(this);
        }

        /// <summary>
        /// loads image and adds it to Images 
        /// </summary>
        /// <param name="filename"></param>
        /// <exception cref="Exception"></exception>
        public void AddImageFromFile(string filename)
        {
            using (var image = IO.LoadImage(filename))
            {
                var tex = new TextureArray2D(image);

                try
                {
                    Images.AddImage(tex, filename, image.OriginalFormat);
                }
                catch (Exception)
                {
                    tex.Dispose();
                    throw;
                }
            }
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
            var desc = new ExportDescription(filename, extension, Export) {FileFormat = format};
            // apply changes before exporting
            Apply();
            Export.Export(Pipelines[pipelineId].Image, desc);
        }

        /// <summary>
        /// Forces all pending pipeline changes to be computed
        /// </summary>
        public void Apply()
        {
            using (var cts = new CancellationTokenSource())
            {
                ApplyAsync(cts.Token).Wait();
            }
        }

        /// <summary>
        /// Forces all pending pipeline changes to be computed
        /// </summary>
        public async Task ApplyAsync(CancellationToken ct)
        {
            await pipelineController.UpdateImagesAsync(ct);
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

                if ((sup & FormatSupport.Texture2D) == 0)
                    throw new Exception($"Texture2D support for {f} is required");
                // TODO this can be optional
                if ((sup & FormatSupport.MipAutogen) == 0)
                    throw new Exception($"MipAutogen support for {f} is required");

                if ((sup & FormatSupport.RenderTarget) == 0)
                    throw new Exception($"RenderTarget support for {f} is required");

                if (f == Format.R32G32B32A32_Float)
                {
                    if((sup & FormatSupport.TypedUnorderedAccessView) == 0)
                        throw new Exception($"TypesUnorderedAccess support for {f} is required");
                }
            }
        }

        public void Dispose()
        {
            Export?.Dispose();
            Images?.Dispose();
            foreach (var imagePipeline in pipelines)
            {
                imagePipeline.Dispose();
            }
            pipelineController.Dispose();
        }
    }
}
