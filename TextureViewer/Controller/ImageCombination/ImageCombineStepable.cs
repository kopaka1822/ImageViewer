using System.Diagnostics;
using OpenTK.Graphics.OpenGL4;
using TextureViewer.Models;
using TextureViewer.Utility;

namespace TextureViewer.Controller.ImageCombination
{
    public class ImageCombineStepable : IStepable
    {
        private int curStep = 0;

        private readonly ImageEquationModel equation;
        private readonly FinalImageModel finalImage;
        private readonly Models.Models models;
        private readonly ImageCombineBuilder builder;

        public ImageCombineStepable(
            ImageEquationModel equation, 
            FinalImageModel finalImage, 
            Models.Models models, 
            ImageCombineBuilder builder)
        {
            this.equation = equation;
            this.finalImage = finalImage;
            this.models = models;
            this.builder = builder;
        }

        public int GetNumSteps()
        {
            return 1;
        }

        public int CurrentStep()
        {
            return curStep;
        }

        public void NextStep()
        {
            Debug.Assert(curStep == 0);
            Debug.Assert(models.GlContext.IsEnabled);

            // make the combine shader
            var shader = new ImageCombineShader(
                equation.ColorFormula.Converted, 
                equation.AlphaFormula.Converted,
                models.Images.NumImages
            );

            // determine the final image
            finalImage.Reset();

            // render into the primary texture
            var target = builder.GetPrimaryTexture();

            // bind source images
            for (int i = 0; i < models.Images.NumImages; ++i)
            {
                models.GlData.BindSampler(i, models.Images.GetTexture(i).HasMipmaps, false);
                models.Images.GetTexture(i).Bind(shader.GetSourceImageBinding(i));
            }

            for (var layer = 0; layer < models.Images.NumLayers; ++layer)
            {
                for (var mipmap = 0; mipmap < models.Images.NumMipmaps; ++mipmap)
                {
                    shader.Run(layer, mipmap, models.Images.GetWidth(mipmap), models.Images.GetHeight(mipmap), target);
                }
            }

            GL.MemoryBarrier(MemoryBarrierFlags.AllBarrierBits);

            shader.Dispose();

            ++curStep;
        }

        public bool HasStep()
        {
            return curStep == 0;
        }

        public string GetDescription()
        {
            return "combining images";
        }
    }
}
