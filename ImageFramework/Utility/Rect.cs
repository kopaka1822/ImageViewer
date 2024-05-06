using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageFramework.Utility
{
    public struct Rect
    {
        public Float2 Start; // top left corner
        public Float2 End; // bottom right corner

        public float Width => End.X - Start.X;
        public float Height => End.Y - Start.Y;

        public float Top
        {
            get => Start.Y;
            set => Start.Y = value;
        }

        public float Bottom
        {
            get => End.Y;
            set => End.Y = value;
        }

        public float Left
        {
            get => Start.X;
            set => Start.X = value;
        }

        public float Right
        {
            get => End.X;
            set => End.X = value;
        }

        public Rect(Float2 start, Float2 end)
        {
            Start = start;
            End = end;
        }

        public Rect(float left, float top, float width, float height)
        {
            Start = new Float2(left, top);
            End = new Float2(left + width, top + height);
        }

        public Rect Expand(float amount)
        {
            return new Rect(Start - new Float2(amount), End + new Float2(amount));
        }
    }
}
