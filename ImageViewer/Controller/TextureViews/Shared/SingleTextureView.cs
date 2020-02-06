using ImageFramework.DirectX;
using ImageFramework.Model.Shader;
using ImageFramework.Utility;
using ImageViewer.Models;
using SharpDX;

namespace ImageViewer.Controller.TextureViews.Shared
{
    public class SingleTextureView : PlainTextureView
    {
        public SingleTextureView(ModelsEx models)
        : base(models, ShaderBuilder.Builder2D)
        {
        }

        public override void Draw(int id, ITexture texture)
        {
            if (texture == null) return;

            DrawLayer(Matrix.Identity, 
                texture.GetSrView(models.Display.ActiveLayer, models.Display.ActiveMipmap),
                       models.Overlay.Overlay?.GetSrView(models.Display.ActiveLayer, models.Display.ActiveMipmap));
        }

        public override Size3 GetTexelPosition(Vector2 mouse)
        {
            var transMouse = GetDirectXMouseCoordinates(mouse);

            var pt = Utility.CanonicalToTexelCoordinates(transMouse.X, transMouse.Y,
                models.Images.GetWidth(models.Display.ActiveMipmap),
                models.Images.GetHeight(models.Display.ActiveMipmap));

            return new Size3(pt.X, pt.Y, 0);
        }
    }
}
