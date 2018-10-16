using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL4;
using TextureViewer.glhelper;
using TextureViewer.Models.Filter;

namespace TextureViewer.Models.Shader
{
    public class FilterShader
    {
        private readonly Program program;
        // first location index for filter parameter
        private readonly int parameterLocationStart;
        // first location index for texture isSrgb bindings
        private static readonly int textureLocationStart = 2;
        private static readonly int textureBindingStart = 1;

        public static readonly int LocalSize = 8;
        public static readonly int MinWorkGroupSize = 4;

        private readonly FilterModel filterModel;

        public FilterShader(string shaderSource, FilterModel filterModel)
        {
            this.filterModel = filterModel;
            var maxTextureBindings = GL.GetInteger(GetPName.MaxTextureImageUnits);
            // MAX_UNIFORM_LOCATIONS
            var maxUniformLocations = GL.GetInteger((GetPName)(0x826E));
            this.parameterLocationStart = textureLocationStart + filterModel.TextureParameters.Count;

            // test if any binding slots were exceeded
            if (textureBindingStart + filterModel.TextureParameters.Count > maxTextureBindings)
                throw new Exception($"Too many texture bindings. Only {maxTextureBindings} texture bindings are available on this GPU");
            if (parameterLocationStart + filterModel.Parameters.Count > maxUniformLocations)
                throw new Exception($"Too many uniform variables. Only {maxUniformLocations} uniform location bindings are available on this GPU");

            var shader = new glhelper.Shader(ShaderType.ComputeShader,
                GetShaderHeader() +
                "#line 1\n" +
                shaderSource);

            shader.Compile();

            program = new Program(new List<glhelper.Shader> { shader }, true);
        }

        public void Dispose()
        {
            program.Dispose();
        }

        /// <summary>
        /// updates all unfirom locations and bind the shader
        /// </summary>
        public void Bind(Models models, int layer, int iteration)
        {
            program.Bind();

            var curIndex = 0;

            // set original textures
            foreach (var tex in filterModel.TextureParameters)
            {
                // bind texture and sampler
                var tex2D = models.Images.GetTexture(tex.Source);
                models.GlData.BindSampler(textureBindingStart + curIndex, false, true);
                tex2D.BindAsTexture2D(textureBindingStart + curIndex, layer, 0);
                // bind srgb value
                GL.Uniform1(textureLocationStart + curIndex, tex2D.IsSrgb ? 1 : 0);
                ++curIndex;
            }

            // set parameter
            var curLocation = parameterLocationStart;
            foreach (var parameter in filterModel.Parameters)
            {
                switch (parameter.GetParamterType())
                {
                    case ParameterType.Float:
                        GL.Uniform1(curLocation++, parameter.GetFloatModel().Value);
                        break;
                    case ParameterType.Int:
                        GL.Uniform1(curLocation++, parameter.GetIntModel().Value);
                        break;
                    case ParameterType.Bool:
                        GL.Uniform1(curLocation++, parameter.GetBoolModel().Value ? 1 : 0);
                        break;
                }
            }

            if (filterModel.IsSepa)
            {
                // set direction for sepa shader
                Debug.Assert(iteration == 1 || iteration == 0);
                GL.Uniform2(0, iteration, 1 - iteration);
            }
            else
            {
                Debug.Assert(iteration == 0);
            }
        }

        public int GetSourceImageLocation()
        {
            return 0;
        }

        public int GetDestinationImageLocation()
        {
            return 1;
        }

        private string GetShaderHeader()
        {
            return "#version 430\n" +
                   $"layout(local_size_x = {LocalSize}, local_size_y = {LocalSize}) in;\n" +
                   "layout(binding = 0) uniform sampler2D src_image;\n" +
                   "layout(rgba32f, binding = 1) uniform writeonly image2D dst_image;\n" +
                   "layout(location = 1) uniform ivec2 pixelOffset;\n" +
                   (filterModel.IsSepa ? "layout(location = 0) uniform ivec2 filterDirection;\n" : "")
                   + SrgbShader.FromSrgbFunction() + GetVariableLocations() + GetTextureFunctions();
        }

        // uniforms for variables
        private string GetVariableLocations()
        {
            var res = "";
            var curLocation = parameterLocationStart;
            foreach (var p in filterModel.Parameters)
            {
                res += $"layout(location = {curLocation++}) uniform {p.GetParamterType().ToString().ToLower()} {p.GetBase().VariableName};\n";
            }
            return res;
        }

        // functions for original texture usage
        private string GetTextureFunctions()
        {
            var res = "";
            var i = 0;
            foreach (var t in filterModel.TextureParameters)
            {
                // uniform sampler
                res += $"layout(binding = {textureBindingStart + i}) uniform sampler2D texture{i};\n";
                // uniform isSrgb
                res += $"layout(location = {textureLocationStart + i}) uniform bool textureSrgb{i};\n";
                // functions
                res += "vec4 " + t.FunctionName + "(vec2 coord){\n";
                res += $"vec4 c = texture(texture{i}, coord);\n";
                res += $"if(textureSrgb{i}) c = fromSrgb(c);\n";
                res += "return c;}\n";

                res += "vec4 " + t.FunctionName + "(ivec2 coord){\n";
                res += $"vec4 c = texelFetch(texture{i}, coord, 0);\n";
                res += $"if(textureSrgb{i}) c = fromSrgb(c);\n";
                res += "return c;}\n";

                ++i;
            }
            return res;
        }
    }
}
