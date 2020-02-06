using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageFramework.Utility
{
    public struct Size2
    {
        public int X;
        public int Y;
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

        public int this[int key]
        {
            get
            {
                if (key == 0) return X;
                Debug.Assert(key == 1);
                return Y;
            }
            set
            {
                Debug.Assert(key >= 0 && key <= 1);
                if (key == 0) X = value;
                if (key == 1) Y = value;
            }
        }

        public int Max => Math.Max(X, Y);
        public int Min => Math.Min(X, Y);
        public int Product => X * Y;

        public int MaxMipLevels
        {
            get
            {
                var max = Max;
                var maxMip = 1;
                while ((max /= 2) > 0) ++maxMip;
                return maxMip;
            }
        }

        public Size2(int val = 0)
        {
            X = val;
            Y = val;
        }

        public Size2(int x, int y)
        {
            X = x;
            Y = y;
        }

        public Size2(Size2 copy)
        {
            X = copy.X;
            Y = copy.Y;
        }

        /// returns corresponding mipmap size
        public Size2 GetMip(int mipmap)
        {
            return new Size2(
                Math.Max(1, X >> mipmap),
                Math.Max(1, Y >> mipmap)
            );
        }

        public override string ToString()
        {
            return $"{X}, {Y}";
        }

        public static bool operator ==(Size2 left, Size2 right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Size2 left, Size2 right)
        {
            return !(left == right);
        }

        /*public static Bool2 operator <(Size2 left, Size2 right)
        {
            return new Bool2(left.X < right.X, left.Y < right.Y);
        }

        public static Bool2 operator <=(Size2 left, Size2 right)
        {
            return new Bool2(left.X <= right.X, left.Y <= right.Y);
        }

        public static Bool2 operator >(Size2 left, Size2 right)
        {
            return new Bool2(left.X > right.X, left.Y > right.Y);
        }

        public static Bool2 operator >=(Size2 left, Size2 right)
        {
            return new Bool2(left.X >= right.X, left.Y >= right.Y);
        }*/

        public static Size2 operator +(Size2 left, Size2 right)
        {
            return new Size2(left.X + right.X, left.Y + right.Y);
        }

        public static Size2 operator -(Size2 left, Size2 right)
        {
            return new Size2(left.X - right.X, left.Y - right.Y);
        }

        public Size2 Clamp(Size2 min, Size2 max)
        {
            return new Size2(
                Utility.Clamp(X, min.X, max.X),
                Utility.Clamp(Y, min.Y, max.Y)
            );
        }

        public static readonly Size2 Zero = new Size2();
        public static readonly Size2 One = new Size2(1);

        public bool Equals(Size2 other)
        {
            return X == other.X && Y == other.Y;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is Size2 other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = X;
                hashCode = (hashCode * 397) ^ Y;
                return hashCode;
            }
        }

        // converts[0, size] to [0, 1]
        public Float2 ToCoords(Size2 size)
        {
            if (size == Zero) return new Float2(0.0f);
            return new Float2(
                (X + 0.5f) / size.X,
                (Y + 0.5f) / size.Y
            );
        }
    }
}
