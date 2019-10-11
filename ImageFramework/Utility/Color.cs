using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageFramework.Model;

namespace ImageFramework.Utility
{
    public struct Color
    {
        [Flags]
        public enum Channel
        {
            R = 1,
            G = 1 << 1,
            B = 1 << 2,
            A = 1 << 3,
            Rgb = R | G | B,
            Rgba = R | G | B | A
        }

        public static readonly Color Black = new Color(0.0f, 0.0f, 0.0f, 0.0f);
        public static readonly Color White = new Color(1.0f, 1.0f, 1.0f, 1.0f);

        public Color(float red, float green, float blue, float alpha = 1.0f)
        {
            Red = red;
            Green = green;
            Blue = blue;
            Alpha = alpha;
        }

        /// <summary>
        /// intializes rgb with the value and alpha with 1.0
        /// </summary>
        /// <param name="single"></param>
        public Color(float single)
        {
            Red = single;
            Green = single;
            Blue = single;
            Alpha = 1.0f;
        }

        public Color(byte r, byte g, byte b, byte a, bool isSigned = false)
        {
            Red = r / 255.0f;
            Green = g / 255.0f;
            Blue = b / 255.0f;
            Alpha = a / 255.0f;
            
            if(isSigned)
            {
                Red = Red * 2.0f - 1.0f;
                Green = Green * 2.0f - 1.0f;
                Blue = Blue * 2.0f - 1.0f;
                Alpha = Alpha * 2.0f - 1.0f;
            }
        }

        public float Red;
        public float Green;
        public float Blue;
        public float Alpha;



        /// <summary>
        /// compares the flagged channels with the given tolerance
        /// </summary>
        /// <param name="other">other color</param>
        /// <param name="flags">channels to verify</param>
        /// <param name="tolerance">absolute tolerance per channel</param>
        /// <returns></returns>
        public bool Equals(Color other, Channel flags, float tolerance = 0.01f)
        {
            if ((flags & Channel.R) != 0 && Math.Abs(Red - other.Red) > tolerance)
                return false;
            if ((flags & Channel.G) != 0 && Math.Abs(Green - other.Green) > tolerance)
                return false;
            if ((flags & Channel.B) != 0 && Math.Abs(Blue - other.Blue) > tolerance)
                return false;
            if ((flags & Channel.A) != 0 && Math.Abs(Alpha - other.Alpha) > tolerance)
                return false;

            return true;
        }

        /// <summary>
        /// convertes color into a decimal string representaion
        /// </summary>
        /// <param name="showAlpha">indicates if the alpha component is included</param>
        /// <param name="decimalPlaces">presicion for the decimal number</param>
        /// <returns></returns>
        public string ToDecimalString(bool showAlpha, int decimalPlaces)
        {
            var r = ScalarToString(Red, decimalPlaces);
            var g = ScalarToString(Green, decimalPlaces);
            var b = ScalarToString(Blue, decimalPlaces);
            var a = ScalarToString(Alpha, decimalPlaces);
            return $"{r} {g} {b}" + (showAlpha ? $" {a}" : "");
        }

        /// <summary>
        /// converts color into a 0-255 bit representation (performs no clamping)
        /// </summary>
        /// <param name="showAlpha">indicates if the alpha component is included</param>
        /// <returns></returns>
        public string ToBitString(bool showAlpha)
        {
            return $"{ScalarToByte(Red)} {ScalarToByte(Green)} {ScalarToByte(Blue)}" + (showAlpha ? $" {ScalarToByte(Alpha)}" : "");
        }

        public string ToFloatString(bool showAlpha, int decimalPlaces)
        {
            return $"{FloatToString(Red, decimalPlaces)} {FloatToString(Green, decimalPlaces)} {FloatToString(Blue, decimalPlaces)}" + (showAlpha ? $" {FloatToString(Alpha, decimalPlaces)}" : "");
        }

        /// <summary>
        /// clamps color values between 0.0 and 1.0
        /// </summary>
        /// <returns>new color object</returns>
        public Color Clamp()
        {
            return new Color(
                Math.Min(Math.Max(Red, 0.0f), 1.0f),
                Math.Min(Math.Max(Green, 0.0f), 1.0f),
                Math.Min(Math.Max(Blue, 0.0f), 1.0f),
                Math.Min(Math.Max(Alpha, 0.0f), 1.0f)
            );
        }

        /// <summary>
        /// converts the color from a linear space into srgb space
        /// </summary>
        /// <returns></returns>
        public Color ToSrgb()
        {
            return new Color(
                ToSrgb(Red),
                ToSrgb(Green),
                ToSrgb(Blue),
                Alpha
            );
        }

        private float ToSrgb(float c)
        {
            if (c >= 1.0f) return 1.0f;
            if (c <= 0.0f) return 0.0f;
            if (c < 0.0031308) return 12.92f * c;
            return 1.055f * (float)Math.Pow(c, 0.41666) - 0.055f;
        }

        public Color FromSrgb()
        {
            return new Color(
                FromSrgb(Red),
                FromSrgb(Green),
                FromSrgb(Blue),
                Alpha
            );
        }

        private float FromSrgb(float c)
        {
            if (c <= 0.04045f) return c / 12.92f;
            return (float)Math.Pow((c + 0.055) / 1.055, 2.4);
        }

        public override string ToString()
        {
            return $"{Red}, {Green}, {Blue}, {Alpha}";
        }

        /// <summary>
        /// helper method for decimal conversion
        /// </summary>
        /// <param name="val">value that should be converted</param>
        /// <param name="decimalPlaces">number of decimal places to display</param>
        /// <returns></returns>
        private static string ScalarToString(float val, int decimalPlaces)
        {
            string s;
            try
            {
                decimal dec = (decimal)val;
                s = decimal.Round(dec, decimalPlaces, MidpointRounding.ToEven).ToString(Models.Culture);
            }
            catch (Exception)
            {
                // float cannot be expressed as a decimal (e.g. NaN, ininity)
                s = val.ToString(Models.Culture);
            }

            // adjust string
            if (!s.StartsWith("-"))
            {
                // empty space instead "-"
                s = " " + s;
            }

            // # Decimal Placs + (+/-) in front + "." space
            while (s.Length <= decimalPlaces + 2)
            {
                s += " ";
            }
            return s;
        }

        private static string ScalarToByte(float val)
        {
            var str = ((int)(val * 255.0f)).ToString(Models.Culture);
            // bytes should ocuppy 3 spaces (srgb maximum is 255)
            while (str.Length < 3)
            {
                str = " " + str;
            }

            return str;
        }

        private static string FloatToString(float val, int decimalPlaces)
        {
            var numPlaces = 7 + decimalPlaces;

            var format = "e" + (decimalPlaces - 1);
            var res = val.ToString(format, Models.Culture);

            // add whitespaces if number too short
            while (res.Length < numPlaces)
            {
                res = " " + res;
            }

            return res;
        }
    }
}
