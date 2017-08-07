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

        public SingleView(ImageContext.ImageContext context)
        {
            this.context = context;
            this.transform = Matrix4.Identity;
        }

        private void Init()
        {
            shader = new SingleViewShader();
        }

        public override void Update()
        {
            base.Update();
            // update uniforms etc
            if(shader == null)
                Init();
        }

        public override void Draw()
        {
            // bind the shader?
            shader.Bind(context);
            shader.SetTransform(transform);

            // draw via vertex array
            base.Draw();
        }

        public override void OnDrag(Vector diff)
        {
            transform *= Matrix4.CreateTranslation((float) diff.X, (float) diff.Y, 0.0f);
        }

        public override void OnScroll(double diff, Point mouse)
        {

        }
    }
}
