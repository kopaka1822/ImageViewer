using System;
using ImageFramework.DirectX;
using ImageFramework.Utility;
using ImageViewer.Controller.TextureViews.Shader;
using ImageViewer.Controller.TextureViews.Shared;
using ImageViewer.Models;
using SharpDX;

namespace ImageViewer.Controller.TextureViews.Texture2D
{
    public class CubeTextureView : PolarTextureView
    {
        private readonly CubeViewShader shader;

        public CubeTextureView(ModelsEx models)
            : base(models)
        {
            shader = new CubeViewShader(models);
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

            shader.Run(GetTransform(), CalcFarplane(), ((TextureArray2D)texture).GetCubeView(models.Display.ActiveMipmap));

            dev.OutputMerger.BlendState = models.ViewData.DefaultBlendState;
        }

        public override Size3 GetTexelPosition(Vector2 mouse)
        {
            // left handed coordinate system
            var transform = GetTransform();
            var preViewDir = new Vector4(mouse.X, -mouse.Y, CalcFarplane(), 0.0f);
            Vector4.Transform(ref preViewDir, ref transform, out var viewDir);

            viewDir.Normalize();

            // according to dx spec: https://microsoft.github.io/DirectX-Specs/d3d/archive/D3D11_3_FunctionalSpec.htm#PointSampling
            // Choose the largest magnitude component of the input vector. Call this magnitude of this value AxisMajor. In the case of a tie, the following precedence should occur: Z, Y, X. 
            int axisMajor = 0;
            int axisFlip = 0;
            float axisMajorValue = 0.0f;
            for (int i = 0; i < 3; ++i)
            {
                if (Math.Abs(viewDir[i]) >= axisMajorValue)
                {
                    axisMajor = i;
                    axisFlip = viewDir[i] < 0.0f ? 1 : 0;
                    axisMajorValue = Math.Abs(viewDir[i]);
                }
            }

            int faceId = axisMajor * 2 + axisFlip;

            // Select and mirror the minor axes as defined by the TextureCube coordinate space. Call this new 2d coordinate Position.
            // Project the coordinate onto the cube by dividing the components Position by AxisMajor. 
            int axisMinor1 = axisMajor == 0 ? 2 : 0; // first coord is x or z
            int axisMinor2 = 3 - axisMajor - axisMinor1;
            float u = viewDir[axisMinor1] / axisMajorValue;
            float v = -viewDir[axisMinor2] / axisMajorValue;

            switch (faceId)
            {
                case 0:
                case 5:
                    u *= -1.0f;
                    break;
                case 2:
                    v *= -1.0f;
                    break;
            }

            models.Display.ActiveLayer = faceId;

            var pt = Utility.CanonicalToTexelCoordinates(u, v,
                models.Images.GetWidth(models.Display.ActiveMipmap),
                models.Images.GetHeight(models.Display.ActiveMipmap));

            return new Size3(pt.X, pt.Y, 0);
        }

        protected override Matrix GetOrientation()
        {
            return Matrix.Scaling(1.0f);
        }
    }
}
