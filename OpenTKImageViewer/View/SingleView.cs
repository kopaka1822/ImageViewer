using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using OpenTK;
using OpenTK.Graphics.OpenGL4;
using OpenTKImageViewer.glhelper;
using OpenTKImageViewer.UI;
using OpenTKImageViewer.View.Shader;

namespace OpenTKImageViewer.View
{
    public class SingleView : PlainView
    {
        public SingleView(ImageContext.ImageContext context, TextBox boxScroll)
            : base(context, boxScroll, StatusBarControl.LayerModeType.Single)
        {
        }

        public override void Draw()
        {
            DrawLayer(Matrix4.Identity, Context.ActiveLayer);
        }
    }
}
