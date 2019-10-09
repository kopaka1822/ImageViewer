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
    public abstract class ProjectionTextureView : ITextureView
    {
        protected readonly ModelsEx models;
        protected readonly TextureViewData data;
        private float pitch = 0.0f;
        private float roll = 0.0f;

        public ProjectionTextureView(ModelsEx models, TextureViewData data)
        {
            this.models = models;
            this.data = data;
        }
        public virtual void Dispose()
        {}

        public virtual void Draw(TextureArray2D texture)
        {
            data.Checkers.Run(data.Buffer, Matrix.Identity);
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

        protected Matrix GetTransform()
        {
            return models.Display.ClientAspectRatio * GetRotation() * GetOrientation();
        }

        protected abstract Matrix GetOrientation();

        private Matrix GetRotation()
        {
            return
                Matrix.RotationX(roll) *
                Matrix.RotationY(pitch);
        }

        protected float CalcFarplane()
        {
            // distance from camera. ray direction in the edges will be vec3(+-1, +-1, farplane)
            return (float)(1.0 / Math.Tan(models.Display.Aperture / 2.0));
        }

        protected Matrix GetLeftHandedOrientation()
        {
            return Matrix.Scaling(1.0f, -1.0f, 1.0f);
        }
    }
}
