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
        public class Settings
        {
            public List<ToneParameter> ToneParameters = new List<ToneParameter>();
            public int StatisticsPosition = 0;

        
            public Settings Clone()
            {
                var res = new Settings {StatisticsPosition = StatisticsPosition};
                foreach (var toneParameter in ToneParameters)
                {
                    res.ToneParameters.Add(toneParameter.Clone());
                }
                return res;
            }

            public bool InvokeKey(System.Windows.Input.Key key)
            {
                var changed = false;
                foreach (var set in ToneParameters)
                foreach (var param in set.Parameters)
                    if (param.InvokeKey(key))
                        changed = true;
                return changed;
            }
        }

        private List<ToneShader> shaders = new List<ToneShader>();
        private Settings settings = new Settings();

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
            return settings.ToneParameters.Any(set => set.Parameters.Any(param => param.Keybindings.Any(binding => binding.Key == key)));
        }

        /// <summary>
        /// tries to invoke the key on the current parameter set
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public void InvokeKey(System.Windows.Input.Key key)
        {
            if (settings.InvokeKey(key))
                OnChangedSettings();
        }

        public void Apply(Settings s)
        {
            // create copy
            settings = s.Clone();
            RemoveUnusedShader();

            OnChangedSettings();
        }

        private class ShaderStepper : IStepable
        {
            private readonly ImageContext context;
            private readonly Settings settings;
            private readonly TextureArray2D[] pingpong;
            private IStepable curStepable = null;

            private int curParameter = 0;
            private int curSepaIteration = 0;
            private int curLevel = 0;
            private int curLayer = 0;

            private int numExecuted = 0;

            public ShaderStepper(ImageContext context, Settings settings, TextureArray2D[] pingpong)
            {
                Debug.Assert(pingpong.Length >= 3);

                this.context = context;
                this.settings = settings;
                this.pingpong = pingpong;
            }

            public bool HasStep()
            {
                return curParameter < settings.ToneParameters.Count;
            }

            private void BindOriginalImages()
            {

            }

            public void NextStep()
            {
                var p = settings.ToneParameters[curParameter];

                // bind original images 
                for (int i = 0; i < context.GetNumImages(); ++i)
                {
                    var slot = p.Shader.GetOriginalImageLocation(i);
                    if (slot == -1)
                        break;

                    context.BindSampler(slot, context.GetImageTexture(i).HasMipmaps(), true);
                    context.GetImageTexture(i).BindAsTexture2D(slot, curLayer, curLevel);
                }

                // ping
                pingpong[0].BindAsTexture2D(p.Shader.GetSourceImageLocation(), curLayer, curLevel);
                context.BindSampler(p.Shader.GetSourceImageLocation(), true, true);
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

                if(curParameter == settings.StatisticsPosition && curSepaIteration == 0)
                {
                    // save pingpong[0] before drawing in it again! pingpong[0] should be used as statistics point
                    pingpong[2] = pingpong[0];
                    // use a new texture for drawing
                    pingpong[0] = context.TextureCache.GetAvailableTexture();
                }

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

                // finished with this iteration

                ++curParameter;
            }

            public float CurrentStep()
            {
                float extraSteps = 0.0f;
                if (curStepable != null)
                    extraSteps += curStepable.CurrentStep();
                var numSteps = GetNumSteps();
                if (numSteps == 0)
                    return 1.0f;
                return ((float)numExecuted + extraSteps) / numSteps;
            }

            public int GetNumSteps()
            {
                int iterations = 0;
                foreach (var p in settings.ToneParameters)
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
                var p = settings.ToneParameters[curParameter];
                return "executing " + p.Shader.Name;
            }
        }

        /// <summary>
        /// applies the current set of shaders to the images. pingpong[0] will point to the final image
        /// </summary>
        /// <param name="pingpong">source [0] and destination [1] image. [2] should be null and is later used for the statistics image</param>
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
        public Settings GetSettings()
        {
            return settings.Clone();
        }

        public void RemoveUnusedShader()
        {
            shaders.RemoveAll(
                shader =>
                {
                    if (settings.ToneParameters.Any(toneParameter => ReferenceEquals(toneParameter.Shader, shader)))
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
            return settings.ToneParameters.Count > 0;
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
