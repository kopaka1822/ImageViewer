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
        protected float cubeOffsetX = 0.0f;
        protected float cubeOffsetY = 0.0f;
        private Matrix rotation = Matrix.Identity;
        float yaw = 0.0f;
        float pitch = 0.0f;


        public Texture3DBaseView(ModelsEx models)
        {
            this.models = models;
        }


        public virtual void Draw(int id, ITexture texture)
        {
            // draw transparent background
            models.ViewData.Checkers.Run(Matrix.Identity);
        }

        public void OnScroll(float amount, Vector2 mouse)
        {
            // modify zoom
            if (amount < 0.0f)
                models.Display.DecreaseZoom();
            else
                models.Display.IncreaseZoom();
        }

        public void OnDrag(Vector2 diff)
        {
            yaw += (float)diff.X * 0.01f * models.Display.Aperture;
            pitch += (float)diff.Y * 0.01f * models.Display.Aperture;
            rotation = Matrix.RotationYawPitchRoll(yaw, pitch, 0.0f);
        }

        protected Matrix GetRotation()
        {
            return rotation;
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
        public virtual void UpdateImage(int id, ITexture texture) {}

        protected float GetCubeCenter()
        {
            return 1.0f / models.Display.Zoom * 2.0f;
        }

        public abstract void Dispose();
    }
}
