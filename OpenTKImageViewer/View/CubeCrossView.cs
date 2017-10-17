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
            // -x
            DrawLayer(Matrix4.CreateTranslation(-2.0f, 0.0f, 0.0f), 1);
            // +y
            DrawLayer(Matrix4.CreateTranslation(0.0f, 2.0f, 0.0f), 3);
            // -y
            DrawLayer(Matrix4.CreateTranslation(0.0f, -2.0f, 0.0f), 2);
            // +z
            DrawLayer(Matrix4.CreateTranslation(0.0f, 0.0f, 0.0f), 4);
            // +x
            DrawLayer(Matrix4.CreateTranslation(2.0f, 0.0f, 0.0f), 0);
            // -z
            DrawLayer(Matrix4.CreateTranslation(4.0f, 0.0f, 0.0f), 5);
        }
    }
}
