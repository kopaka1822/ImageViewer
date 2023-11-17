using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageFramework.DirectX;
using ImageFramework.Model.Shader;
using ImageFramework.Utility;
using ImageViewer.Controller.TextureViews.Shared;
using ImageViewer.Models;
using SharpDX;
using Size2 = ImageFramework.Utility.Size2;

namespace ImageViewer.Controller.TextureViews.Texture2D
{
    public class SideBySideView : PlainTextureView
    {
        public SideBySideView(ModelsEx models) 
            : base(models, ShaderBuilder.Builder2D)
        {}

        public override void Draw(int id, ITexture texture)
        {
            if (texture == null) return;

            var dev = Device.Get();
            var size = new Size2(models.Window.SwapChain.Width, models.Window.SwapChain.Height);
            // overwrite viewport
            

            var enabled = models.GetEnabledPipelines();
            if(enabled.Count == 0) return;

            var texCount = enabled.Count;
            var texId = enabled.IndexOf(id);


            int xStart = (texId * size.Width) / texCount;
            int xEnd = ((texId + 1) * size.Width) / texCount;

            // overwrite viewport and scissors
            dev.Rasterizer.SetViewport(xStart, 0, xEnd-xStart, size.Height);
            dev.Rasterizer.SetScissorRectangle(0, 0, size.Width, size.Height);

            var lm = models.Display.ActiveLayerMipmap;
            DrawLayer(Matrix.Identity, texture.GetSrView(lm), models.Overlay.Overlay?.GetSrView(lm));


            // reset view scissors
            dev.SetViewScissors(size.Width, size.Height);
        }

        public override Size3? GetTexelPosition(Vector2 mouse)
        {
            var numViews = models.GetEnabledPipelines().Count;
            numViews = Math.Max(numViews, 1);

            // mouse is in [-1, 1] coordinates
            var xnorm = mouse.X * 0.5f + 0.5f; // [-1, 1] => [0, 1]
            xnorm = xnorm * numViews; // [0, numViews]
            xnorm = xnorm - (int)xnorm; // [0, 1]
            mouse.X = xnorm * 2.0f - 1.0f; // [0, 1] => [-1, 1]

            var transMouse = GetDirectXMouseCoordinates(mouse);

            var pt = Utility.CanonicalToTexelCoordinates(transMouse.X, transMouse.Y,
                models.Images.GetWidth(models.Display.ActiveMipmap),
                models.Images.GetHeight(models.Display.ActiveMipmap));

            return new Size3(pt.X, pt.Y, 0);
        }

        protected override Matrix GetImageAspectRatio()
        {
            var numViews = models.GetEnabledPipelines().Count;
            numViews = Math.Max(numViews, 1);
            var imgDim = models.Images.Size;
            var clientDim = models.Window.ClientSize;

            return Matrix.Scaling(
                numViews * imgDim.Width / (float)clientDim.Width,
                imgDim.Height / (float)clientDim.Height,
                1.0f
            );
        }
    }
}
