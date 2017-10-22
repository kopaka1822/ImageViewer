using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL4;
using OpenTKImageViewer.glhelper;
using OpenTKImageViewer.Tonemapping;
using OpenTKImageViewer.Utility;

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

        public bool HasKeyToInvoke(System.Windows.Input.Key key)
        {
            foreach (var set in settings)
                foreach (var param in set.Parameters)
                    foreach (var binding in param.Keybindings)
                        if (binding.Key == key)
                            return true;
            return false;
        }

        /// <summary>
        /// tries to invoke the key on the current parameter set
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public void InvokeKey(System.Windows.Input.Key key)
        {
            if (invokeKey(key))
                OnChangedSettings();
        }

        private bool invokeKey(System.Windows.Input.Key key)
        {
            bool changed = false;
            foreach (var set in settings)
                foreach (var param in set.Parameters)
                    if (param.InvokeKey(key))
                        changed = true;
            return changed;
        }

        public void Apply(List<ToneParameter> p)
        {
            // create copy
            settings = CloneSettings(p);
            RemoveUnusedShader();

            OnChangedSettings();
        }

        /// <summary>
        /// for the special case if a key is pressed and the tonemapper dialog is still open
        /// </summary>
        /// <param name="p"></param>
        /// <param name="key"></param>
        public void ApplyAndInvoke(List<ToneParameter> p, System.Windows.Input.Key key)
        {
            // set settings to original settings
            settings = p;
            // use the invoke to change the settings in this reference
            invokeKey(key);
            // save the copy of the final settings
            settings = CloneSettings(settings);
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
                int numIterations = p.Shader.IsSepa ? 2 : 1;
                for (int iteration = 0; iteration < numIterations; ++iteration)
                {
                    for (int level = 0; level < context.GetNumMipmaps(); ++level)
                    {
                        for (int layer = 0; layer < context.GetNumLayers(); ++layer)
                        {
                            ping.BindAsImage(p.Shader.GetSourceImageLocation(), level, layer, TextureAccess.ReadOnly);
                            pong.BindAsImage(p.Shader.GetDestinationImageLocation(), level, layer, TextureAccess.WriteOnly);
                            p.Shader.Dispatch(context.GetWidth(level), context.GetHeight(level), p.Parameters, iteration);
                        }
                    }

                    // swap active image (final image is always ping)
                    var temp = ping;
                    ping = pong;
                    pong = temp;
                }
            }
        }

        private class ShaderStepper : IStepable
        {
            private readonly ImageContext context;
            private readonly List<ToneParameter> settings;
            private readonly TextureArray2D[] pingpong;
            private IStepable curStepable = null;

            private int curParameter = 0;
            private int curSepaIteration = 0;
            private int curLevel = 0;
            private int curLayer = 0;

            private int numExecuted = 0;

            public ShaderStepper(ImageContext context, List<ToneParameter> settings, TextureArray2D[] pingpong)
            {
                this.context = context;
                this.settings = settings;
                this.pingpong = pingpong;
            }

            public bool HasStep()
            {
                return curParameter < settings.Count;
            }

            public void NextStep()
            {
                var p = settings[curParameter];

                // ping
                //pingpong[0].BindAsImage(p.Shader.GetSourceImageLocation(), curLevel, curLayer, TextureAccess.ReadOnly);
                pingpong[0].BindAsTexture2D(p.Shader.GetSourceImageLocation(), true, curLayer, curLevel);
                // pong
                pingpong[1].BindAsImage(p.Shader.GetDestinationImageLocation(), curLevel, curLayer, TextureAccess.WriteOnly);
                if (curStepable == null)
                    curStepable = p.Shader.GetDispatchStepable(context.GetWidth(curLevel), context.GetHeight(curLevel),
                        p.Parameters, curSepaIteration);
                
                if(curStepable.HasStep())
                    curStepable.NextStep();

                if (curStepable.HasStep())
                    return;

                curStepable = null;
                ++numExecuted;

                // increment
                if (++curLayer < context.GetNumLayers())
                    return;
                
                curLayer = 0;

                if (++curLevel < context.GetNumMipmaps())
                    return;
                
                curLevel = 0;

                // swap ping pong images
                var temp = pingpong[0];
                pingpong[0] = pingpong[1];
                pingpong[1] = temp;

                if (curSepaIteration < (p.Shader.IsSepa ? 1 : 0))
                {
                    ++curSepaIteration;
                    return;
                }
                curSepaIteration = 0;

                ++curParameter;
            }

            public float CurrentStep()
            {
                float extraSteps = 0.0f;
                if (curStepable != null)
                    extraSteps += curStepable.CurrentStep();

                return ((float)numExecuted + extraSteps) / GetNumSteps();
            }

            public int GetNumSteps()
            {
                int iterations = 0;
                foreach (var p in settings)
                {
                    int numIterations = p.Shader.IsSepa ? 2 : 1;
                    iterations += numIterations * context.GetNumMipmaps() * context.GetNumLayers();
                }
                return iterations;
            }

            public string GetDescription()
            {
                if (!HasStep())
                    return "";
                var p = settings[curParameter];
                return "executing " + p.Shader.Name;
            }
        }

        /// <summary>
        /// applies the current set of shaders to the images. pingpong[0] will point to the final image
        /// </summary>
        /// <param name="pingpong">source [0] and destination [1] image</param>
        /// <param name="context">image context</param>
        /// <returns>Stepable container</returns>
        public IStepable GetApplyShaderStepable(TextureArray2D[] pingpong, ImageContext context)
        {
            return new ShaderStepper(context, settings, pingpong);
        }

        /// <summary>
        /// creates deep copy of settings (except for the shader refence that will be kept)
        /// </summary>
        /// <returns></returns>
        public List<ToneParameter> GetSettings()
        {
            return CloneSettings(settings);
        }

        /// <summary>
        /// creates deep copy of the list (except for the shader refence that will be kept)
        /// </summary>
        /// <returns></returns>
        private static List<ToneParameter> CloneSettings(List<ToneParameter> p)
        {
            var res = new List<ToneParameter>();
            foreach (var toneParameter in p)
            {
                res.Add(toneParameter.Clone());
            }
            return res;
        }

        public void RemoveUnusedShader()
        {
            shaders.RemoveAll(
                shader =>
                {
                    if (settings.Any(toneParameter => ReferenceEquals(toneParameter.Shader, shader)))
                        return false;

                    shader.Dispose();
                    return true;
                });
        }

        protected virtual void OnChangedSettings()
        {
            ChangedSettings?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// checks if tonemappers are active
        /// </summary>
        /// <returns>true if at least one tonemapper is active in the current setting</returns>
        public bool HasTonemapper()
        {
            return settings.Count > 0;
        }

        /// <summary>
        /// gets rid of all opengl resources
        /// </summary>
        public void Dispose()
        {
            if (shaders != null)
            {
                foreach (var toneShader in shaders)
                {
                    toneShader.Dispose();
                }
                shaders = null;
            }
        }
    }
}
