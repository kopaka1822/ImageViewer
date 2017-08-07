using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using OpenTK;
using OpenTKImageViewer.View.Shader;

namespace OpenTKImageViewer.View
{
    public class SingleView : VertexArrayView
    {
        private ImageContext.ImageContext context;
        private SingleViewShader shader;
        private Matrix4 transform;
        private Matrix4 aspectRatio;

        public SingleView(ImageContext.ImageContext context)
        {
            this.context = context;
            this.transform = Matrix4.Identity;
        }

        private void Init()
        {
            shader = new SingleViewShader();
        }

        public override void Update(MainWindow window)
        {
            base.Update(window);
            // update uniforms etc
            if(shader == null)
                Init();

            aspectRatio = GetAspectRatio(window.GetClientWidth(), window.GetClientHeight());
        }

        public override void Draw()
        {
            // bind the shader?
            shader.Bind(context);
            shader.SetTransform(transform * aspectRatio);
            context.BindFinalTextureAs2DSamplerArray(shader.GetTextureLocation());

            // draw via vertex array
            base.Draw();
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
            // TODO mip
            return new Vector(
                vec.X * 2.0 / context.GetWidth(0),
                -vec.Y * 2.0 / context.GetHeight(0)
            );
        }
    }
}
