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

        public override void UpdateMouseDisplay(MainWindow window)
        {
            var transMouse = GetOpenGLMouseCoordinates(window);
            
            // on which layer are the mouse coordinates?
            // layer 4 is between [-1, 1]

            // clamp mouse coordinates between -1 and 1 + set the layer
            if (transMouse.X < -1.0f)
            {
                // layer 1
                Context.ActiveLayer = 1;
                transMouse.X += 2.0f;
            }
            else if (transMouse.X > 1.0f)
            {
                // layer 0 or 5
                if (transMouse.X > 3.0f)
                {
                    // layer 5
                    Context.ActiveLayer = 5;
                    transMouse.X -= 4.0f;
                }
                else
                {
                    Context.ActiveLayer = 0;
                    transMouse.X -= 2.0f;
                }
            }
            else
            {
                // layer 2, 3 or 4
                if (transMouse.Y > 1.0f)
                {
                    // layer 3
                    Context.ActiveLayer = 3;
                    transMouse.Y -= 2.0f;
                }
                else if(transMouse.Y < -1.0f)
                {
                    // layer 2
                    Context.ActiveLayer = 2;
                    transMouse.Y += 2.0f;
                }
                else
                {
                    Context.ActiveLayer = 4;
                }
            }
            
            // get clamped texture results
            transMouse = MouseToTextureCoordinates(transMouse,
                Context.GetWidth((int)Context.ActiveMipmap),
                Context.GetHeight((int)Context.ActiveMipmap));

            window.StatusBar.SetMouseCoordinates((int)(transMouse.X), (int)(transMouse.Y));
        }
    }
}
