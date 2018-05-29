using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TextureViewer.Models.Shader.Statistics
{
    public class MinStatistics : RgbLuminanceStatistics
    {
        public MinStatistics(bool useSrgb) : base(useSrgb)
        {
        }

        protected override string GetCombineFunction()
        {
            return base.GetCombineFunction() + "return min(a, b);";
        }
    }
}
