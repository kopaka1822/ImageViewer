using System;
using System.Collections.Generic;
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
    }
}
