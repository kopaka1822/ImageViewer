using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageFramework.DirectX;
using ImageFramework.Utility;
using ImageViewer.Controller.TextureViews.Shader;
using ImageViewer.Controller.TextureViews.Shared;
using ImageViewer.Models;
using SharpDX;

namespace ImageViewer.Controller.TextureViews.Texture3D
{
    public class RayCastingView : ProjectionTextureView
    {
        private readonly RayCastingShader shader;
        private float cubeOffsetX = 0.0f;
        private float cubeOffsetY = 0.0f;

        public RayCastingView(ModelsEx models, TextureViewData data) : base(models, data)
        {
            shader = new RayCastingShader();
        }

        public override Size3 GetTexelPosition(Vector2 mouse)
        {
            return Size3.Zero;
        }

        public override void Draw(ITexture texture)
        {
            if (texture == null) return;

            var src = new Vector4(0.0f, 0.0f, 0.0f, 1.0f);
            var mat = GetWorldToImage();
            Vector4.Transform(ref src, ref mat, out var res);

            shader.Run(data.Buffer, models.Display.ClientAspectRatio * GetOrientation(), GetWorldToImage(), models.Display.Multiplier, CalcFarplane(), models.Display.DisplayNegative, 
                texture.GetSrView(models.Display.ActiveLayer, models.Display.ActiveMipmap));
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

        protected override Matrix GetOrientation()
        {
            return Matrix.Identity;
        }

        private Matrix GetWorldToImage()
        {
            float aspectX = models.Images.Size.X / (float)models.Images.Size.Y;
            float aspectZ = models.Images.Size.Z / (float)models.Images.Size.Y;

            /*return
                Matrix.Translation(0.5f, 0.5f, 0.5f) * // move to [0, 1]
                Matrix.Scaling(0.5f * aspectX, 0.5f, 0.5f * aspectZ) * // scale to [-0.5, 0.5]
                //-GetRotation() * // undo rotation
                Matrix.Translation(-cubeOffsetX, -cubeOffsetY, -GetCubeCenter()); // translate cube center to origin*/

            return
                Matrix.Translation(-cubeOffsetX, -cubeOffsetY, -GetCubeCenter()) * // translate cube center to origin
                GetRotation() * // undo rotation
                Matrix.Scaling(0.5f * aspectX, 0.5f, 0.5f * aspectZ) * // scale to [-0.5, 0.5]
                Matrix.Translation(0.5f, 0.5f, 0.5f); // move to [0, 1]
        }

        private float GetCubeCenter()
        {
            return 1.0f / models.Display.Zoom * 2.0f;
        }

        public override void Dispose()
        {
            shader?.Dispose();
            base.Dispose();
        }
    }
}
