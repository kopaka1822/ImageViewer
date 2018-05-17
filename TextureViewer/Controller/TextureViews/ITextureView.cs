using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using TextureViewer.glhelper;

namespace TextureViewer.Controller.TextureViews
{
    interface ITextureView
    {
        void Draw(TextureArray2D texture);
        void Dispose();
        void OnScroll(float amount, Vector2 mouse);
        void OnDrag(Vector2 diff);
    }
}
