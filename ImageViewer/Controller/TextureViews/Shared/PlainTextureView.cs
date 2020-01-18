using System;
using ImageFramework.DirectX;
using ImageFramework.Model.Shader;
using ImageFramework.Utility;
using ImageViewer.Controller.TextureViews.Shader;
using ImageViewer.Models;
using SharpDX;
using SharpDX.Direct3D11;

namespace ImageViewer.Controller.TextureViews.Shared
{
    public abstract class PlainTextureView : ITextureView
    {
        protected readonly ModelsEx models;
        private Vector3 translation = Vector3.Zero;
        private readonly SingleViewShader shader;

        public PlainTextureView(ModelsEx models, IShaderBuilder builder)
        {
            this.models = models;
            shader = new SingleViewShader(models, builder);
        }

        public void Dispose()
        {
            shader.Dispose();
        }

        public abstract void Draw(ITexture texture);

        public void OnScroll(float amount, Vector2 mouse)
        {
            // modify zoom
            var oldZoom = models.Display.Zoom;

            if (amount < 0.0f)
                models.Display.DecreaseZoom();
            else
                models.Display.IncreaseZoom();

            // do this because zoom is clamped and may not have changed at all
            var value = models.Display.Zoom / oldZoom;
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

        public void OnDrag2(Vector2 diff)
        {
            
        }

        public abstract Size3 GetTexelPosition(Vector2 mouse);
        public void UpdateImage(int id, ITexture texture)
        {
            
        }

        private Matrix GetTransform()
        {
            return Matrix.Scaling(models.Display.Zoom, models.Display.Zoom, 1.0f) *
                   Matrix.Translation(translation) * GetImageAspectRatio();
        }

        protected virtual Matrix GetImageAspectRatio()
        {
            return models.Display.ImageAspectRatio;
        }

        /// <summary>
        /// transforms mouse coordinates into directX space
        /// </summary>
        /// <param name="mouse">canonical mouse coordinates</param>
        /// <returns>vector with correct x and y coordinates</returns>
        protected Vector2 GetDirectXMouseCoordinates(Vector2 mouse)
        {
            // Matrix Coordinate system is reversed (left handed)
            var vec = new Vector4(mouse.X, mouse.Y, 0.0f, 1.0f);
            var trans = GetTransform() * GetOrientation();
            trans.Invert();

            Vector4.Transform(ref vec, ref trans, out var res);

            return new Vector2(res.X, -res.Y);
        }

        private Matrix GetOrientation()
        {
            return Matrix.Scaling(1.0f, -1.0f, 1.0f);
        }

        protected void DrawLayer(Matrix offset, int layer, ShaderResourceView texture,
            int xaxis = 0, int yaxis = 1, int zvalue = 1)
        {
            var dev = ImageFramework.DirectX.Device.Get();
            var finalTransform = offset * GetTransform();
            
            // draw the checkers background
            models.ViewData.Checkers.Run(finalTransform);
            
            // blend over the final image
            dev.OutputMerger.BlendState = models.ViewData.AlphaBlendState;

            shader.Run(finalTransform, texture, layer, xaxis, yaxis, zvalue);

            dev.OutputMerger.BlendState = models.ViewData.DefaultBlendState;
        }
    }
}
