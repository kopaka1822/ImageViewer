using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using TextureViewer.Models;

namespace TextureViewer.Controller.TextureViews
{
    public class PlainTextureView : ITextureView
    {
        protected readonly OpenGlModel glModel;

        public PlainTextureView(OpenGlModel glModel)
        {
            this.glModel = glModel;
        }

        public void Draw()
        {
            

        }

        public void Dispose()
        {
            
        }

        public void DrawLayer(Matrix4 offset, uint layer, int imageId)
        {
            
        }

        private Matrix4 GetOrientation()
        {
            return Matrix4.CreateScale(1.0f, -1.0f, 1.0f);
        }
    }
}
