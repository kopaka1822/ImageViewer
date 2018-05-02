using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TextureViewer.Utility
{
    public class Color
    {
        public Color(float red, float green, float blue, float alpha = 1.0f)
        {
            Red = red;
            Green = green;
            Blue = blue;
            Alpha = alpha;
        }

        public float Red { get; }
        public float Green { get; }
        public float Blue { get; }
        public float Alpha { get; }
    }
}
