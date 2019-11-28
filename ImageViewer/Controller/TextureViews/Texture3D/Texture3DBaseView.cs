using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageFramework.DirectX;
using ImageFramework.Utility;
using ImageViewer.Controller.TextureViews.Shared;
using ImageViewer.Models;
using SharpDX;

namespace ImageViewer.Controller.TextureViews.Texture3D
{
    public abstract class Texture3DBaseView : ITextureView
    {
        protected readonly ModelsEx models;
        protected readonly TextureViewData data;
        protected float cubeOffsetX = 0.0f;
        protected float cubeOffsetY = 0.0f;
        private float pitch = 0.0f;
        private float roll = 0.0f;


        public Texture3DBaseView(ModelsEx models, TextureViewData data)
        {
            this.models = models;
            this.data = data;
        }


        public virtual void Draw(ITexture texture)
        {
            // draw transparent background
            data.Checkers.Run(data.Buffer, Matrix.Identity, models.Settings.AlphaBackground);
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
            pitch += (float)diff.X * 0.01f * models.Display.Aperture;
            roll += (float)diff.Y * 0.01f * models.Display.Aperture;
        }

        protected Matrix GetRotation()
        {
            return
                Matrix.RotationX(roll) *
                Matrix.RotationY(pitch);
        }

        protected float CalcFarplane()
        {
            // distance from camera. ray direction in the edges will be vec3(+-1, +-1, farplane)
            return (float)(1.0 / Math.Tan(models.Display.Aperture / 2.0));
        }

        public void OnDrag2(Vector2 diff)
        {
            float aspectX = models.Window.ClientSize.Width / (float)models.Window.ClientSize.Height;
            cubeOffsetX += 4.0f * diff.X / models.Display.Zoom / models.Window.ClientSize.Width * aspectX;
            cubeOffsetY -= 4.0f * diff.Y / models.Display.Zoom / models.Window.ClientSize.Height;
        }

        public abstract Size3 GetTexelPosition(Vector2 mouse);

        protected float GetCubeCenter()
        {
            return 1.0f / models.Display.Zoom * 2.0f;
        }

        public abstract void Dispose();
    }
}
