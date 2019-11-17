using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageFramework.DirectX;
using ImageFramework.Model.Shader;
using ImageFramework.Utility;
using ImageViewer.Models;
using ImageViewer.Models.Display;
using SharpDX;
using Point = System.Drawing.Point;

namespace ImageViewer.Controller.TextureViews
{
    public class Single3DView : PlainTextureView
    {
        private Single3DDisplayModel displayEx;

        public Single3DView(ModelsEx models, TextureViewData data) : 
            base(models, data, ShaderBuilder.Builder3D)
        {
            displayEx = (Single3DDisplayModel)models.Display.ExtendedViewData;
        }

        public override void Draw(ITexture texture)
        {
            if (texture == null) return;

            DrawLayer(Matrix.Identity, models.Display.ActiveLayer, 
                texture.GetSrView(models.Display.ActiveLayer, models.Display.ActiveMipmap),
                displayEx.FreeAxis1, displayEx.FreeAxis2, displayEx.FixedAxisSlice);
        }

        public override System.Drawing.Point GetTexelPosition(Vector2 mouse)
        {
            var transMouse = GetDirectXMouseCoordinates(mouse);

            var pt = Utility.CanonicalToTexelCoordinates(transMouse.X, transMouse.Y,
                models.Images.GetWidth(models.Display.ActiveMipmap),
                models.Images.GetHeight(models.Display.ActiveMipmap));

            return new Point(pt.X, pt.Y);
        }
    }
}
