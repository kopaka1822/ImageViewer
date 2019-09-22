using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageFramework.ImageLoader
{
    public class ImageFormat
    {
        public SharpDX.DXGI.Format Format { get; set; }
        public bool IsSrgb { get; set; }
        public bool HasAlpha { get; set; }

        public override string ToString()
        {
            return Format.ToString();
        }
    }
}
