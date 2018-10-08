using System;
using System.Collections.Generic;
using System.Drawing;
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
            GL.Disable(EnableCap.FramebufferSrgb);
            // this will draw the checkers background
            base.Draw(texture);

            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);

            // bind shader
            shader.Bind();
            shader.SetTransform(GetTransform());
            shader.SetMipmap((float)models.Display.ActiveMipmap);
            shader.SetLayer((float)models.Display.ActiveLayer);
            shader.SetFarplane(CalcFarplane());
            shader.SetGrayscale(models.Display.Grayscale);
            shader.SetCrop(models.Export, models.Display.ActiveLayer);

            models.GlData.BindSampler(shader.GetTextureLocation(), true, models.Display.LinearInterpolation);
            texture.Bind(shader.GetTextureLocation());

            models.GlData.Vao.DrawQuad();

            GL.Enable(EnableCap.FramebufferSrgb);
            GL.Disable(EnableCap.Blend);
            Program.Unbind();
        }

        public override Point GetTexelPosition(Vector2 mouse)
        {
            // calculate farplane
            var viewDir = new Vector4(mouse.X, mouse.Y, CalcFarplane(), 0.0f);
            viewDir = viewDir * GetTransform() * GetLeftHandedOrientation();
            viewDir.Normalize();

            // determine pixel coordinate from view dir
            var polarDirection = new Vector2();
            // t computation
            polarDirection.Y = (float)(Math.Acos(viewDir.Y) / Math.PI);

            //  computation
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            polarDirection.X = viewDir.X == 0.0 ? (float)(Math.PI / 2 * Math.Sign(viewDir.Z)) : (float)(Math.Atan2(viewDir.Z, viewDir.X));
            polarDirection.X = (float)(polarDirection.X / (2.0 * Math.PI) + 0.25);

            if (polarDirection.X < 0.0)
                polarDirection.X += 1.0f;
            if (polarDirection.Y < 0.0)
                polarDirection.Y += 1.0f;

            return Utility.Utility.CanonicalToTexelCoordinates(
                (polarDirection * 2.0f) - Vector2.One,
                models.Images.GetWidth(models.Display.ActiveMipmap),
                models.Images.GetHeight(models.Display.ActiveMipmap));
        }

        protected override Matrix4 GetOrientation()
        {
            return Matrix4.CreateScale(-1.0f, -1.0f, 1.0f);
        }
    }
}
