using ImageFramework.DirectX;
using ImageFramework.Model.Shader;
using ImageFramework.Utility;
using ImageViewer.Controller.TextureViews.Shared;
using ImageViewer.Models;
using ImageViewer.Models.Display;
using SharpDX;

namespace ImageViewer.Controller.TextureViews.Texture3D
{
    public class Single3DView : PlainTextureView
    {
        private Single3DDisplayModel displayEx;

        public Single3DView(ModelsEx models) : 
            base(models, ShaderBuilder.Builder3D)
        {
            displayEx = (Single3DDisplayModel)models.Display.ExtendedViewData;
        }

        public override void Draw(ITexture texture)
        {
            if (texture == null) return;

            DrawLayer(Matrix.Identity,
                texture.GetSrView(models.Display.ActiveLayer, models.Display.ActiveMipmap),
                models.Overlay.Overlay?.GetSrView(models.Display.ActiveLayer, models.Display.ActiveMipmap),
                displayEx.FreeAxis1, displayEx.FreeAxis2, displayEx.FixedAxisSlice);
        }

        public override Size3 GetTexelPosition(Vector2 mouse)
        {
            var transMouse = GetDirectXMouseCoordinates(mouse);

            var dim = models.Images.Size.GetMip(models.Display.ActiveMipmap);
            var pt = Utility.CanonicalToTexelCoordinates(transMouse.X, transMouse.Y,
                dim[displayEx.FreeAxis1],
                dim[displayEx.FreeAxis2]);

            Size3 res = Size3.Zero;
            res[displayEx.FreeAxis1] = pt.X;
            res[displayEx.FreeAxis2] = pt.Y;
            res[displayEx.FixedAxis] = displayEx.FixedAxisSlice;

            return res;
        }

        protected override Matrix GetImageAspectRatio()
        {
            var dim = models.Images.Size;
            return Matrix.Scaling(
                    dim[displayEx.FreeAxis1] / (float)models.Window.ClientSize.Width,
                    dim[displayEx.FreeAxis2] / (float)models.Window.ClientSize.Height,
                    1.0f);

        }
    }
}
