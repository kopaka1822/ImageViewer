using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using OpenTK;
using OpenTKImageViewer.UI;
using OpenTKImageViewer.View.Shader;

namespace OpenTKImageViewer.View
{
    public class CubeCrossView : PlainView
    {
        public CubeCrossView(ImageContext.ImageContext context, TextBox boxScroll)
            : base(context, boxScroll, StatusBarControl.LayerModeType.SingleDeactivated)
        {

        }

        public override void Draw()
        {
            // back
            DrawLayer(Matrix4.CreateTranslation(-2.0f, 0.0f, 0.0f), 5);
            // left
            DrawLayer(Matrix4.Identity, 1);
            // up
            DrawLayer(Matrix4.CreateTranslation(2.0f, 2.0f, 0.0f), 3);
            // down
            DrawLayer(Matrix4.CreateTranslation(2.0f, -2.0f, 0.0f), 2);
            // front
            DrawLayer(Matrix4.CreateTranslation(2.0f, 0.0f, 0.0f), 4);
            // right
            DrawLayer(Matrix4.CreateTranslation(4.0f, 0.0f, 0.0f), 0);
        }
    }
}
