using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
// ReSharper disable CompareOfFloatsByEqualityOperator

namespace ImageFramework.Utility
{
    public struct Float2
    {
        public float X;
        public float Y;

        public float this[int key]
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

        public Float2(float value = 0.0f)
        {
            X = value;
            Y = value;
        }

        public Float2(float x, float y)
        {
            X = x;
            Y = y;
        }

        public static readonly Float2 Zero = new Float2();
        public static readonly Float2 One = new Float2(1.0f);
        public static readonly Float2 NegOne = new Float2(-1.0f);

        public override string ToString()
        {
            return $"{X}, {Y}";
        }

        public static bool operator ==(Float2 left, Float2 right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Float2 left, Float2 right)
        {
            return !(left == right);
        }

        public static Float2 operator +(Float2 left, Float2 right)
        {
            return new Float2(left.X + right.X, left.Y + right.Y);
        }

        public static Float2 operator -(Float2 left, Float2 right)
        {
            return new Float2(left.X - right.X, left.Y - right.Y);
        }

        public static Float2 operator *(Float2 left, Float2 right)
        {
            return new Float2(left.X * right.X, left.Y * right.Y);
        }

        public static Float2 operator *(Float2 left, float right)
        {
            return new Float2(left.X * right, left.Y * right);
        }

        public static Float2 operator *(float left, Float2 right)
        {
            return new Float2(left * right.X, left * right.Y);
        }

        public static Float2 operator /(Float2 left, Float2 right)
        {
            return new Float2(left.X / right.X, left.Y / right.Y);
        }

        public bool Equals(Float2 other)
        {
            return X == other.X && Y == other.Y;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is Float2 other && Equals(other);
        }

        public override int GetHashCode()
        {
            unsafe
            {
                var hashCode = AsInt(X);
                hashCode = (hashCode * 397) ^ AsInt(Y);
                return hashCode;
            }
        }

        private unsafe int AsInt(float v)
        {
            float* fp = &v;
            return *(int*)fp;
        }

        public Size2 ToPixels(Size2 size)
        {
            return new Size2(
                Utility.Clamp((int)(X * size.X), 0, size.X - 1),
                Utility.Clamp((int)(Y * size.Y), 0, size.Y - 1)
            );
        }

        public Float2 Clamp(Float2 min, Float2 max)
        {
            return new Float2(
                Utility.Clamp(X, min.X, max.X),
                Utility.Clamp(Y, min.Y, max.Y)
            );
        }
    }
}
