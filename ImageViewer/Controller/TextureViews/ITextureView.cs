using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageFramework.DirectX;
using ImageFramework.Utility;
using SharpDX;
using Point = System.Drawing.Point;

namespace ImageViewer.Controller.TextureViews
{
    interface ITextureView : IDisposable
    {
        void Draw(ITexture texture);
        void OnScroll(float amount, Vector2 mouse);
        void OnDrag(Vector2 diff);

        void OnDrag2(Vector2 diff);
        Size3 GetTexelPosition(Vector2 mouse);
    }
}
