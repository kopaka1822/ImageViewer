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
using OpenTKImageViewer.glhelper;
using OpenTKImageViewer.UI;
using OpenTKImageViewer.View.Shader;

namespace OpenTKImageViewer.View
{
    public class CubeView : ProjectionView
    {
        private CubeViewShader shader;

        public CubeView(ImageContext.ImageContext context, TextBox boxScroll)
            :
            base(context, boxScroll)
        {
        }

        public override void Dispose()
        {
            shader?.Dispose();

            base.Dispose();
        }

        protected override void Init()
        {
            base.Init();
            shader = new CubeViewShader();
        }

        public override void Draw(int activeImage)
        {
            DrawCheckers();

            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);

            shader.Bind(Context);
            shader.SetTransform(GetTransform());
            shader.SetFarplane(GetZoom());
            shader.SetLevel((float)Context.ActiveMipmap);
            shader.SetGrayscale(Context.Grayscale);
            Context.BindFinalTextureAsCubeMap(activeImage, shader.GetTextureLocation());
            // draw via vertex array
            DrawQuad();

            GL.Disable(EnableCap.Blend);
            Program.Unbind();
        }

        protected override Matrix4 GetOrientation()
        {
            return Matrix4.CreateScale(1.0f, 1.0f, 1.0f);
        }
        
        public override void UpdateMouseDisplay(MainWindow window)
        {
            var mousePoint = window.StatusBar.GetCanonicalMouseCoordinates();

            // left handed coordinate system
            var viewDir = new Vector4((float)mousePoint.X, (float)mousePoint.Y, GetZoom(), 0.0f) * GetTransform();
            var dir = new Vector3(viewDir.X, viewDir.Y, viewDir.Z).Normalized();

            // TODO determine pixel coordinate from view dir
            Vector3[] faces = new Vector3[6];
            faces[0] = new Vector3(1.0f, 0.0f, 0.0f);
            faces[1] = new Vector3(-1.0f, 0.0f, 0.0f);
            faces[2] = new Vector3(0.0f, 1.0f, 0.0f);
            faces[3] = new Vector3(0.0f, -1.0f, 0.0f);
            faces[4] = new Vector3(0.0f, 0.0f, 1.0f);
            faces[5] = new Vector3(0.0f, 0.0f, -1.0f);

            float maxScalar = -1.0f;
            int maxIndex = 0;
            for (int i = 0; i < 6; ++i)
            {
                float s = faces[i].X * dir.X +
                          faces[i].Y * dir.Y +
                          faces[i].Z * dir.Z;
                if (s > maxScalar)
                {
                    maxScalar = s;
                    maxIndex = i;
                }
            }

            // determine texture coordinates from view direction

            // 3. normal form: faces[maxIndex].X * x1 + faces[maxIndex].Y * x2 + faces[maxIndex].Z * x3 + 1 = 0
            // line: (0 0 0) + r * dir
            // solve: faces[maxIndex].X * r * dir.X + faces[maxIndex].Y * r * dir.Y + faces[maxIndex].Z * r * dir.Z + 1 = 0
            // solve: faces[maxIndex].X * r * dir.X + faces[maxIndex].Y * r * dir.Y + faces[maxIndex].Z * r * dir.Z = -1
            // solve: faces[maxIndex].X * dir.X + faces[maxIndex].Y * dir.Y + faces[maxIndex].Z * dir.Z = -1 / r
            // solve: -1 * (faces[maxIndex].X * dir.X + faces[maxIndex].Y * dir.Y + faces[maxIndex].Z * dir.Z) = 1 / r
            // solve: -1 / (faces[maxIndex].X * dir.X + faces[maxIndex].Y * dir.Y + faces[maxIndex].Z * dir.Z) = r
            
            // intersection
            float r = -1.0f / (faces[maxIndex].X * dir.X + faces[maxIndex].Y * dir.Y + faces[maxIndex].Z * dir.Z);

            var intersectionPoint = dir * r;

            // determine s and t coordinates
            int xIndex = (faces[maxIndex].X != 0.0f) ? 1 : 0;
            int yIndex = (xIndex == 0) ? ((faces[maxIndex].Y != 0.0f) ? 2 : 1) : 2;

            float sc = intersectionPoint[xIndex];
            float tc = intersectionPoint[yIndex];

            // some tricking to get the coordinates right
            switch (maxIndex)
            {
                case 0:
                    sc *= -1.0f;
                    break;
                case 2:
                    sc *= -1.0f;
                    break;
                case 1:
                case 3:
                case 4:
                    sc *= -1.0f;
                    tc *= -1.0f;
                    break;
                case 5:
                    tc *= -1.0f;
                    break;
            }

            if (maxIndex == 0 || maxIndex == 1)
            {
                var t = sc;
                sc = tc;
                tc = t;
            }
            
            var transMouse = MouseToTextureCoordinates(new Vector4(sc, tc, 0.0f, 0.0f),
                Context.GetWidth((int)Context.ActiveMipmap),
                Context.GetHeight((int)Context.ActiveMipmap));

            window.StatusBar.SetMouseCoordinates((int)(transMouse.X), (int)(transMouse.Y));
            window.Context.ActiveLayer = (uint)maxIndex;
        }
    }
}
