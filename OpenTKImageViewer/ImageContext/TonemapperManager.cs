using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL4;
using OpenTKImageViewer.glhelper;
using OpenTKImageViewer.Tonemapping;

namespace OpenTKImageViewer.ImageContext
{
    public delegate void ChangedTonemappingSettingsHandler(object sender, EventArgs e);

    public class TonemapperManager
    {
        private List<ToneShader> shaders = new List<ToneShader>();
        private List<ToneParameter> settings = new List<ToneParameter>();

        public event ChangedTonemappingSettingsHandler ChangedSettings;

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
            OnChangedSettings();
        }

        /// <summary>
        /// applies the current set of shaders to the images. ping will point to the final image
        /// </summary>
        /// <param name="ping">the source image</param>
        /// <param name="pong">another buffer image with the same format as ping</param>
        /// <param name="context">image context</param>
        public void ApplyShader(ref TextureArray2D ping, ref TextureArray2D pong, ImageContext context)
        {
            foreach (var p in settings)
            {
                for (int iteration = 0; iteration < (p.Shader.IsSepa ? 1 : 2); ++iteration)
                {
                    for (int level = 0; level < context.GetNumMipmaps(); ++level)
                    {
                        for (int layer = 0; layer < context.GetNumLayers(); ++layer)
                        {
                            ping.BindAsImage(p.Shader.GetSourceImageLocation(), level, layer, TextureAccess.ReadOnly);
                            pong.BindAsImage(p.Shader.GetDestinationImageLocation(), level, layer, TextureAccess.WriteOnly);
                            p.Shader.Dispatch(context.GetWidth(level), context.GetHeight(level), p.Parameters);
                        }
                    }

                    // swap active image (final image is always ping)
                    var temp = ping;
                    ping = pong;
                    pong = temp;
                }
            }
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

        protected virtual void OnChangedSettings()
        {
            ChangedSettings?.Invoke(this, EventArgs.Empty);
        }
    }
}
