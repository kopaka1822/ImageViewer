using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageFramework.DirectX;
using SharpDX;
using Point = System.Drawing.Point;

namespace ImageViewer.Controller.TextureViews
{
    interface ITextureView : IDisposable
    {
        void Draw(TextureArray2D texture);
        void OnScroll(float amount, Vector2 mouse);
        void OnDrag(Vector2 diff);
        Point GetTexelPosition(Vector2 mouse);
    }
}
