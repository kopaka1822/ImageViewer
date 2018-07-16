using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TextureViewer.Utility;

namespace TextureViewer.Models.Shader.Statistics
{
    public class AverageStatistics : RgbLuminanceStatistics
    {
        private readonly ImagesModel images;

        public AverageStatistics(bool useSrgb, ImagesModel images) : base(useSrgb)
        {
            this.images = images;
        }

        protected override string GetCombineFunction()
        {
            return base.GetCombineFunction() + "return a + b;";
        }

        protected override Color ModifyResult(Color c)
        {
            // divide through number of pixels to get average
            int numPixels = images.Width * images.Height;
            float invPixels = 1.0f / numPixels;
            return new Color(c.Red * invPixels, c.Green * invPixels, c.Blue * invPixels, c.Alpha * invPixels);
        }
    }
}
