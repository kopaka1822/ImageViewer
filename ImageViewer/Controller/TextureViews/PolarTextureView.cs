using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageFramework.DirectX;
using ImageViewer.Models;
using SharpDX;
using Point = System.Drawing.Point;

namespace ImageViewer.Controller.TextureViews
{
    public class PolarTextureView : ITextureView
    {
        private readonly ModelsEx models;

        public PolarTextureView(ModelsEx models)
        {
            this.models = models;
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public void Draw(TextureArray2D texture)
        {
            throw new NotImplementedException();
        }

        public void OnScroll(float amount, Vector2 mouse)
        {
            throw new NotImplementedException();
        }

        public void OnDrag(Vector2 diff)
        {
            throw new NotImplementedException();
        }

        public Point GetTexelPosition(Vector2 mouse)
        {
            throw new NotImplementedException();
        }
    }
}
