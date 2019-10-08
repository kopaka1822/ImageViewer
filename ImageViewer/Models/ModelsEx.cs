using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageViewer.Controller;

namespace ImageViewer.Models
{
    public class ModelsEx : ImageFramework.Model.Models
    {
        public WindowModel Window { get; }
        public DisplayModel Display { get; }

        public SettingsModel Settings { get; }

        public IReadOnlyList<StatisticModel> Statistics { get; }

        private readonly ResizeController resizeController;

        public ModelsEx(MainWindow window)
        : base(4)
        {
            // only enabled first pipeline
            for (int i = 1; i < NumPipelines; ++i)
                Pipelines[i].IsEnabled = false;

            Settings = new SettingsModel();
            Window = new WindowModel(window);
            Display = new DisplayModel(this);

            var stats = new List<StatisticModel>();
            for(int i = 0; i < NumPipelines; ++i)
                stats.Add(new StatisticModel(this, i));
            Statistics = stats;

            resizeController = new ResizeController(this);
        }

        public override void Dispose()
        {
            Settings.Save();
            base.Dispose();
        }
    }
}
