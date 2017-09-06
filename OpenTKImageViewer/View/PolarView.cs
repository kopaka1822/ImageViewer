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
            shader.SetTransform(GetTransform());
            shader.SetLevel((float)context.ActiveMipmap);
            shader.SetLayer((float)context.ActiveLayer);
            shader.SetFarplane(zoom);
            shader.SetGrayscale(context.Grayscale);
            context.BindFinalTextureAs2DSamplerArray(shader.GetTextureLocation());

            // draw via vertex array
            base.Draw();
        }

        private Matrix4 GetTransform()
        {
            return aspectRatio * GetRotation() * GetOrientation();
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
            pitch += (float)diff.X * 0.01f / zoom;
            roll += (float)diff.Y * 0.01f / zoom;
        }

        public override void OnScroll(double diff, Point mouse)
        {
            zoom = (float)Math.Min(Math.Max(zoom * (1.0 + (diff * 0.001)), 0.5), 100.0);
        }

        public override void UpdateMouseDisplay(MainWindow window)
        {
            var mousePoint = window.StatusBar.GetCanonicalMouseCoordinates();

            // Matrix Coordinate system is reversed (left handed)
            var viewDir = (new Vector4((float)mousePoint.X, (float)mousePoint.Y, zoom, 0.0f)) * (GetTransform() * GetLeftHandedOrientation());
            viewDir.Normalize();

            // determine pixel coordinate from view dir
            Vector2 polarDirection = new Vector2();
            // t computation
            polarDirection.Y = (float) (Math.Acos(viewDir.Y) / Math.PI);

            // s computation
            var normalizedDirection = new Vector3(viewDir.X, 0.0f, viewDir.Z).Normalized();
            if (normalizedDirection.X >= 0)
                polarDirection.X = (float) (Math.Acos(-normalizedDirection.Z) / (2.0 * Math.PI));
            else
                polarDirection.X = (float) ((Math.Acos(normalizedDirection.Z) + Math.PI) / (2.0 * Math.PI));
 
            //window.StatusBar.SetMouseCoordinates((int)(mousePoint.X * 100.0), (int)(mousePoint.Y * 100.0));
            window.StatusBar.SetMouseCoordinates((int)(polarDirection.X * context.GetWidth((int)context.ActiveMipmap)), 
                (int)(polarDirection.Y * context.GetHeight((int)context.ActiveMipmap)));
        }

        private Matrix4 GetLeftHandedOrientation()
        {
            return Matrix4.CreateScale(1.0f, -1.0f, 1.0f);
        }
    }
}
