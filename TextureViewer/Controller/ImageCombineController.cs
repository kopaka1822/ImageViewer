using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TextureViewer.Controller.Filter;
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
            equation.AlphaFormula.PropertyChanged += FormulaOnPropertyChanged;
            equation.PropertyChanged += EquationOnPropertyChanged;
            this.models.Images.PropertyChanged += ImagesOnPropertyChanged;
            this.models.Filter.Changed += FilterOnChanged;
        }

        private void FilterOnChanged(object sender, EventArgs eventArgs)
        {
            if (equation.UseFilter)
            {
                recomputeImage = true;
                models.GlContext.RedrawFrame();
            }
        }

        private void ImagesOnPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            switch (args.PropertyName)
            {
                case nameof(ImagesModel.NumImages):
                    if (models.Images.PrevNumImages == 0 || models.Images.PrevNumImages > models.Images.NumImages)
                    {
                        recomputeImage = true;
                        // issue redraw to do work
                        models.GlContext.RedrawFrame();
                    }
                    break;
            }
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
                    recomputeImage = true;
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

        /// <summary>
        /// checks if anything needs to be recomputed. will be called if the window is repainted
        /// </summary>
        /// <returns>null if nothing needs to be recomputet, IStepable Instance otherwise</returns>
        public IStepable GetWork()
        {
            if (!recomputeImage || !equation.Visible || models.Images.NumImages == 0) return null;
            recomputeImage = false;
            return MakeStepable();
        }

        private IStepable MakeStepable()
        {
            var builder = new ImageCombineBuilder(models.GlData.TextureCache);

            var steps = new List<IStepable> {new ImageCombineStepable(equation, finalImage, models, builder)};
            if (equation.UseFilter)
            {
                for (int i = 0; i < models.Filter.NumFilter; ++i)
                {
                    if(i == models.Filter.StatisticsPoint)
                        steps.Add(new StatisticsSaveStepable(builder));

                    steps.Add(models.Filter.Filter[i].MakeStepable(models, builder));
                }
            }
            steps.Add(new FinalImageStepable(builder, finalImage));

            return new StepList(steps);
        }
    }
}
