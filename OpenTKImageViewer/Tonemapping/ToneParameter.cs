using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenTKImageViewer.Tonemapping
{
    public class ToneParameter
    {
        public ToneParameter(List<ShaderLoader.Parameter> parameters, ToneShader shader)
        {
            Parameters = parameters;
            Shader = shader;
        }

        public List<ShaderLoader.Parameter> Parameters { get; }
        public ToneShader Shader { get; }

        /// <summary>
        /// creates deep copy of parameter array and keeps reference of shader
        /// </summary>
        /// <returns>copy</returns>
        public ToneParameter Clone()
        {
            var p = new List<ShaderLoader.Parameter>();
            foreach (var parameter in Parameters)
            {
                p.Add(parameter.Clone());
            }

            return new ToneParameter(p, Shader);
        }

        public void RestoreDefaults()
        {
            foreach (var parameter in Parameters)
            {
                parameter.CurrentValue = parameter.Default;
            }
        }
    }
}
