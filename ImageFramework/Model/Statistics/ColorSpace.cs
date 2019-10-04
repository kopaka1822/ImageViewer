using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageFramework.Utility;

namespace ImageFramework.Model.Statistics
{
    public struct ColorSpace
    {
        public Color Linear { get; }
        public float LinearLuminance { get; }

        public Color Srgb { get; }

        public float SrgbLuminance { get; }
    }
}
