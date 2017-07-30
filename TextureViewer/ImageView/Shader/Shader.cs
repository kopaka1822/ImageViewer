using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
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
        private int uniformGrayscale;
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
            vertexShader.CreateInContext(gl);
            vertexShader.SetSource(GetVertexShaderCode());
            vertexShader.Compile();

            /*IntPtr ptr = Marshal.AllocHGlobal(512);
            StringBuilder builder = new StringBuilder(512);
            gl.GetShaderInfoLog(vertexShader.ShaderObject, 512, ptr, builder);
            */
            
            if (vertexShader.CompileStatus.HasValue && vertexShader.CompileStatus.Value == false)
                throw new Exception("vertex shader: " + vertexShader.InfoLog);

            fragmentShader = new FragmentShader();
            fragmentShader.CreateInContext(gl);
            fragmentShader.SetSource(GetFragmentShaderCode());
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
                throw new Exception("shader linking: " + program.InfoLog);

            Utility.GlCheck(gl);
        }

        public void Bind(OpenGL gl, Matrix tranform)
        {
            // TODO bind layer
            program.Push(gl, null);
            UpdateUniforms(gl, tranform);

            Utility.GlCheck(gl);
        }

        protected void UpdateUniforms(OpenGL gl, Matrix tranform)
        {
            gl.UniformMatrix4(uniformModelMatrix, 1, false, tranform.AsColumnMajorArrayFloat);
            gl.Uniform1(uniformCurrentLayer, context.ActiveLayer);
            gl.Uniform1(uniformGrayscale, (uint)context.Grayscale);
        }

        protected void LocateUniforms(OpenGL gl)
        {
            uniformModelMatrix = gl.GetUniformLocation(program.ProgramObject, "modelMatrix");

            uniformCurrentLayer = gl.GetUniformLocation(program.ProgramObject, "currentLayer");

            uniformGrayscale = gl.GetUniformLocation(program.ProgramObject, "grayscale");
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

        protected string GetUniforms()
        {
            return "uniform uint currentLayer;\n" +
                "uniform uint grayscale;\n";
        }

        protected string GetTextures(string type)
        {
            string res = "";
            for (int i = 0; i < context.GetNumImages(); ++i)
                res += "uniform " + type + " tex" + i + ";\n";
            return res;
        }
        
        protected string GetFinalColor()
        {
            // TODO add custom operations
            return "vec4 GetFinalColor(){\n" + "return GetTextureColor0();\n" + "}\n";
        }

        protected string GetMain()
        {
            return "void main(){\n" + 
                "vec4 color = GetFinalColor();\n" + 
                "if(grayscale == uint(1)) color = color.rrrr;\n" +
                "else if(grayscale == uint(2)) color = color.gggg;\n" +
                "else if(grayscale == uint(3)) color = color.bbbb;\n" +
                "else if(grayscale == uint(4)) color = color.aaaa;\n" +
                "fragColor = color;\n" +
                "}";
        }

        protected abstract string GetVertexShaderCode();

        protected abstract string GetFragmentShaderCode();
    }
}
