using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
// ReSharper disable CompareOfFloatsByEqualityOperator

namespace ImageFramework.Utility
{
    public struct Float3
    {
        public float X;
        public float Y;
        public float Z;

        public Float2 XY => new Float2(X, Y);
        public Float2 YZ => new Float2(Y, Z);

        public float this[int key]
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

        public Float3(float value = 0.0f)
        {
            X = value;
            Y = value;
            Z = value;
        }

        public Float3(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public Float3(Float2 xy, float z)
        {
            X = xy.X;
            Y = xy.Y;
            Z = z;
        }

        public Float3(float x, Float2 yz)
        {
            X = x;
            Y = yz.X;
            Z = yz.Y;
        }

        public static readonly Float3 Zero = new Float3();
        public static readonly Float3 One = new Float3(1.0f);

        public override string ToString()
        {
            return $"{X}, {Y}, {Z}";
        }

        public static bool operator==(Float3 left, Float3 right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Float3 left, Float3 right)
        {
            return !(left == right);
        }

        public bool Equals(Float3 other)
        {
            return X == other.X && Y == other.Y && Z == other.Z;
        }

        public static Bool3 operator <(Float3 left, Float3 right)
        {
            return new Bool3(left.X < right.X, left.Y < right.Y, left.Z < right.Z);
        }

        public static Bool3 operator <=(Float3 left, Float3 right)
        {
            return new Bool3(left.X <= right.X, left.Y <= right.Y, left.Z <= right.Z);
        }

        public static Bool3 operator >(Float3 left, Float3 right)
        {
            return new Bool3(left.X > right.X, left.Y > right.Y, left.Z > right.Z);
        }

        public static Bool3 operator >=(Float3 left, Float3 right)
        {
            return new Bool3(left.X >= right.X, left.Y >= right.Y, left.Z >= right.Z);
        }

        public static Float3 operator +(Float3 left, Float3 right)
        {
            return new Float3(left.X + right.X, left.Y + right.Y, left.Z + right.Z);
        }

        public static Float3 operator -(Float3 left, Float3 right)
        {
            return new Float3(left.X - right.X, left.Y - right.Y, left.Z - right.Z);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is Float3 other && Equals(other);
        }

        public override int GetHashCode()
        {
            unsafe
            {
                var hashCode = AsInt(X);
                hashCode = (hashCode * 397) ^ AsInt(Y);
                hashCode = (hashCode * 397) ^ AsInt(Z);
                return hashCode;
            }
        }

        private unsafe int AsInt(float v)
        {
            float* fp = &v;
            return *(int*) fp;
        }

        // converts [0, 1] range to [0, size-1]
        public Size3 ToPixels(Size3 size)
        {
            return new Size3(
                Utility.Clamp((int)(X * size.X), 0, size.X - 1),
                Utility.Clamp((int)(Y * size.Y), 0, size.Y - 1),
                Utility.Clamp((int)(Z * size.Z), 0, size.Z - 1)
            );
        }

        public Float3 Clamp(Float3 min, Float3 max)
        {
            return new Float3(
                Utility.Clamp(X, min.X, max.X),
                Utility.Clamp(Y, min.Y, max.Y),
                Utility.Clamp(Z, min.Z, max.Z)
            );
        }
    }
}
