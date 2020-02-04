using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageFramework.Utility
{
    public class Bool3
    {
        public bool X;
        public bool Y;
        public bool Z;

        public bool this[int key]
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

        public Bool3(bool value = false)
        {
            X = value;
            Y = value;
            Z = value;
        }

        public Bool3(bool x, bool y, bool z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public static readonly Bool3 False = new Bool3(false);
        public static readonly Bool3 True = new Bool3(true);

        public override string ToString()
        {
            return $"{X}, {Y}, {Z}";
        }

        public bool AllTrue()
        {
            return X && Y && Z;
        }

        public bool AllFalse()
        {
            return !X && !Y && !Z;
        }

        public bool AnyTrue()
        {
            return X || Y || Z;
        }

        public bool AnyFalse()
        {
            return !X || !Y || !Z;
        }

        public static bool operator ==(Bool3 left, Bool3 right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Bool3 left, Bool3 right)
        {
            return !(left == right);
        }

        public bool Equals(Bool3 other)
        {
            return X == other.X && Y == other.Y && Z == other.Z;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is Bool3 other && Equals(other);
        }

        public override int GetHashCode()
        {
            return (X ? 0 : 1) | (Y ? 0 : 2) | (Z ? 0 : 4);
        }
    }
}
