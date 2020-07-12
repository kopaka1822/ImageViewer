using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageFramework.DirectX;
using ImageFramework.Utility;
using SharpDX;
using Point = System.Drawing.Point;

namespace ImageViewer.Controller.TextureViews
{
    public class EmptyView : ITextureView
    {
        public void Draw(int id, ITexture texture)
        {
            Debug.Assert(texture == null);
        }

        public void Dispose(){}

        public void OnScroll(float amount, Vector2 mouse) {}

        public void OnDrag(Vector2 diff) {}
        public void OnDrag2(Vector2 diff) {}

        public Size3? GetTexelPosition(Vector2 mouse)
        {
            return null;
        }

        public void UpdateImage(int id, ITexture texture) { }
    }
}
