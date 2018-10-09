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
            Debug.Assert(models.GlContext.GlEnabled);

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
                var tex = models.Images.GetTexture(i);
                shader.BindSourceImage(tex, models.GlData.GetSampler(tex.HasMipmaps, false), i);
            }

            for (var layer = 0; layer < models.Images.NumLayers; ++layer)
            {
                shader.Run(layer, 0, models.Images.Width, models.Images.Height, target);
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
