using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using OpenTK;
using OpenTKImageViewer.glhelper;
using OpenTKImageViewer.UI;
using OpenTKImageViewer.View.Shader;

namespace OpenTKImageViewer.View
{
    public class SingleView : VertexArrayView
    {
        private ImageContext.ImageContext context;
        private SingleViewShader shader;
        private Matrix4 transform;
        private Matrix4 aspectRatio;
        private TextBox boxScroll;

        public SingleView(ImageContext.ImageContext context, TextBox boxScroll)
        {
            this.context = context;
            this.transform = Matrix4.Identity;
            this.boxScroll = boxScroll;
        }

        private void Init()
        {
            shader = new SingleViewShader();
        }

        public override void Update(MainWindow window)
        {
            base.Update(window);
            window.StatusBar.LayerMode = StatusBarControl.LayerModeType.Single;
            // update uniforms etc
            if (shader == null)
                Init();

            aspectRatio = GetAspectRatio(window.GetClientWidth(), window.GetClientHeight());

            boxScroll.Text = Math.Round((Decimal)(transform[0, 0] * 100.0f), 2).ToString() + "%";

            
        }

        public override void UpdateMouseDisplay(MainWindow window)
        {
            var mousePoint = window.StatusBar.GetCanonicalMouseCoordinates();
            // Matrix Coordinate system is reversed (left handed)
            var vec = new Vector4((float)mousePoint.X, (float)mousePoint.Y, 0.0f, 1.0f);//new Vector4(0.0f, 0.0f, 0.0f, 1.0f);
            var transMouse = vec * (GetOrientation() * transform * aspectRatio).Inverted();

            transMouse = MouseToTextureCoordinates(transMouse, 
                context.GetWidth((int) context.ActiveMipmap),
                context.GetHeight((int) context.ActiveMipmap));

            window.StatusBar.SetMouseCoordinates((int)(transMouse.X), (int)(transMouse.Y));
        }

        public override void SetZoom(float dec)
        {
            dec = Math.Min(Math.Max(dec, 0.01f), 100.0f);
            transform[0, 0] = dec;
            transform[1, 1] = dec;
        }

        public override void Draw()
        {
            // bind the shader?
            shader.Bind(context);
            shader.SetTransform(transform * aspectRatio);
            shader.SetLevel((float)context.ActiveMipmap);
            shader.SetLayer((float)context.ActiveLayer);
            shader.SetGrayscale(context.Grayscale);
            context.BindFinalTextureAs2DSamplerArray(shader.GetTextureLocation());
            glhelper.Utility.GLCheck();
            

            // draw via vertex array
            base.Draw();
            glhelper.Utility.GLCheck();
        }

        public Matrix4 GetAspectRatio(float clientWidth, float clientHeight)
        {
            return Matrix4.CreateScale(context.GetWidth(0) / clientWidth, context.GetHeight(0) / clientHeight, 1.0f);
        }

        public override void OnDrag(Vector diff, MainWindow window)
        {
            var vec = WindowToClient(diff);
            transform *= Matrix4.CreateTranslation((float)vec.X, (float)vec.Y, 0.0f);
        }

        public override void OnScroll(double diff, Point mouse)
        {
            var scale = Math.Min(Math.Max(transform[0,0] * (1.0 + (diff * 0.001)), 0.01), 100.0) / transform[0,0];
            transform *= Matrix4.CreateScale((float)scale, (float)scale, 1.0f);
        }

        private Vector WindowToClient(Vector vec)
        {
            return new Vector(
                vec.X * 2.0 / context.GetWidth(0),
                -vec.Y * 2.0 / context.GetHeight(0)
            );
        }

        private Matrix4 GetOrientation()
        {
            return Matrix4.CreateScale(1.0f, -1.0f, 1.0f);
        }
    }
}
