using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using TextureViewer.glhelper;

namespace TextureViewer.Controller.TextureViews
{
    public class EmptyView : ITextureView
    {
        public void Draw(TextureArray2D texture)
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
