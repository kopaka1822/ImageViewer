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
using ImageFramework.Model.Shader;
using ImageFramework.Utility;

namespace ImageFramework.Model
{
    public class Models : IDisposable
    {
        public static readonly CultureInfo Culture = new CultureInfo("en-US");
        public ImagesModel Images { get; }
        public IReadOnlyList<ImagePipeline> Pipelines { get; }

        public ProgressModel Progress { get; }
        internal ShaderModel Shader { get; }

        private readonly List<ImagePipeline> pipelines = new List<ImagePipeline>();

        private readonly PipelineController pipelineController;

        public Models(int numPipelines = 1)
        {
            // models
            Shader = new ShaderModel();
            Images = new ImagesModel();
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


        public void Dispose()
        {
            Shader?.Dispose();
            Images?.Dispose();
            foreach (var imagePipeline in pipelines)
            {
                imagePipeline.Dispose();
            }
            pipelineController.Dispose();
        }
    }
}
