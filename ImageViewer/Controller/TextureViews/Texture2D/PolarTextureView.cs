using System;
using ImageFramework.DirectX;
using ImageFramework.Utility;
using ImageViewer.Controller.TextureViews.Shader;
using ImageViewer.Controller.TextureViews.Shared;
using ImageViewer.Models;
using SharpDX;

namespace ImageViewer.Controller.TextureViews.Texture2D
{
    public class PolarTextureView : ProjectionTextureView
    {
        private readonly PolarViewShader shader;

        public PolarTextureView(ModelsEx models)
        : base(models)
        {
            shader = new PolarViewShader(models);
        }

        public override void Dispose()
        {
            shader?.Dispose();
            base.Dispose();
        }

        public override void Draw(ITexture texture)
        {
            if (texture == null) return;

            base.Draw(texture);

            var dev = Device.Get();
            dev.OutputMerger.BlendState = models.ViewData.AlphaBlendState;

            shader.Run(GetTransform(), CalcFarplane(), texture.GetSrView(models.Display.ActiveLayer, models.Display.ActiveMipmap));

            dev.OutputMerger.BlendState = models.ViewData.DefaultBlendState;
        }

        public override Size3 GetTexelPosition(Vector2 mouse)
        {
            // calculate farplane
            var viewDir = new Vector4(mouse.X, -mouse.Y, CalcFarplane(), 0.0f);
            var trans = GetTransform();
            Vector4.Transform(ref viewDir, ref trans, out var transformedViewDir);
            viewDir = transformedViewDir;
            viewDir.Normalize();
            viewDir.Y *= -1.0f;

            // determine pixel coordinate from view dir
            var polarDirection = new Vector2();
            // t computation
            polarDirection.Y = (float)(Math.Acos(viewDir.Y) / Math.PI);

            //  computation
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            polarDirection.X = viewDir.X == 0.0 ? (float)(Math.PI / 2 * Math.Sign(viewDir.Z)) : (float)(Math.Atan2(viewDir.Z, viewDir.X));
            polarDirection.X = (float)(polarDirection.X / (2.0 * Math.PI) + 0.25);

            if (polarDirection.X < 0.0)
                polarDirection.X += 1.0f;
            if (polarDirection.Y < 0.0)
                polarDirection.Y += 1.0f;

            var canonical = (polarDirection * 2.0f) - Vector2.One;
            var pt = Utility.CanonicalToTexelCoordinates(canonical.X, canonical.Y,
                models.Images.GetWidth(models.Display.ActiveMipmap),
                models.Images.GetHeight(models.Display.ActiveMipmap));

            return new Size3(pt.X, pt.Y, 0);
        }

        protected override Matrix GetOrientation()
        {
            return Matrix.Scaling(-1.0f, -1.0f, 1.0f);
        }
    }
}
