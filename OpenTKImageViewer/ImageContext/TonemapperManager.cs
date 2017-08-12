using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTKImageViewer.Tonemapping;

namespace OpenTKImageViewer.ImageContext
{
    public class TonemapperManager
    {
        private List<ToneShader> shaders = new List<ToneShader>();
        private List<ToneParameter> settings = new List<ToneParameter>();

        /// <summary>
        /// loads the requested shader and throws an exception on failure.
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        public ToneParameter LoadShader(string filename)
        {
            var loader = new ShaderLoader(filename);
            var shader = new ToneShader(loader);
            shaders.Add(shader);
            return new ToneParameter(loader.Parameters, shader);
        }

        public void Apply(List<ToneParameter> p)
        {
            settings = p;
            
            RemoveUnusedShader();
        }

        /// <summary>
        /// creates deep copy of settings (except for the shader refence that will be kept)
        /// </summary>
        /// <returns></returns>
        public List<ToneParameter> CloneSettings()
        {
            var res = new List<ToneParameter>();
            foreach (var toneParameter in settings)
            {
                res.Add(toneParameter.Clone());
            }
            return res;
        }

        public void RemoveUnusedShader() => shaders.RemoveAll(
            shader => !settings.Any(toneParameter => ReferenceEquals(toneParameter.Shader, shader)));
    }
}
