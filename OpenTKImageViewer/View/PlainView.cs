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
using OpenTKImageViewer.glhelper;
using OpenTKImageViewer.UI;
using OpenTKImageViewer.View.Shader;

namespace OpenTKImageViewer.View
{
    /// <summary>
    /// features for basic plain image view used in single view and cube cross view
    /// </summary>
    public abstract class PlainView : VertexArrayView
    {
        protected readonly ImageContext.ImageContext Context;
        private SingleViewShader shader;
        private CheckersShader checkersShader;
        private readonly StatusBarControl.LayerModeType layerMode;
        private Matrix4 transform = Matrix4.Identity;
        private Matrix4 aspectRatio;
        private readonly TextBox boxScroll;

        protected PlainView(ImageContext.ImageContext context, TextBox boxScroll, StatusBarControl.LayerModeType layerMode)
        {
            Context = context;
            this.layerMode = layerMode;
            this.boxScroll = boxScroll;
        }

        public override void Dispose()
        {
            checkersShader?.Dispose();
            shader?.Dispose();

            base.Dispose();
        }

        private void Init()
        {
            shader = new SingleViewShader();
            checkersShader = new CheckersShader();
        }

        public override void Update(MainWindow window)
        {
            base.Update(window);
            window.StatusBar.LayerMode = layerMode;
            // update uniforms etc
            if (shader == null)
                Init();

            aspectRatio = GetAspectRatio(window.GetClientWidth(), window.GetClientHeight());

            boxScroll.Text = Math.Round((Decimal)(transform[0, 0] * 100.0f), 2).ToString(App.GetCulture()) + "%";


        }

        /// <summary>
        /// transforms mouse coordinates into opengl space
        /// </summary>
        /// <param name="window">window with current mouse coordinates</param>
        /// <returns>vector with correct x and y coordinates</returns>
        protected Vector4 GetOpenGLMouseCoordinates(MainWindow window)
        {
            var mousePoint = window.StatusBar.GetCanonicalMouseCoordinates();
            // Matrix Coordinate system is reversed (left handed)
            var vec = new Vector4((float)mousePoint.X, (float)mousePoint.Y, 0.0f, 1.0f);//new Vector4(0.0f, 0.0f, 0.0f, 1.0f);
            return vec * (GetOrientation() * transform * aspectRatio).Inverted();
        }
        
        /// <summary>
        /// set zoom in percent
        /// </summary>
        /// <param name="dec">zoom in percent</param>
        public override void SetZoom(float dec)
        {
            dec *= 0.01f;
            dec = Math.Min(Math.Max(dec, 0.01f), 100.0f);
            transform[0, 0] = dec;
            transform[1, 1] = dec;
        }

        protected void DrawLayer(Matrix4 offset, uint layer, int imageId)
        {
            var finalTrans = offset * transform * aspectRatio;

            checkersShader.Bind(finalTrans);
            DrawQuad();
            glhelper.Utility.GLCheck();

            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);

            shader.Bind(Context);
            shader.SetTransform(finalTrans);
            shader.SetLevel((float)Context.ActiveMipmap);
            shader.SetLayer((float)layer);
            shader.SetGrayscale(Context.Grayscale);
            Context.BindFinalTextureAs2DSamplerArray(imageId, shader.GetTextureLocation());
            glhelper.Utility.GLCheck();

            // draw via vertex array
            DrawQuad();
            glhelper.Utility.GLCheck();

            GL.Disable(EnableCap.Blend);
            Program.Unbind();
        }

        private Matrix4 GetAspectRatio(float clientWidth, float clientHeight)
        {
            return Matrix4.CreateScale(Context.GetWidth(0) / clientWidth, Context.GetHeight(0) / clientHeight, 1.0f);
        }

        public override void OnDrag(Vector diff, MainWindow window)
        {
            var vec = WindowToClient(diff);
            transform *= Matrix4.CreateTranslation((float)vec.X, (float)vec.Y, 0.0f);
        }

        public override void OnScroll(double diff, Point mouse)
        {
            var scale = Math.Min(Math.Max(transform[0, 0] * (1.0 + (diff * 0.001)), 0.01), 100.0) / transform[0, 0];
            transform *= Matrix4.CreateScale((float)scale, (float)scale, 1.0f);
        }

        private Vector WindowToClient(Vector vec)
        {
            return new Vector(
                vec.X * 2.0 / Context.GetWidth(0),
                -vec.Y * 2.0 / Context.GetHeight(0)
            );
        }

        private Matrix4 GetOrientation()
        {
            return Matrix4.CreateScale(1.0f, -1.0f, 1.0f);
        }
    }
}
