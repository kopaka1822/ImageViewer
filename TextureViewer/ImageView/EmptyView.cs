using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using SharpGL;

namespace TextureViewer.ImageView
{
    class EmptyView : IImageView
    {
        private OpenGL gl;

        public void Init(OpenGL gl, MainWindow parent)
        {
            this.gl = gl;
        }

        public void Draw()
        {
            // do nothing
        }

        public void OnDrag(Vector diff)
        {
            
        }
    }
}
