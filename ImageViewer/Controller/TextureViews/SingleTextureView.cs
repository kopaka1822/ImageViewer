using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageFramework.DirectX;
using ImageFramework.Utility;
using ImageViewer.Models;
using SharpDX;
using Point = System.Drawing.Point;

namespace ImageViewer.Controller.TextureViews
{
    public class SingleTextureView : PlainTextureView
    {
        public SingleTextureView(ModelsEx models, TextureViewData data)
        : base(models, data)
        {
        }

        public override void Draw(TextureArray2D texture)
        {
            if (texture == null) return;

            DrawLayer(Matrix.Identity, models.Display.ActiveLayer, texture.GetSrView(models.Display.ActiveLayer, models.Display.ActiveMipmap));
        }

        public override Point GetTexelPosition(Vector2 mouse)
        {
            var transMouse = GetDirectXMouseCoordinates(mouse);
            var pt = Utility.CanonicalToTexelCoordinates(transMouse.X, transMouse.Y,
                models.Images.GetWidth(models.Display.ActiveMipmap),
                models.Images.GetHeight(models.Display.ActiveMipmap));

            return new Point(pt.X, pt.Y);
        }
    }
}
