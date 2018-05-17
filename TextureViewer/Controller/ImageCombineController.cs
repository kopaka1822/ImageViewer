using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TextureViewer.Controller.ImageCombination;
using TextureViewer.Models;
using TextureViewer.Utility;

namespace TextureViewer.Controller
{
    public class ImageCombineController
    {
        private readonly ImageEquationModel equation;
        private readonly FinalImageModel finalImage;
        private readonly Models.Models models;

        // indicates if the image should be recomputed
        private bool recomputeImage = false;

        public ImageCombineController(ImageEquationModel equation, FinalImageModel finalImage, Models.Models models)
        {
            this.equation = equation;
            this.finalImage = finalImage;
            this.models = models;
            equation.ColorFormula.PropertyChanged += FormulaOnPropertyChanged;
            equation.ColorFormula.PropertyChanged += FormulaOnPropertyChanged;
            equation.PropertyChanged += EquationOnPropertyChanged;
            this.models.Images.PropertyChanged += ImagesOnPropertyChanged;
        }

        private void ImagesOnPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            switch (args.PropertyName)
            {
                case nameof(ImagesModel.NumImages):
                    if (models.Images.PrevNumImages == 0)
                    {
                        recomputeImage = true;
                        // issue redraw to do work
                        models.GlContext.RedrawFrame();
                    }
                    break;
            }
        }

        /// <summary>
        /// checks if anything needs to be recomputed. will be called if the window is repainted
        /// </summary>
        /// <returns>null if nothing needs to be recomputet, IStepable Instance otherwise</returns>
        public IStepable GetWork()
        {
            if (!recomputeImage || !equation.Visible) return null;
            recomputeImage = false;
            return MakeStepable();
        }

        private void EquationOnPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            switch (args.PropertyName)
            {
                case nameof(ImageEquationModel.Visible):
                    recomputeImage = true;
                    // issue redraw to do work
                    models.GlContext.RedrawFrame();
                    break;
                case nameof(ImageEquationModel.UseFilter):
                    if(equation.Visible)
                        models.GlContext.RedrawFrame();
                    break;
            }
        }

        private void FormulaOnPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            if (args.PropertyName.Equals(nameof(FormulaModel.Converted)))
            {
                recomputeImage = true;
                // issue redraw to do work
                models.GlContext.RedrawFrame();
            }
        }

        private IStepable MakeStepable()
        {
            var builder = new ImageCombineBuilder(models.GlData.TextureCache);

            var steps = new List<IStepable>();
            steps.Add(new ImageCombineStepable(equation, finalImage, models, builder));
            if (equation.UseFilter)
            {
                // TODO add filter
            }
            steps.Add(new FinalImageStepable(builder, finalImage));

            return new StepList(steps);
        }
    }
}
