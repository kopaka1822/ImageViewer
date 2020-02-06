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
    public class ShearWarpView : Texture3DBaseView
    {
        private readonly ShearWarpShader shader;

        public ShearWarpView(ModelsEx models) : base(models)
        {
            shader = new ShearWarpShader(models);
            
        }

        public override Size3 GetTexelPosition(Vector2 mouse)
        {
            return new Size3(0, 0, 0);
        }

        public override void Dispose()
        {
            shader.Dispose();
        }

        public override void Draw(int id, ITexture texture)
        {
            if (texture == null) return;

            base.Draw(id, texture);

            var dev = Device.Get();
            dev.OutputMerger.BlendState = models.ViewData.AlphaBlendState;

            var projection = Matrix.PerspectiveFovLH(1.57f, models.Window.ClientSize.Width / (float)models.Window.ClientSize.Height, 0.01f, 10000.0f);

            shader.Run(models.ViewData.Buffer, projection, GetModel(), models.Display.Multiplier, models.Display.DisplayNegative,
                texture.GetSrView(models.Display.ActiveLayer, models.Display.ActiveMipmap),
                models.ViewData.GetSampler(models.Display.LinearInterpolation), models.Images.Size.GetMip(models.Display.ActiveMipmap));

            dev.OutputMerger.BlendState = models.ViewData.DefaultBlendState;
        }

        private Matrix GetModel()
        {
            return
                GetRotation() * // rotate in origin
                GetOrientation() *
                Matrix.Translation(cubeOffsetX, cubeOffsetY, GetCubeCenter()); // move to correct position
        }

        private Matrix GetOrientation()
        {
            return Matrix.Scaling(-1.0f, -1.0f, 1.0f);
        }
    }
}
