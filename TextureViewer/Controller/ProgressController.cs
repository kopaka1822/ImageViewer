using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TextureViewer.Utility;

namespace TextureViewer.Controller
{
    /// <summary>
    /// handles the image combination and filter progress
    /// </summary>
    public class ProgressController
    {
        private readonly Models.Models models;
        private readonly List<ImageCombineController> imageController;

        private IStepable currentWork = null;

        public ProgressController(Models.Models models)
        {
            this.models = models;
            this.imageController = new List<ImageCombineController>();
            for (var i = 0; i < models.Equations.NumEquations; ++i)
            {
                imageController.Add(new ImageCombineController(models.Equations.Get(i), models.FinalImages.Get(i), models, i));
            }
        }

        /// <summary>
        /// true if DoWork should be called to perform some work
        /// </summary>
        /// <returns></returns>
        public bool HasWork()
        {
            if (currentWork != null) return true;

            // check if the other controllers have something
            foreach (var ctrl in imageController)
            {
                currentWork = ctrl.GetWork();
                if (currentWork != null)
                    return true;
            }

            return false;
        }

        /// <summary>
        /// perform enqueued task (i.e. calculating image)
        /// </summary>
        public void DoWork()
        {
            models.Progress.IsProcessing = true;

            Debug.Assert(currentWork != null);

            // advance the first stepable
            if (currentWork.HasStep())
            {
                currentWork.NextStep();
            }

            if (!currentWork.HasStep())
            {
                // remove this element
                currentWork = null;
                if (!HasWork())
                {
                    models.Progress.IsProcessing = false;
                }
            }
            else
            {
                // report progress
                models.Progress.What = currentWork.GetDescription();
                models.Progress.Progress = (float)currentWork.CurrentStep() / currentWork.GetNumSteps();
            }
        }
    }
}
