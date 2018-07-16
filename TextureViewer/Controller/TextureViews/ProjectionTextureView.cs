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
            // modify zoom
            var step = amount > 0.0f ? 1.0f / 1.001f : 1.001f;
            var value = (float)Math.Pow(step, Math.Abs(amount));

            models.Display.Aperture *= value;
        }

        public void OnDrag(Vector2 diff)
        {
            pitch += (float)diff.X * 0.01f * models.Display.Aperture;
            roll += (float)diff.Y * 0.01f * models.Display.Aperture;
        }

        public virtual Point GetTexelPosition(Vector2 mouse)
        {
            throw new NotImplementedException();
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

        protected float CalcFarplane()
        {
            // distance from camera. ray direction in the edges will be vec3(+-1, +-1, farplane)
            return (float)(1.0 / Math.Tan(models.Display.Aperture / 2.0));
        }

        protected Matrix4 GetLeftHandedOrientation()
        {
            return Matrix4.CreateScale(1.0f, -1.0f, 1.0f);
        }
    }
}
