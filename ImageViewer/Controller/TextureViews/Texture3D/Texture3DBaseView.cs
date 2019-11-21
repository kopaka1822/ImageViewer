using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageFramework.Utility;
using ImageViewer.Controller.TextureViews.Shared;
using ImageViewer.Models;
using SharpDX;

namespace ImageViewer.Controller.TextureViews.Texture3D
{
    public abstract class Texture3DBaseView : ProjectionTextureView
    {
        protected float cubeOffsetX = 0.0f;
        protected float cubeOffsetY = 0.0f;

        public Texture3DBaseView(ModelsEx models, TextureViewData data)
            : base(models, data)
        {
            
        }

        public override Size3 GetTexelPosition(Vector2 mouse)
        {
            return Size3.Zero;
        }

        public override void OnScroll(float amount, Vector2 mouse)
        {
            // modify zoom
            var step = amount < 0.0f ? 1.0f / 1.001f : 1.001f;
            var value = (float)Math.Pow(step, Math.Abs(amount));

            models.Display.Zoom = models.Display.Zoom * value;
        }

        public override void OnDrag2(Vector2 diff)
        {
            float aspectX = models.Window.ClientSize.Width / (float)models.Window.ClientSize.Height;
            cubeOffsetX += 4.0f * diff.X / models.Display.Zoom / models.Window.ClientSize.Width * aspectX;
            cubeOffsetY -= 4.0f * diff.Y / models.Display.Zoom / models.Window.ClientSize.Height;
        }


        protected float GetCubeCenter()
        {
            return 1.0f / models.Display.Zoom * 2.0f;
        }

        protected override Matrix GetOrientation()
        {
            return Matrix.Identity;
        }
    }
}
