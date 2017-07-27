using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpGL;
using SharpGL.SceneGraph;
using SharpGL.SceneGraph.Shaders;
using TextureViewer.glhelper;

namespace TextureViewer.ImageView.Shader
{
    public abstract class Shader
    {
        private VertexShader vertexShader;
        private FragmentShader fragmentShader;
        private ShaderProgram program;
        protected Context context;

        // uniform locations
        private int uniformModelMatrix;
        private int uniformCurrentLayer;
        private List<int> uniformTextures = new List<int>();

        public Shader(Context context)
        {
            this.context = context;
        }

        public void Init(OpenGL gl)
        {
            Utility.GlCheck(gl);

            Debug.Assert(vertexShader == null);
            Debug.Assert(fragmentShader == null);
            vertexShader = new VertexShader();
            fragmentShader = new FragmentShader();
            vertexShader.CreateInContext(gl);
            fragmentShader.CreateInContext(gl);

            vertexShader.SetSource(GetVertexShaderCode());
            fragmentShader.SetSource(GetFragmentShaderCode());

            vertexShader.Compile();
            if (vertexShader.CompileStatus.HasValue && vertexShader.CompileStatus.Value == false)
                throw new Exception("vertex shader: " + vertexShader.InfoLog);

            fragmentShader.Compile();
            if (fragmentShader.CompileStatus.HasValue && fragmentShader.CompileStatus.Value == false)
                throw new Exception("fragment shader:" + fragmentShader.InfoLog);

            program = new ShaderProgram();
            program.CreateInContext(gl);

            program.AttachShader(vertexShader);
            program.AttachShader(fragmentShader);

            program.Link();

            Utility.GlCheck(gl);
            LocateUniforms(gl);
            Utility.GlCheck(gl);

            if (program.LinkStatus.HasValue && program.LinkStatus.Value == false)
                throw new Exception("shader linkin: " + program.InfoLog);

            Utility.GlCheck(gl);
        }

        public void Bind(OpenGL gl, Matrix tranform)
        {
            // TODO bind layer
            program.Push(gl, null);
            gl.UniformMatrix4(uniformModelMatrix, 1, false, tranform.AsColumnMajorArrayFloat);
            gl.Uniform1(uniformCurrentLayer, context.ActiveLayer);

            Utility.GlCheck(gl);
        }

        protected void LocateUniforms(OpenGL gl)
        {
            uniformModelMatrix = gl.GetUniformLocation(program.ProgramObject, "modelMatrix");

            uniformCurrentLayer = gl.GetUniformLocation(program.ProgramObject, "currentLayer");

            // locate texture uniforms
            uniformTextures.Clear();

            Utility.GlCheck(gl);
        }



        public void Unbind(OpenGL gl)
        {
            program.Pop(gl, null);
        }

        protected string GetVersion()
        {
            return "#version 330\n";
        }

        protected string GetVaryings()
        {
            return "in vec2 texcoord;\n" 
                + "out vec4 fragColor;\n";
        }

        protected string GetUniforms()
        {
            return "uniform uint currentLayer;\n";
        }

        protected string GetTextures(string type)
        {
            string res = "";
            for (int i = 0; i < context.GetNumImages(); ++i)
                res += "uniform " + type + " tex" + i + ";\n";
            return res;
        }

        protected string GetTextures2DArray()
        {
            return GetTextures("sampler2DArray");
        }

        protected string GetTexture2DArrayGetters()
        {
            // TODO apply tone mapping function here?
            string res = "";
            for (int i = 0; i < context.GetNumImages(); ++i)
            {
                res += "vec4 GetTextureColor" + i + "(){\n";
                // image function
                res += "return texture(tex" + i + ", vec3(texcoord, float(currentLayer)));\n";
                res += "}\n";
            }
            return res;
        }

        protected string GetFinalColor()
        {
            // TODO add custom operations
            return "vec4 GetFinalColor(){\n" + "return GetTextureColor0();\n" + "}\n";
        }

        protected string GetMain()
        {
            return "void main(){\n" + "fragColor = GetFinalColor();\n" + "}";
        }

        protected string GetVertexShaderCode()
        {
            return GetVersion() +
                "in vec4 vertex;\n" +
                "out vec2 texcoord;\n" + 
                "uniform mat4 modelMatrix;\n" +
                "void main(){\n" +
                    "texcoord = (vertex.xy + vec2(1.0)) * vec2(0.5);\n" +
                    "gl_Position = modelMatrix * vec4(vertex.xy, 0.0, 1.0);\n" +
                "}";
        }

        protected abstract string GetFragmentShaderCode();
    }
}
