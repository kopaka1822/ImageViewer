using ImageFramework.DirectX;
using ImageFramework.Utility;
using ImageViewer.Controller.TextureViews.Shader;
using ImageViewer.Controller.TextureViews.Shared;
using ImageViewer.Models;
using SharpDX;

namespace ImageViewer.Controller.TextureViews.Texture2D
{
    public class CubeTextureView : PolarTextureView
    {
        private readonly CubeViewShader shader;

        public CubeTextureView(ModelsEx models)
            : base(models)
        {
            shader = new CubeViewShader(models);
        }

        public override void Dispose()
        {
            shader?.Dispose();
            base.Dispose();
        }

        public override void Draw(ITexture texture)
        {
            if (texture == null) return;

            base.Draw(texture);

            var dev = Device.Get();
            dev.OutputMerger.BlendState = models.ViewData.AlphaBlendState;

            shader.Run(GetTransform(), CalcFarplane(), ((TextureArray2D)texture).GetCubeView(models.Display.ActiveMipmap));

            dev.OutputMerger.BlendState = models.ViewData.DefaultBlendState;
        }

        public override Size3 GetTexelPosition(Vector2 mouse)
        {
            // left handed coordinate system
            var transform = GetTransform();
            var preViewDir = new Vector4(mouse.X, -mouse.Y, CalcFarplane(), 0.0f);
            Vector4.Transform(ref preViewDir, ref transform, out var viewDir);

            viewDir.Normalize();

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
                float s = faces[i].X * viewDir.X +
                          faces[i].Y * viewDir.Y +
                          faces[i].Z * viewDir.Z;
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
            float r = -1.0f / (faces[maxIndex].X * viewDir.X + faces[maxIndex].Y * viewDir.Y + faces[maxIndex].Z * viewDir.Z);

            var intersectionPoint = viewDir * r;

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
                case 2:
                    sc *= -1.0f;
                    tc *= -1.0f;
                    break;
                case 1:
                    tc *= -1.0f;
                    break;
                case 3:
                case 4:
                    sc *= -1.0f;
                    break;
            }

            if (maxIndex == 0 || maxIndex == 1)
            {
                var t = sc;
                sc = tc;
                tc = t;
            }

            models.Display.ActiveLayer = maxIndex;

            var pt = Utility.CanonicalToTexelCoordinates(sc, tc,
                models.Images.GetWidth(models.Display.ActiveMipmap),
                models.Images.GetHeight(models.Display.ActiveMipmap));

            return new Size3(pt.X, pt.Y, 0);
        }

        protected override Matrix GetOrientation()
        {
            return Matrix.Scaling(1.0f);
        }
    }
}
