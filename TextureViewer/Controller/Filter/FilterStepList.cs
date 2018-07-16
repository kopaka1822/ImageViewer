using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL4;
using TextureViewer.Controller.ImageCombination;
using TextureViewer.Utility;

namespace TextureViewer.Controller.Filter
{
    /// <summary>
    /// extension of the step list which flips primary and secondary image after everything was executed
    /// </summary>
    public class FilterStepList : StepList
    {
        private readonly ImageCombineBuilder builder;

        public FilterStepList(ImageCombineBuilder builder, List<IStepable> steps) : base(steps)
        {
            this.builder = builder;
        }

        public override void NextStep()
        {
            base.NextStep();

            // if everything finished flip the images
            if (!HasStep())
            {
                builder.SwapPrimaryAndTemporary();
                GL.MemoryBarrier(MemoryBarrierFlags.AllBarrierBits);
            }
        }
    }
}
