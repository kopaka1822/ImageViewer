using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        public bool FirstTime;
        public float RefNan;
    }
}
