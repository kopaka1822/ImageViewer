using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Graphics.OpenGL4;
using TextureViewer.Controller.TextureViews.Shader;
using TextureViewer.glhelper;
using TextureViewer.Models;

namespace TextureViewer.Controller.TextureViews
{
    public class PlainTextureView : ITextureView
    {
        protected readonly Models.Models models;

        private Matrix4 transform = Matrix4.Identity;
        private SingleViewShader shader;

        public PlainTextureView(Models.Models models)
        {
            this.models = models;
            shader = new SingleViewShader();
        }

        public virtual void Draw()
        {
        }

        public void Dispose()
        {
            shader.Dispose();
        }

        public void DrawLayer(Matrix4 offset, int layer)
        {
            var finalTransform = offset * transform * models.Display.AspectRatio;

            // draw the checkers background
            models.GlData.CheckersShader.Bind(finalTransform);
            models.GlData.Vao.DrawQuad();

            // blend over the final image
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);

            shader.Bind();
            shader.SetTransform(finalTransform);
            shader.SetLayer(models.Display.ActiveLayer);
            shader.SetMipmap(models.Display.ActiveMipmap);
            shader.SetGrayscale(models.Display.Grayscale);
            
            // TODO bind the correct image
            models.GlData.BindSampler(0, false, models.Display.LinearInterpolation);
            models.Images.GetTexture(0).Bind(0);

            models.GlData.Vao.DrawQuad();

            // disable everything
            GL.Disable(EnableCap.Blend);
            Program.Unbind();
        }

        private Matrix4 GetOrientation()
        {
            return Matrix4.CreateScale(1.0f, -1.0f, 1.0f);
        }
    }
}
