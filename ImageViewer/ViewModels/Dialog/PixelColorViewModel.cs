using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageFramework.Utility;

namespace ImageViewer.ViewModels.Dialog
{
    public class PixelColorViewModel
    {
        private readonly string decimalColor;
        private readonly string srgbColor;
        private readonly string hexColor;

        public PixelColorViewModel(Color color)
        {
            decimalColor = $"{ToFloat(color.Red)}, {ToFloat(color.Green)}, {ToFloat(color.Blue)}, {ToFloat(color.Alpha)}";
            var srgb = color.ToSrgb();
            srgbColor = $"{ToBit(srgb.Red)}, {ToBit(srgb.Green)}, {ToBit(srgb.Blue)}, {ToBit(srgb.Alpha)}";
            hexColor = $"#{ToHex(srgb.Red)}{ToHex(srgb.Green)}{ToHex(srgb.Blue)}{ToHex(srgb.Alpha)}";
        }

        private static string ToFloat(float c)
        {
            try
            {
                return ((decimal)c).ToString(ImageFramework.Model.Models.Culture);
            }
            catch (Exception)
            {
                return c.ToString(ImageFramework.Model.Models.Culture);
            }
        }

        private static string ToHex(float c)
        {
            try
            {
                var b = (byte)Math.Min(Math.Max(c * 255.0f, 0.0f), 255.0f);
                return b.ToString("X2");
            }
            catch (Exception)
            {
                return c.ToString(ImageFramework.Model.Models.Culture);
            }
        }

        private static string ToBit(float c)
        {
            try
            {
                return ((int)Math.Min(Math.Max(c * 255.0f, 0.0f), 255.0f)).ToString(ImageFramework.Model.Models.Culture);
            }
            catch (Exception)
            {
                return c.ToString(ImageFramework.Model.Models.Culture);
            }
        }

        public string Linear
        {
            get => decimalColor;
            set { }
        }

        public string Srgb
        {
            get => srgbColor;
            set { }
        }

        public string Hex
        {
            get => hexColor;
            set { }
        }
    }
}
