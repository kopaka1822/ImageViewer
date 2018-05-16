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
        private readonly Queue<IStepable> workList = new Queue<IStepable>();
        private readonly List<ImageCombineController> imageController;

        public ProgressController(Models.Models models)
        {
            this.models = models;
            this.imageController = new List<ImageCombineController>();
            for (var i = 0; i < models.Equations.NumEquations; ++i)
            {
                imageController.Add(new ImageCombineController(models.Equations.Get(i), models.FinalImages.Get(i), models));
            }
        }

        void DispatchWork(IStepable work)
        {
            workList.Enqueue(work);
        }

        /// <summary>
        /// true if DoWork should be called to perform some work
        /// </summary>
        /// <returns></returns>
        public bool HasWork()
        {
            // check if the other controllers have something
            foreach (var ctrl in imageController)
            {
                var work = ctrl.GetWork();
                if(work != null)
                    DispatchWork(work);
            }

            return workList.Count > 0;
        }

        /// <summary>
        /// perform enqueued task (i.e. calculating image)
        /// </summary>
        public void DoWork()
        {
            models.Progress.IsProcessing = true;

            Debug.Assert(workList.Count != 0);

            var e = workList.Peek();

            // advance the first stepable
            if (e.HasStep())
            {
                e.NextStep();
            }

            if (!e.HasStep())
            {
                // remove this element
                workList.Dequeue();
                if (workList.Count == 0)
                {
                    models.Progress.IsProcessing = false;
                }
            }
            else
            {
                // report progress
                models.Progress.What = e.GetDescription();
                models.Progress.Progress = (float)e.CurrentStep() / e.GetNumSteps();
            }
        }
    }
}
