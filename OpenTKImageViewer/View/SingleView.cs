using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using OpenTKImageViewer.View.Shader;

namespace OpenTKImageViewer.View
{
    public class SingleView : VertexArrayView
    {
        private ImageContext.ImageContext context;
        private ViewShader shader;

        public SingleView(ImageContext.ImageContext context)
        {
            this.context = context;
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

            // draw via vertex array
            base.Draw();
        }

        public override void OnDrag(Vector diff)
        {

        }

        public override void OnScroll(double diff, Point mouse)
        {

        }
    }
}
