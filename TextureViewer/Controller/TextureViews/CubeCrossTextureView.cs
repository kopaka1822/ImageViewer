using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using TextureViewer.glhelper;

namespace TextureViewer.Controller.TextureViews
{
    public class CubeCrossTextureView : PlainTextureView
    {
        public CubeCrossTextureView(Models.Models models) : base(models)
        {

        }

        public override void Draw(TextureArray2D texture)
        {
            // -x
            DrawLayer(Matrix4.CreateTranslation(-2.0f, 0.0f, 0.0f), 1, texture);
            // +y
            DrawLayer(Matrix4.CreateTranslation(0.0f, 2.0f, 0.0f), 3, texture);
            // -y
            DrawLayer(Matrix4.CreateTranslation(0.0f, -2.0f, 0.0f), 2, texture);
            // +z
            DrawLayer(Matrix4.CreateTranslation(0.0f, 0.0f, 0.0f), 4, texture);
            // +x
            DrawLayer(Matrix4.CreateTranslation(2.0f, 0.0f, 0.0f), 0, texture);
            // -z
            DrawLayer(Matrix4.CreateTranslation(4.0f, 0.0f, 0.0f), 5, texture);
        }

        public override Point GetTexelPosition(Vector2 mouse)
        {
            var transMouse = GetOpenGlMouseCoordinates(mouse);

            // on which layer are the mouse coordinates?
            // layer 4 is between [-1, 1]

            // clamp mouse coordinates between -1 and 1 + set the layer
            if (transMouse.X < -1.0f)
            {
                // layer 1
                models.Display.ActiveLayer = 1;
                transMouse.X += 2.0f;
            }
            else if (transMouse.X > 1.0f)
            {
                // layer 0 or 5
                if (transMouse.X > 3.0f)
                {
                    // layer 5
                    models.Display.ActiveLayer = 5;
                    transMouse.X -= 4.0f;
                }
                else
                {
                    models.Display.ActiveLayer = 0;
                    transMouse.X -= 2.0f;
                }
            }
            else
            {
                // layer 2, 3 or 4
                if (transMouse.Y > 1.0f)
                {
                    // layer 3
                    models.Display.ActiveLayer = 3;
                    transMouse.Y -= 2.0f;
                }
                else if (transMouse.Y < -1.0f)
                {
                    // layer 2
                    models.Display.ActiveLayer = 2;
                    transMouse.Y += 2.0f;
                }
                else
                {
                    models.Display.ActiveLayer = 4;
                }
            }

            return Utility.Utility.CanonicalToTexelCoordinates(transMouse,
                models.Images.GetWidth(models.Display.ActiveMipmap),
                models.Images.GetHeight(models.Display.ActiveMipmap));
        }
    }
}
