using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL4;
using TextureViewer.Controller;
using TextureViewer.Controller.TextureViews.Shader;
using TextureViewer.glhelper;
using TextureViewer.Models.Shader;
using TextureViewer.Models.Shader.Statistics;

namespace TextureViewer.Models
{
    /// <summary>
    /// opengl related data used for texture views
    /// </summary>
    public class OpenGlModel
    {
        public VertexArray Vao { get; }
        public CheckersShader CheckersShader { get; }
        public TextureCacheModel TextureCache { get; }
        public PixelValueShader GetPixelShader { get; }
        public SrgbShader SrgbShader { get; }

        // statistics shader
        public MaxStatistics LinearMaxStatistics { get; }
        public MinStatistics LinearMinStatistics { get; }
        public AverageStatistics LinearAvgStatistics { get; }

        public MaxStatistics SrgbMaxStatistics { get; }
        public MinStatistics SrgbMinStatistics { get; }
        public AverageStatistics SrgbAvgStatistics { get; }

        private readonly Sampler samplerLinear;
        private readonly Sampler samplerLinearMip;
        private readonly Sampler samplerNearest;
        private readonly Sampler samplerNearestMip;

        public OpenGlModel(OpenGlContext context, ImagesModel images)
        {
            Debug.Assert(context.IsEnabled);
            Vao = new VertexArray();
            CheckersShader = new CheckersShader();
            samplerLinear = new Sampler(TextureMinFilter.Linear, TextureMagFilter.Linear);
            samplerLinearMip = new Sampler(TextureMinFilter.LinearMipmapLinear, TextureMagFilter.Linear);
            samplerNearest = new Sampler(TextureMinFilter.Nearest, TextureMagFilter.Nearest);
            samplerNearestMip = new Sampler(TextureMinFilter.NearestMipmapNearest, TextureMagFilter.Nearest);
            TextureCache = new TextureCacheModel(images, context);
            GetPixelShader = new PixelValueShader();
            SrgbShader = new SrgbShader();

            LinearMaxStatistics = new MaxStatistics(false);
            SrgbMaxStatistics = new MaxStatistics(true);
            LinearMinStatistics = new MinStatistics(false);
            SrgbMinStatistics = new MinStatistics(true);
            LinearAvgStatistics = new AverageStatistics(false, images);
            SrgbAvgStatistics = new AverageStatistics(true, images);
        }

        /// <summary>
        /// binds a specific sampler
        /// </summary>
        /// <param name="unit">sampler binding point</param>
        /// <param name="hasMipmaps">mipmap sampling enabled</param>
        /// <param name="linear">linear interpolation enabled</param>
        public void BindSampler(int unit, bool hasMipmaps, bool linear)
        {
            if (hasMipmaps)
            {
                if (linear)
                    samplerLinearMip.Bind(unit);
                else
                    samplerNearestMip.Bind(unit);
            }
            else
            {
                if (linear)
                    samplerLinear.Bind(unit);
                else
                    samplerNearest.Bind(unit);
            }
        }

        public void Dispose()
        {
            CheckersShader.Dispose();
            Vao.Dispose();
            samplerLinear.Dispose();
            samplerLinearMip.Dispose();
            samplerNearest.Dispose();
            samplerNearestMip.Dispose();
            TextureCache.Clear();
            GetPixelShader.Dispose();
            SrgbShader.Dispose();

            LinearMaxStatistics.Dispose();
            SrgbMaxStatistics.Dispose();
            LinearMinStatistics.Dispose();
            SrgbMinStatistics.Dispose();
        }
    }
}
