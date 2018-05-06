using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TextureViewer.Controller
{
    /// <summary>
    /// handles the image combination and filter progress
    /// </summary>
    public class ProgressController
    {
        private readonly Models.Models models;

        public ProgressController(Models.Models models)
        {
            this.models = models;
        }

        /// <summary>
        /// true if DoWork should be called to perform some work
        /// </summary>
        /// <returns></returns>
        public bool HasWork()
        {
            return false;
        }

        /// <summary>
        /// perform enqueued task (i.e. calculating image)
        /// </summary>
        public void DoWork()
        {

        }
    }
}
