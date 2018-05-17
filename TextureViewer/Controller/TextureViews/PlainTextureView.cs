using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        private readonly SingleViewShader shader;

        protected PlainTextureView(Models.Models models)
        {
            this.models = models;
            shader = new SingleViewShader();
        }

        public virtual void Draw(TextureArray2D texture)
        {
        }

        public void Dispose()
        {
            shader.Dispose();
        }

        public void OnScroll(float amount, Vector2 mouse)
        {
            // modify zoom
            var step = amount < 0.0f ? 1.0f / 1.001f : 1.001f;
            var value = (float)Math.Pow(step, Math.Abs(amount));

            models.Display.Zoom = models.Display.Zoom * value;
        }

        public void OnDrag(Vector2 diff)
        {
            // window to client
            transform *= Matrix4.CreateTranslation(
                diff.X * 2.0f / models.Images.GetWidth(0),
                diff.Y * -2.0f / models.Images.GetHeight(0),
                0.0f);
        }

        protected void DrawLayer(Matrix4 offset, int layer, TextureArray2D texture)
        {
            Debug.Assert(texture != null);

            var finalTransform = offset * 
                                  
                                 Matrix4.CreateScale(models.Display.Zoom, models.Display.Zoom, 1.0f) *
                                 transform *
                                 models.Display.AspectRatio;

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
            
            models.GlData.BindSampler(0, false, models.Display.LinearInterpolation);
            texture.Bind(0);

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
