using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX.Mathematics.Interop;

namespace ImageFramework.DirectX.Structs
{
    internal struct StatisticsData
    {
        public int DirectionX;
        public int DirectionY;
        public int Width;
        public int Height;
        public int Stride;
        public int Layer;
        public RawBool FirstTime;
        public RawBool TrueBool;
    }
}
