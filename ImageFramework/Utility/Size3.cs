using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageFramework.Utility
{
    public struct Size3
    {
        public int X;
        public int Y;
        public int Z;
        // size alias
        public int Width
        {
            get => X;
            set => X = value;
        }

        public int Height
        {
            get => Y;
            set => Y = value;
        }

        public int Depth
        {
            get => Z;
            set => Z = value;
        }

        public int Max => Math.Max(Math.Max(X, Y), Z);
        public int Min => Math.Min(Math.Min(X, Y), Z);
        public int Product => X * Y * Z;

        public Size3(int val = 0)
        {
            X = val;
            Y = val;
            Z = val;
        }

        public Size3(int x, int y, int z = 1)
        {
            X = x;
            Y = y;
            Z = z;
        }

        /// returns corresponding mipmap size
        public Size3 GetMip(int mipmap)
        {
            return new Size3(
                Math.Max(1, X >> mipmap),
                Math.Max(1, Y >> mipmap),
                Math.Max(1, Z >> mipmap)
            );
        }

        public override string ToString()
        {
            return $"{X}, {Y}, {Z}";
        }

        public static bool operator==(Size3 left, Size3 right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Size3 left, Size3 right)
        {
            return !(left == right);
        }

        public static readonly Size3 Zero = new Size3();
    }
}
