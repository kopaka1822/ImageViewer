using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;

namespace ImageViewer.Controller.TextureViews
{
    public struct ViewBufferData
    {
        public Matrix Transform;
        public Vector4 Crop;
        public float Multiplier;
        public float Farplane;
    }
}
