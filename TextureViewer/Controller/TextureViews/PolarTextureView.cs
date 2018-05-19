using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Graphics.OpenGL4;
using TextureViewer.Controller.TextureViews.Shader;
using TextureViewer.glhelper;

namespace TextureViewer.Controller.TextureViews
{
    public class PolarTextureView : ProjectionTextureView
    {
        private readonly PolarViewShader shader;

        public PolarTextureView(Models.Models models) : base(models)
        {
            shader = new PolarViewShader();
        }

        public override void Dispose()
        {
            shader.Dispose();

            base.Dispose();
        }

        public override void Draw(TextureArray2D texture)
        {
            // this will draw the checkers background
            base.Draw(texture);

            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);

            // bind shader
            shader.Bind();
            shader.SetTransform(GetTransform());
            shader.SetMipmap((float)models.Display.ActiveMipmap);
            shader.SetLayer((float)models.Display.ActiveLayer);
            shader.SetFarplane(models.Display.Aperture);
            shader.SetGrayscale(models.Display.Grayscale);

            texture.Bind(shader.GetTextureLocation());

            models.GlData.Vao.DrawQuad();

            GL.Disable(EnableCap.Blend);
            Program.Unbind();
        }

        protected override Matrix4 GetOrientation()
        {
            return Matrix4.CreateScale(-1.0f, -1.0f, 1.0f);
        }
    }
}
