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

            shader.Run(data.Buffer, GetTransform(), models.Display.Multiplier, CalcFarplane(), models.Display.DisplayNegative, 
                texture.GetSrView(models.Display.ActiveLayer, models.Display.ActiveMipmap), data.GetSampler(models.Display.LinearInterpolation));
        }

        protected override Matrix GetOrientation()
        {
            return Matrix.Identity;
        }

        public override void Dispose()
        {
            shader?.Dispose();
            base.Dispose();
        }
    }
}
