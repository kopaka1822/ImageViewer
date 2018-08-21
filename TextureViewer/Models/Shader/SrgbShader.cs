using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL4;
using TextureViewer.glhelper;

namespace TextureViewer.Models.Shader
{
    public class SrgbShader
    {
        private readonly Program shader;

        public SrgbShader()
        {
            var s = new List<glhelper.Shader>
            {
                new glhelper.Shader(ShaderType.VertexShader, GetVertexSource()).Compile(),
                new glhelper.Shader(ShaderType.FragmentShader, GetFragmentSource()).Compile()
            };
            shader = new Program(s, true);
        }

        public void Bind()
        {
            shader.Bind();
        }

        private static string GetVertexSource()
        {
            return OpenGlContext.ShaderVersion + "\n" +
                "void main(){\n" +
                   "vec4 vertex = vec4(0.0, 0.0, 0.0, 1.0);" +
                   "if(gl_VertexID == 0u) vertex = vec4(1.0, -1.0, 0.0, 1.0);\n" +
                   "if(gl_VertexID == 1u) vertex = vec4(-1.0, -1.0, 0.0, 1.0);\n" +
                   "if(gl_VertexID == 2u) vertex = vec4(1.0, 1.0, 0.0, 1.0);\n" +
                   "if(gl_VertexID == 3u) vertex = vec4(-1.0, 1.0, 0.0, 1.0);\n" +
                   "gl_Position = vertex;\n" +
                "}\n";
        }

        public static string ToSrgbFunction()
        {
            return @"vec4 toSrgb(vec4 c){
                        for(int i = 0; i < 3; ++i){
                            if( c[i] > 1.0) c[i] = 1.0;
                            else if( c[i] < 0.0) c[i] = 0.0;
                            else if( c[i] <= 0.0031308) c[i] = 12.92 * c[i];
                            else c[i] = 1.055 * pow(c[i], 0.41666) - 0.055;
                        }
                        return c;
                    }";
        }

        public static string FromSrgbFunction()
        {
            return @"vec4 fromSrgb(vec4 c){
                        for(int i = 0; i < 3; ++i){
                            if(c[i] > 1.0) c[i] = 1.0;
                            else if(c[i] < 0.0) c[i] = 0.0;
                            else if(c[i] <= 0.04045) c[i] /= 12.92;
                            else c[i] = pow((c[i] + 0.055)/1.055, 2.4);
                        }
                        return c;
                    }";
        }

        private static string GetFragmentSource()
        {
            return OpenGlContext.ShaderVersion + "\n" +
                "layout(binding = 0) uniform sampler2D tex;\n" +
                "out vec4 fragColor;\n" +
                ToSrgbFunction() +
                "void main(){\n" +
                   "fragColor =  texelFetch(tex, ivec2(gl_FragCoord.xy), 0);\n" +
                   // convert back
                   "fragColor = toSrgb(fragColor);\n" +
                "}\n";
        }

        public void Dispose()
        {
            shader.Dispose();
        }
    }
}
