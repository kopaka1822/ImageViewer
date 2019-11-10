using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageFramework.Annotations;

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

        public int this[int key]
        {
            get
            {
                if (key == 0) return X;
                if (key == 1) return Y;
                Debug.Assert(key == 2);
                return Z;
            }
            set
            {
                Debug.Assert(key >= 0 && key <= 2);
                if (key == 0) X = value;
                if (key == 1) Y = value;
                if (key == 2) Z = value;
            }
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

        public bool Equals(Size3 other)
        {
            return X == other.X && Y == other.Y && Z == other.Z;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is Size3 other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = X;
                hashCode = (hashCode * 397) ^ Y;
                hashCode = (hashCode * 397) ^ Z;
                return hashCode;
            }
        }
    }
}
