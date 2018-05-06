using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;

namespace TextureViewer.Controller.TextureViews
{
    interface ITextureView
    {
        void Draw();
        void Dispose();
        void OnScroll(float amount, Vector2 mouse);
        void OnDrag(Vector2 diff);
    }
}
