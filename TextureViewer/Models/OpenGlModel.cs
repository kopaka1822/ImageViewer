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

namespace TextureViewer.Models
{
    /// <summary>
    /// opengl related data used for texture views
    /// </summary>
    public class OpenGlModel
    {
        public VertexArray Vao { get; }
        public CheckersShader CheckersShader { get; }

        private readonly Sampler samplerLinear;
        private readonly Sampler samplerLinearMip;
        private readonly Sampler samplerNearest;
        private readonly Sampler samplerNearestMip;

        public OpenGlModel(OpenGlController controller)
        {
            Debug.Assert(controller.IsEnabled);
            Vao = new VertexArray();
            CheckersShader = new CheckersShader();
            samplerLinear = new Sampler(TextureMinFilter.Linear, TextureMagFilter.Linear);
            samplerLinearMip = new Sampler(TextureMinFilter.LinearMipmapLinear, TextureMagFilter.Linear);
            samplerNearest = new Sampler(TextureMinFilter.Nearest, TextureMagFilter.Nearest);
            samplerNearestMip = new Sampler(TextureMinFilter.NearestMipmapNearest, TextureMagFilter.Nearest);
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
        }
    }
}
