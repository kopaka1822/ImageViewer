using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using OpenTK;
using OpenTK.Graphics.OpenGL4;
using OpenTKImageViewer.UI;
using OpenTKImageViewer.View.Shader;

namespace OpenTKImageViewer.View
{
    public class PolarView : ProjectionView
    {
        private PolarViewShader shader;

        public PolarView(ImageContext.ImageContext context, TextBox boxScroll)
            :
            base(context, boxScroll)
        {
        }

        protected override void Init()
        {
            base.Init();
            shader = new PolarViewShader();
        }

        public override void Draw(int activeImage)
        {
            DrawCheckers();

            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);

            // bind the shader?
            shader.Bind(Context);
            shader.SetTransform(GetTransform());
            shader.SetLevel((float)Context.ActiveMipmap);
            shader.SetLayer((float)Context.ActiveLayer);
            shader.SetFarplane(GetZoom());
            shader.SetGrayscale(Context.Grayscale);
            Context.BindFinalTextureAs2DSamplerArray(activeImage, shader.GetTextureLocation());

            // draw via vertex array
            DrawQuad();

            GL.Disable(EnableCap.Blend);
        }
        
        protected override Matrix4 GetOrientation()
        {
            return Matrix4.CreateScale(-1.0f, -1.0f, 1.0f);
        }

        public override void UpdateMouseDisplay(MainWindow window)
        {
            var mousePoint = window.StatusBar.GetCanonicalMouseCoordinates();

            // Matrix Coordinate system is reversed (left handed)
            var viewDir = (new Vector4((float)mousePoint.X, (float)mousePoint.Y, GetZoom(), 0.0f)) * (GetTransform() * GetLeftHandedOrientation());
            viewDir.Normalize();

            // determine pixel coordinate from view dir
            Vector2 polarDirection = new Vector2();
            // t computation
            polarDirection.Y = (float) (Math.Acos(viewDir.Y) / Math.PI);

            // s computation
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            polarDirection.X = viewDir.X == 0.0 ? (float)(Math.PI / 2 * Math.Sign(viewDir.Z)) : (float)(Math.Atan2(viewDir.Z, viewDir.X));
            polarDirection.X = (float) (polarDirection.X / (2.0 * Math.PI) + 0.25);

            if (polarDirection.X < 0.0)
                polarDirection.X += 1.0f;
            if (polarDirection.Y < 0.0)
                polarDirection.Y += 1.0f;

            window.StatusBar.SetMouseCoordinates((int)(polarDirection.X * Context.GetWidth((int)Context.ActiveMipmap)), 
                (int)(polarDirection.Y * Context.GetHeight((int)Context.ActiveMipmap)));
        }

        private Matrix4 GetLeftHandedOrientation()
        {
            return Matrix4.CreateScale(1.0f, -1.0f, 1.0f);
        }
    }
}
