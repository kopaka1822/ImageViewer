using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
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

        private Vector3 translation = Vector3.Zero;
        private readonly SingleViewShader shader;

        protected PlainTextureView(Models.Models models)
        {
            this.models = models;
            shader = new SingleViewShader();
        }

        public virtual void Draw(TextureArray2D texture)
        {
            throw new NotImplementedException();
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

            var oldZoom = models.Display.Zoom;


            models.Display.Zoom = models.Display.Zoom * value;

            // do this because zoom is clamped and may not have changed at all
            value = models.Display.Zoom / oldZoom;
            // modify translation as well
            translation.X *= value;
            translation.Y *= value;
        }

        public void OnDrag(Vector2 diff)
        {
            // window to client
            translation.X += diff.X * 2.0f / models.Images.GetWidth(0);
            translation.Y -= diff.Y * 2.0f / models.Images.GetHeight(0);
        }

        public virtual Point GetTexelPosition(Vector2 mouse)
        {
            throw new NotImplementedException();
        }

        private Matrix4 GetTransform()
        {
            return Matrix4.CreateScale(models.Display.Zoom, models.Display.Zoom, 1.0f) *
                   Matrix4.CreateTranslation(translation) *
                   models.Display.ImageAspectRatio;
        }

        protected void DrawLayer(Matrix4 offset, int layer, TextureArray2D texture)
        {
            Debug.Assert(texture != null);

            var finalTransform = offset * GetTransform();

            // draw the checkers background
            models.GlData.CheckersShader.Bind(finalTransform);
            models.GlData.Vao.DrawQuad();

            // blend over the final image
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);

            shader.Bind();
            shader.SetTransform(finalTransform);
            shader.SetLayer(layer);
            shader.SetMipmap(models.Display.ActiveMipmap);
            shader.SetGrayscale(models.Display.Grayscale);
            shader.SetCrop(models.Export);

            models.GlData.BindSampler(shader.GetTextureLocation(), true, models.Display.LinearInterpolation);
            texture.Bind(shader.GetTextureLocation());

            models.GlData.Vao.DrawQuad();

            // disable everything
            GL.Disable(EnableCap.Blend);
            Program.Unbind();
        }

        /// <summary>
        /// transforms mouse coordinates into opengl space
        /// </summary>
        /// <param name="mouse">canonical mouse coordinates</param>
        /// <returns>vector with correct x and y coordinates</returns>
        protected Vector2 GetOpenGlMouseCoordinates(Vector2 mouse)
        {
            // Matrix Coordinate system is reversed (left handed)
            var vec = new Vector4((float)mouse.X, (float)mouse.Y, 0.0f, 1.0f);
            vec = vec * (GetOrientation() * GetTransform()).Inverted();
            return new Vector2(vec.X, vec.Y);
        }

        private Matrix4 GetOrientation()
        {
            return Matrix4.CreateScale(1.0f, -1.0f, 1.0f);
        }
    }
}
