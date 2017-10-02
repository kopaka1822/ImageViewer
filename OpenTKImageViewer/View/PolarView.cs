using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using OpenTK;
using OpenTK.Graphics.OpenGL4;
using OpenTKImageViewer.UI;
using OpenTKImageViewer.View.Shader;

namespace OpenTKImageViewer.View
{
    public class PolarView : VertexArrayView
    {
        private ImageContext.ImageContext context;
        private PolarViewShader shader;
        private CheckersShader checkersShader;
        private Matrix4 aspectRatio;
        private float yawn = 0.0f;
        private float pitch = 0.0f;
        private float roll = 0.0f;
        private float zoom = 2.0f;
        private readonly TextBox boxScroll;

        public PolarView(ImageContext.ImageContext context, TextBox boxScroll)
        {
            this.context = context;
            this.boxScroll = boxScroll;
        }

        public void SetZoomFarplane(float dec)
        {
            zoom = Math.Min(Math.Max(dec, 0.5f), 100.0f);
        }

        /// <summary>
        /// set zoom in radians
        /// </summary>
        /// <param name="dec">desired angle in radians</param>
        public override void SetZoom(float dec)
        {
            var degree = dec * Math.PI / 180.0;
            SetZoomFarplane((float) (1.0 / ( 2.0 * Math.Tan(degree / 2.0))));
        }

        private void Init()
        {
            shader = new PolarViewShader();
            checkersShader = new CheckersShader();
        }

        public override void Update(MainWindow window)
        {
            base.Update(window);

            window.StatusBar.LayerMode = StatusBarControl.LayerModeType.Single;

            aspectRatio = GetAspectRatio(window.GetClientWidth(), window.GetClientHeight());
            // update uniforms etc
            if (shader == null)
                Init();

            // recalculate zoom to degrees
            var angle = 2.0 * Math.Atan(1.0 / (2.0 * (double) zoom));
            
            boxScroll.Text = Math.Round((Decimal)(angle / Math.PI * 180.0), 2).ToString(CultureInfo.InvariantCulture) + "°";
            //boxScroll.Text = Math.Round((Decimal)(zoom * 100, 2).ToString(CultureInfo.InvariantCulture) + "°";
        }

        public override void Draw()
        {
            checkersShader.Bind(Matrix4.Identity);
            base.Draw();
            glhelper.Utility.GLCheck();

            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);

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

            GL.Disable(EnableCap.Blend);
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
            SetZoomFarplane((float) (zoom * (1.0 + (diff * 0.001))));
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
