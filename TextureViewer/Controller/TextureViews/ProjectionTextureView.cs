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

        public Point GetTexelPosition(Vector2 mouse)
        {
            // calculate farplane
            var viewDir = new Vector4(mouse.X, mouse.Y, CalcFarplane(), 0.0f);
            viewDir = viewDir * GetTransform() * GetLeftHandedOrientation();
            viewDir.Normalize();

            // determine pixel coordinate from view dir
            var polarDirection = new Vector2();
            // t computation
            polarDirection.Y = (float) (Math.Acos(viewDir.Y) / Math.PI);

            //  computation
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            polarDirection.X = viewDir.X == 0.0 ? (float)(Math.PI / 2 * Math.Sign(viewDir.Z)) : (float)(Math.Atan2(viewDir.Z, viewDir.X));
            polarDirection.X = (float)(polarDirection.X / (2.0 * Math.PI) + 0.25);

            if (polarDirection.X < 0.0)
                polarDirection.X += 1.0f;
            if (polarDirection.Y < 0.0)
                polarDirection.Y += 1.0f;

            return Utility.Utility.CanonicalToTexelCoordinates(
                (polarDirection * 2.0f) - Vector2.One,
                models.Images.GetWidth(models.Display.ActiveMipmap),
                models.Images.GetHeight(models.Display.ActiveMipmap));
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

        private Matrix4 GetLeftHandedOrientation()
        {
            return Matrix4.CreateScale(1.0f, -1.0f, 1.0f);
        }
    }
}
