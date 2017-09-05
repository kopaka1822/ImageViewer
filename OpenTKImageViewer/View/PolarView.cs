using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using OpenTK;
using OpenTKImageViewer.UI;
using OpenTKImageViewer.View.Shader;

namespace OpenTKImageViewer.View
{
    public class PolarView : VertexArrayView
    {
        private ImageContext.ImageContext context;
        private PolarViewShader shader;
        private Matrix4 aspectRatio;
        private float yawn = 0.0f;
        private float pitch = 0.0f;
        private float roll = 0.0f;
        private float zoom = 2.0f;

        public PolarView(ImageContext.ImageContext context)
        {
            this.context = context;
        }

        private void Init()
        {
            shader = new PolarViewShader();
        }

        public override void Update(MainWindow window)
        {
            base.Update(window);

            window.StatusBar.LayerMode = StatusBarControl.LayerModeType.Single;

            aspectRatio = GetAspectRatio(window.GetClientWidth(), window.GetClientHeight());
            // update uniforms etc
            if (shader == null)
                Init();
        }

        public override void Draw()
        {
            // bind the shader?
            shader.Bind(context);
            shader.SetTransform(aspectRatio * GetRotation() * GetOrientation());
            shader.SetLevel((float)context.ActiveMipmap);
            shader.SetLayer((float)context.ActiveLayer);
            shader.SetFarplane(zoom);
            shader.SetGrayscale(context.Grayscale);
            context.BindFinalTextureAs2DSamplerArray(shader.GetTextureLocation());

            // draw via vertex array
            base.Draw();
        }

        private Matrix4 GetRotation()
        {
            return Matrix4.CreateRotationX(roll) * Matrix4.CreateRotationY(pitch) * Matrix4.CreateRotationZ(yawn);
        }

        private Matrix4 GetOrientation()
        {
            return Matrix4.CreateScale(-1.0f, -1.0f, 1.0f);
        }

        public Matrix4 GetAspectRatio(float clientWidth, float clientHeight)
        {
            return Matrix4.CreateScale(clientWidth / clientHeight, 1.0f, 1.0f);
        }

        public override void OnDrag(Vector diff, MainWindow window)
        {
            pitch += (float)diff.X * 0.01f;
            roll += (float)diff.Y * 0.01f;
        }

        public override void OnScroll(double diff, Point mouse)
        {
            zoom = (float)Math.Min(Math.Max(zoom * (1.0 + (diff * 0.001)), 0.1), 100.0);
        }

    }
}
