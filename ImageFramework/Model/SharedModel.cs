using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageFramework.Model.Shader;

namespace ImageFramework.Model
{
    /// <summary>
    /// data that is usually used by multiple models
    /// </summary>
    internal class SharedModel : IDisposable
    {
        public MitchellNetravaliScaleShader ScaleShader { get; }
        public QuadShader QuadShader { get; } = new QuadShader();
        public ConvertFormatShader Convert { get; }
        public SharedModel()
        {
            ScaleShader = new MitchellNetravaliScaleShader(QuadShader);
            Convert = new ConvertFormatShader(QuadShader);
        }

        public void Dispose()
        {
            Convert?.Dispose();
            ScaleShader?.Dispose();
            QuadShader?.Dispose();
        }
    }
}
