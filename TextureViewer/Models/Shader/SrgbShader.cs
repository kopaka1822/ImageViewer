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

        private static string GetFragmentSource()
        {
            return OpenGlContext.ShaderVersion + "\n" +
                "layout(binding = 0) uniform sampler2D tex;\n" +
                "out vec4 fragColor;\n" +
                "void main(){\n" +
                   "fragColor = texelFetch(tex, ivec2(gl_FragCoord.xy), 0);\n" +
                   "fragColor.rgb = pow(fragColor.rgb, vec3(1.0/2.2));\n" +
                "}\n";
        }

        public void Dispose()
        {
            shader.Dispose();
        }
    }
}
