using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageFramework.DirectX;
using SharpDX;
using Point = System.Drawing.Point;

namespace ImageViewer.Controller.TextureViews
{
    public class EmptyView : ITextureView
    {
        public void Draw(ITexture texture)
        {
            Debug.Assert(texture == null);
        }

        public void Dispose()
        {

        }

        public void OnScroll(float amount, Vector2 mouse)
        {

        }

        public void OnDrag(Vector2 diff)
        {

        }

        public Point GetTexelPosition(Vector2 mouse)
        {
            return new Point(0, 0);
        }
    }
}
