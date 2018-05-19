using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Graphics.OpenGL4;
using TextureViewer.Controller.TextureViews.Shader;
using TextureViewer.glhelper;

namespace TextureViewer.Controller.TextureViews
{
    public class CubeTextureView : ProjectionTextureView
    {
        private readonly CubeViewShader shader;

        public CubeTextureView(Models.Models models) : base(models)
        {
            shader = new CubeViewShader();
        }

        protected override Matrix4 GetOrientation()
        {
            return Matrix4.CreateScale(1.0f, -1.0f, 1.0f);
        }

        public override void Draw(TextureArray2D texture)
        {
            // draw checkerss
            base.Draw(texture);

            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);

            shader.Bind();
            shader.SetTransform(GetTransform());
            shader.SetFarplane(CalcFarplane());
            shader.SetLevel((float)models.Display.ActiveMipmap);
            shader.SetGrayscale(models.Display.Grayscale);

            models.GlData.BindSampler(shader.GetTextureLocation(), true, models.Display.LinearInterpolation);
            texture.BindAsCubemap(shader.GetTextureLocation());
            // draw via vertex array
            models.GlData.Vao.DrawQuad();

            GL.Disable(EnableCap.Blend);
            Program.Unbind();
        }

        public override void Dispose()
        {
            shader.Dispose();

            base.Dispose();
        }

        public override Point GetTexelPosition(Vector2 mouse)
        {
            // left handed coordinate system
            var viewDir = new Vector4((float)mouse.X, (float)mouse.Y, CalcFarplane(), 0.0f) * GetTransform();
            var dir = new Vector3(viewDir.X, viewDir.Y, viewDir.Z).Normalized();

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
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            int xIndex = (faces[maxIndex].X != 0.0f) ? 1 : 0;
            // ReSharper disable once CompareOfFloatsByEqualityOperator
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

            models.Display.ActiveLayer = maxIndex;

            return Utility.Utility.CanonicalToTexelCoordinates(
                new Vector2(sc, tc),
                models.Images.GetWidth(models.Display.ActiveMipmap),
                models.Images.GetHeight(models.Display.ActiveMipmap));
        }
    }
}
