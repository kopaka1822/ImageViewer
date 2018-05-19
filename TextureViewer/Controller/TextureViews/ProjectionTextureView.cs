using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using TextureViewer.glhelper;

namespace TextureViewer.Controller.TextureViews
{
    public abstract class ProjectionTextureView : ITextureView
    {
        protected readonly Models.Models models;

        private float pitch = 0.0f;
        private float roll = 0.0f;

        protected ProjectionTextureView(Models.Models models)
        {
            this.models = models;
        }

        public virtual void Draw(TextureArray2D texture)
        {
            // draw the checkers background
            models.GlData.CheckersShader.Bind(Matrix4.Identity);
            models.GlData.Vao.DrawQuad();
        }

        public virtual void Dispose()
        {
            
        }

        public void OnScroll(float amount, Vector2 mouse)
        {
            //throw new NotImplementedException();
        }

        public void OnDrag(Vector2 diff)
        {
            pitch += (float)diff.X * 0.01f / models.Display.Aperture;
            roll += (float)diff.Y * 0.01f / models.Display.Aperture;
        }

        public Point GetTexelPosition(Vector2 mouse)
        {
            //throw new NotImplementedException();
            return new Point(0);
        }

        protected Matrix4 GetTransform()
        {
            return models.Display.ClientAspectRatio * GetRotation() * GetOrientation();
        }

        protected abstract Matrix4 GetOrientation();

        private Matrix4 GetRotation()
        {
            return Matrix4.CreateRotationX(roll) * Matrix4.CreateRotationY(pitch);
        }
    }
}
