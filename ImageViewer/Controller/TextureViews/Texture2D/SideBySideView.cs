using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using ImageFramework.DirectX;
using ImageFramework.Model.Shader;
using ImageFramework.Utility;
using ImageViewer.Controller.TextureViews.Shared;
using ImageViewer.Models;
using ImageViewer.UtilityEx;
using SharpDX;
using SharpDX.DirectWrite;
using Color = ImageFramework.Utility.Color;
using Colors = ImageFramework.Utility.Colors;
using Matrix = SharpDX.Matrix;
using Size2 = ImageFramework.Utility.Size2;

namespace ImageViewer.Controller.TextureViews.Texture2D
{
    public class SideBySideView : PlainTextureView
    {
        public SideBySideView(ModelsEx models)
            : base(models, ShaderBuilder.Builder2D)
        {
            // conditionally enabled other equations when switching to this view
            if (models.NumEnabled < 2)
            {
                foreach(var pipe in models.Pipelines)
                {
                    if (pipe.IsValid) pipe.IsEnabled = true;
                }
            }
        }

        public override void Draw(int id, ITexture texture)
        {
            if (texture == null) return;

            

            var enabled = models.GetEnabledPipelines();
            if(enabled.Count == 0) return;

            var dev = Device.Get();
            var size = new Size2(models.Window.SwapChain.Width, models.Window.SwapChain.Height);

            var texCount = enabled.Count;
            var texId = enabled.IndexOf(id);

            int xStart = (texId * size.Width) / texCount;
            int xEnd = ((texId + 1) * size.Width) / texCount;

            // overwrite viewport and scissors
            dev.Rasterizer.SetViewport(xStart, 0, xEnd-xStart, size.Height);
            dev.Rasterizer.SetScissorRectangle(0, 0, size.Width, size.Height);

            var lm = models.Display.ActiveLayerMipmap;
            DrawLayer(Matrix.Identity, texture.GetSrView(lm), models.Overlay.Overlay?.GetSrView(lm));

            // draw image name if requested
            if (models.Settings.ImageNameOverlay)
            {
                var imgId = Math.Max(0, models.Pipelines[id].GetFirstImageId());
                var imgName = models.Images.Images[imgId].Alias;

                using (var draw = models.Window.SwapChain.Draw.Begin())
                {
                    var padding = (float)(double)(Application.Current.Resources["DefaultBorderValue"]);
                    float fontSize = 12.0f;
                    float h = 2 * padding + fontSize;

                    var fontBrush = (Application.Current.Resources["FontBrush"] as SolidColorBrush).ToFramework().ToSrgb();
                    var bgBrush = (Application.Current.Resources["BackgroundBrush"] as SolidColorBrush).ToFramework().ToSrgb();

                    draw.FillRectangle(new Float2(xStart, 0.0f), new Float2(xEnd, h), bgBrush);
                    draw.Text(new Float2(xStart + padding, padding), new Float2(xEnd - padding, h - padding), fontSize, fontBrush, imgName, SharpDX.DirectWrite.TextAlignment.Center);
                }
            }

            // reset view scissors
            dev.SetViewScissors(size.Width, size.Height);
        }

        public override bool CustomImageNameOverlay()
        {
            return true; // uses custom overlay
        }

        public override Size3? GetTexelPosition(Vector2 mouse)
        {
            var numViews = models.NumEnabled;
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
            var numViews = models.NumEnabled;
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
