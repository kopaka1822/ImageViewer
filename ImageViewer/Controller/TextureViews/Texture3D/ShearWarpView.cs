using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageFramework.DirectX;
using ImageViewer.Controller.TextureViews.Shader;
using ImageViewer.Controller.TextureViews.Shared;
using ImageViewer.Models;
using SharpDX;

namespace ImageViewer.Controller.TextureViews.Texture3D
{
    public class ShearWarpView : Texture3DBaseView
    {
        private readonly ShearWarpShader shader;

        public ShearWarpView(ModelsEx models, TextureViewData data) : base(models, data)
        {
            shader = new ShearWarpShader();
            
        }

        public override void Dispose()
        {
            shader.Dispose();
            base.Dispose();
        }

        public override void Draw(ITexture texture)
        {
            if (texture == null) return;

            base.Draw(texture);

            var dev = Device.Get();
            dev.OutputMerger.BlendState = data.AlphaBlendState;

            var projection = Matrix.PerspectiveFovLH(1.57f, models.Window.ClientSize.Width / (float)models.Window.ClientSize.Height, 0.01f, 10000.0f);

            shader.Run(data.Buffer, projection, GetModel(), models.Display.Multiplier, models.Display.DisplayNegative,
                texture.GetSrView(models.Display.ActiveLayer, models.Display.ActiveMipmap), 
                data.GetSampler(models.Display.LinearInterpolation), models.Images.Size.GetMip(models.Display.ActiveMipmap));

            dev.OutputMerger.BlendState = data.DefaultBlendState;
        }

        private Matrix GetModel()
        {
            return
                GetRotation() * // rotate in origin
                GetOrientation() *
                Matrix.Translation(cubeOffsetX, cubeOffsetY, GetCubeCenter()); // move to correct position
        }

        protected override Matrix GetOrientation()
        {
            return Matrix.Scaling(-1.0f, -1.0f, 1.0f);
        }
    }
}
