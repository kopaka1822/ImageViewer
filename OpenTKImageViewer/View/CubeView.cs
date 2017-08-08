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
    public class CubeView : VertexArrayView
    {
        private ImageContext.ImageContext context;
        private CubeViewShader shader;
        private float yawn = 0.0f;
        private float pitch = 0.0f;
        private float roll = 0.0f;

        public CubeView(ImageContext.ImageContext context)
        {
            this.context = context;
        }

        private void Init()
        {
            shader = new CubeViewShader();
        }

        public override void Update(MainWindow window)
        {
            base.Update(window);

            if(shader == null)
                Init();
        }

        public override void Draw()
        {
            shader.Bind(context);
            shader.SetTransform(GetRotation() * GetOrientation());

            context.BindFinalTextureAsCubeMap(shader.GetTextureLocation());
            // draw via vertex array
            base.Draw();
        }

        private Matrix4 GetRotation()
        {
            return  Matrix4.CreateRotationX(roll) * Matrix4.CreateRotationY(pitch) * Matrix4.CreateRotationZ(yawn);
        }

        private Matrix4 GetOrientation()
        {
            return Matrix4.CreateScale(1.0f, -1.0f, 1.0f);
        }

        public override void OnDrag(Vector diff, MainWindow window)
        {
            pitch += (float)diff.X * 0.01f;
            roll += (float)diff.Y * 0.01f;
        }

        public override void OnScroll(double diff, Point mouse)
        {
            base.OnScroll(diff, mouse);
        }
    }
}
