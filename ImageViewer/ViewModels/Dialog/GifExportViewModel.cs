using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageViewer.ViewModels.Dialog
{
    public class GifExportViewModel
    {

        public int FramesPerSecond => SelectedFps == 0 ? 30 : 60;

        public int SelectedFps { get; set; } = 1;

        public int TotalSeconds { get; set; } = 3;

        public int SliderSize { get; set; } = 3;

        public string Title1 { get; set; }

        public string Title2 { get; set; }

        public void InitTitles(ImageFramework.Model.Models models)
        {
            var eqs = models.GetEnabledPipelines();
            Debug.Assert(eqs.Count == 2);

            var eq1 = models.Pipelines[eqs[0]].GetFirstImageId();
            var eq2 = models.Pipelines[eqs[1]].GetFirstImageId();

            var t1 = models.Images.GetImageAlias(eq1);
            var t2 = models.Images.GetImageAlias(eq2);

            // keep custom titles if the last name is identical
            if (t1 != lastTitle1) Title1 = t1;
            if (t2 != lastTitle2) Title2 = t2;
            lastTitle1 = t1;
            lastTitle2 = t2;
        }

        // init titles helper
        private string lastTitle1 = "";
        private string lastTitle2 = "";
    }
}
