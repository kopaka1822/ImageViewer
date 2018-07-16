using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using TextureViewer.glhelper;
using TextureViewer.Models;

namespace TextureViewer.Controller.TextureViews
{
    class SingleTextureView : PlainTextureView
    {
        public SingleTextureView(Models.Models models) : base(models)
        {
        }

        public override void Draw(TextureArray2D texture)
        {
            DrawLayer(Matrix4.Identity, models.Display.ActiveLayer, texture);
        }

        public override Point GetTexelPosition(Vector2 mouse)
        {
            var transMouse = GetOpenGlMouseCoordinates(mouse);
            return Utility.Utility.CanonicalToTexelCoordinates(transMouse,
                models.Images.GetWidth(models.Display.ActiveMipmap),
                models.Images.GetHeight(models.Display.ActiveMipmap));
        }
    }
}
