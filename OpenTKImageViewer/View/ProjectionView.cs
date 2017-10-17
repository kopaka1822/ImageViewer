using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using OpenTK;
using OpenTKImageViewer.UI;
using OpenTKImageViewer.View.Shader;

namespace OpenTKImageViewer.View
{
    /// <summary>
    /// features for basic projection used in polar view an cube map view
    /// </summary>
    public abstract class ProjectionView : VertexArrayView
    {
        protected readonly ImageContext.ImageContext Context;
        private CheckersShader checkersShader;
        private readonly TextBox boxScroll;
        private float yawn = 0.0f;
        private float pitch = 0.0f;
        private float roll = 0.0f;
        private float zoom = 2.0f;
        private Matrix4 aspectRatio;

        protected ProjectionView(ImageContext.ImageContext context, TextBox boxScroll)
        {
            this.Context = context;
            this.boxScroll = boxScroll;
        }

        protected virtual void Init()
        {
            checkersShader = new CheckersShader();
        }

        public override void Update(MainWindow window)
        {
            base.Update(window);
            window.StatusBar.LayerMode = StatusBarControl.LayerModeType.SingleDeactivated;

            aspectRatio = GetAspectRatio(window.GetClientWidth(), window.GetClientHeight());

            if (checkersShader == null)
                Init();

            // recalculate zoom to degrees
            var angle = 4.0 * Math.Atan(1.0 / (2.0 * (double)zoom));

            boxScroll.Text = Math.Round((Decimal)(angle / Math.PI * 180.0), 2).ToString(CultureInfo.InvariantCulture) + "°";
        }

        private Matrix4 GetAspectRatio(float clientWidth, float clientHeight)
        {
            return Matrix4.CreateScale(clientWidth / clientHeight, 1.0f, 1.0f);
        }

        private void SetZoomFarplane(float dec)
        {
            zoom = Math.Min(Math.Max(dec, 0.6f), 100.0f);
        }

        /// <summary>
        /// set zoom in radians
        /// </summary>
        /// <param name="dec">desired angle in radians</param>
        public override void SetZoom(float dec)
        {
            var degree = dec * Math.PI / 180.0;
            SetZoomFarplane((float)(1.0 / (2.0 * Math.Tan(degree / 4.0))));
        }

        protected Matrix4 GetTransform()
        {
            return aspectRatio * GetRotation() * GetOrientation();
        }

        private Matrix4 GetRotation()
        {
            return Matrix4.CreateRotationX(roll) * Matrix4.CreateRotationY(pitch) * Matrix4.CreateRotationZ(yawn);
        }

        protected abstract Matrix4 GetOrientation();

        public override void OnDrag(Vector diff, MainWindow window)
        {
            pitch += (float)diff.X * 0.01f / zoom;
            roll += (float)diff.Y * 0.01f / zoom;
        }

        public override void OnScroll(double diff, Point mouse)
        {
            SetZoomFarplane((float)(zoom * (1.0 + (diff * 0.001))));
        }

        protected float GetZoom()
        {
            return zoom;
        }

        protected void DrawCheckers()
        {
            checkersShader.Bind(Matrix4.Identity);
            DrawQuad();
        }
    }
}
