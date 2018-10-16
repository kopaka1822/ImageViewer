using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL4;
using TextureViewer.glhelper;
using TextureViewer.Models;
using TextureViewer.Models.Dialog;

namespace TextureViewer.Controller.TextureViews.Shader
{
    public abstract class ViewShader
    {
        protected Program ShaderProgram;

        protected ViewShader(string vertexSource, string fragmentSource)
        {
            List<glhelper.Shader> shaders = new List<glhelper.Shader>(2);
            shaders.Add(new glhelper.Shader(ShaderType.VertexShader, vertexSource).Compile());
            shaders.Add(new glhelper.Shader(ShaderType.FragmentShader, fragmentSource).Compile());

            ShaderProgram = new Program(shaders, true);
        }

        public abstract void Bind();

        protected static string GetVersion()
        {
            return OpenGlContext.ShaderVersion + "\n";
        }

        /// <summary>
        /// adds grayscale line to shader. color must be the current color and grayscale the grayscale mode
        /// </summary>
        /// <returns></returns>
        protected static string ApplyGrayscale()
        {
            return "if(grayscale == uint(1)) color = vec4(color.rrr,1.0);\n" +
                   "else if(grayscale == uint(2)) color = vec4(color.ggg,1.0);\n" +
                   "else if(grayscale == uint(3)) color = vec4(color.bbb,1.0);\n" +
                   "else if(grayscale == uint(4)) color = vec4(color.aaa,1.0);\n";
        }

        /// <summary>
        /// darkens the area that will be cropped by the image export.
        /// - color must be the current color
        /// - texcoord the current texture coordinate
        /// - crop must be the crop boundry vec4(xmin, xmax, ymin, ymax)
        /// </summary>
        /// <returns></returns>
        protected static string ApplyColorCrop(string texcoord = "texcoord")
        {
            return $"if({texcoord}.x < crop.x || {texcoord}.x > crop.y || {texcoord}.y < crop.z || {texcoord}.y > crop.w)\n" +
                   "color.rgb = min(color.rgb, vec3(1.0)) * vec3(0.5);\n";
        }

        protected static void SetCropCoordinates(int location, ExportModel model, int layer)
        {
            if(model.Layer != -1) //  only a single layer active
            {
                // darken due to layer mismatch?
                if (model.IsExporting && model.Layer != layer)
                {
                    // everything is gray
                    GL.Uniform4(location, 0.0f, 0.0f, 0.0f, 0.0f);
                    return;
                }

                if (model.UseCropping && (model.DisplayCropping || model.IsExporting))
                {
                    // draw crop box
                    GL.Uniform4(location, model.GetCropStartXPercent(), model.GetCropEndXPercent(),
                           model.GetCropStartYPercent(), model.GetCropEndYPercent());
                    return;
                }
            } 

            // nothing is gray
            GL.Uniform4(location, 0.0f, 1.0f, 0.0f, 1.0f);
        }

        public void Dispose()
        {
            ShaderProgram?.Dispose();
        }
    }
}
