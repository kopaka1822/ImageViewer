using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL4;
using TextureViewer.Controller.ImageCombination;
using TextureViewer.Models.Filter;
using TextureViewer.Models.Shader;

namespace TextureViewer.Controller.Filter
{
    /// <summary>
    /// common items for single invocation and multiple invocation stepper
    /// </summary>
    public class FilterStepableBase
    {
        protected FilterModel Model;
        private readonly int iteration;
        protected readonly ImageCombineBuilder Builder;
        private readonly Models.Models models;
        private readonly int layer;

        public FilterStepableBase(
            Models.Models models,
            FilterModel model, 
            ImageCombineBuilder builder, 
            int layer,
            int iteration)
        {
            Model = model;
            this.iteration = iteration;
            this.Builder = builder;
            this.models = models;
            this.layer = layer;
        }

        protected void BindProgramAndUniforms()
        {
            //create resources
            Builder.GetPrimaryTexture();
            Builder.GetTemporaryTexture();

            models.GlData.BindSampler(Model.Shader.GetSourceImageLocation(), false, true);
            Builder.GetPrimaryTexture().BindAsTexture2D(Model.Shader.GetSourceImageLocation(), layer: layer, mipmap: 0);
            Builder.GetTemporaryTexture()
                .BindAsImage(Model.Shader.GetDestinationImageLocation(), layer: layer, mipmap: 0, access: TextureAccess.WriteOnly);

            Model.Shader.Bind(models, layer, iteration);
        }

        /// <summary>
        /// converts amount of pixels into number of work groups (division through local size)
        /// </summary>
        /// <param name="pixels"></param>
        /// <returns></returns>
        protected static int GetNumWorkGroups(int pixels)
        {
            return pixels / FilterShader.LocalSize + (pixels % FilterShader.LocalSize != 0 ? 1 : 0);
        }

        /// <summary>
        /// converts amount of pixels into minimal number of shader invocations 
        /// (depending on LocalSize and MinWorkGroupSize)
        /// </summary>
        /// <param name="pixels"></param>
        /// <returns></returns>
        protected static int GetNumMinimalInvocations(int pixels)
        {
            return pixels / (FilterShader.LocalSize * FilterShader.MinWorkGroupSize) + (pixels % (FilterShader.LocalSize * FilterShader.MinWorkGroupSize) != 0 ? 1 : 0);
        }
    }
}
