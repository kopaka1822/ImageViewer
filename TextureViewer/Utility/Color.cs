using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;

namespace TextureViewer.Utility
{
    public class Color
    {
        public static readonly Color ZERO = new Color(0.0f, 0.0f, 0.0f, 0.0f);

        public Color(float red, float green, float blue, float alpha = 1.0f)
        {
            Red = red;
            Green = green;
            Blue = blue;
            Alpha = alpha;
        }

        public Color(Vector4 color) :
            this(color.X, color.Y, color.Z, color.W)
        {

        }

        public float Red { get; set; }
        public float Green { get; set; }
        public float Blue { get; set; }
        public float Alpha { get; set; }

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
            if (c > 1.0f) return 1.0f;
            if (c < 0.0f) return 0.0f;
            if (c < 0.0031308) return 12.92f * c;
            return 1.055f * (float)Math.Pow(c, 0.41666) - 0.055f;
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
                s = decimal.Round(dec, decimalPlaces, MidpointRounding.ToEven).ToString(App.GetCulture());
            }
            catch (Exception)
            {
                // float cannot be expressed as a decimal (e.g. NaN, ininity)
                s = val.ToString(App.GetCulture());
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
            var str = ((int) (val * 255.0f)).ToString();
            // bytes should ocuppy 3 spaces (srgb maximum is 255)
            while (str.Length < 3)
            {
                str = " " + str;
            }

            return str;
        }

        public Vector4 ToVector()
        {
            return new Vector4(Red, Green, Blue, Alpha);
        }
    }
}
