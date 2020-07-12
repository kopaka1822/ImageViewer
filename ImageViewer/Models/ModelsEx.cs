using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageFramework.Model.Overlay;
using ImageViewer.Controller;
using ImageViewer.Controller.TextureViews.Shared;
using ImageViewer.Models.Display;
using ImageViewer.UtilityEx;

namespace ImageViewer.Models
{
    public class ModelsEx : ImageFramework.Model.Models
    {
        public WindowModel Window { get; }
        public DisplayModel Display { get; }
        public SettingsModel Settings { get; }

        public TextureViewData ViewData { get; }
        
        public BoxOverlay ZoomBox { get; }

        public IReadOnlyList<StatisticModel> Statistics { get; }

        public ExportConfigModel ExportConfig { get; }

        public PathManager ExportPath { get; } = new PathManager();
        private PathManager viewerConfigPath = null;

        public PathManager ViewerConfigPath => viewerConfigPath ??
                                               (viewerConfigPath = new PathManager(Window.ExecutionPath + "\\Configs",
                                                   null, "icfg"));

        private readonly ResizeController resizeController;
        private readonly ComputeImageController computeImageController;
        private readonly ClientDropController clientDropController;
        private readonly PaintController paintController;
        private readonly CropController cropController;
        public ModelsEx(MainWindow window)
        : base(4)
        {
            // only enabled first pipeline
            for (int i = 1; i < NumPipelines; ++i)
                Pipelines[i].IsEnabled = false;

            Settings = new SettingsModel();
            Window = new WindowModel(window);
            Display = new DisplayModel(this);
            ExportConfig = new ExportConfigModel();
            ViewData = new TextureViewData(this);

            var stats = new List<StatisticModel>();
            for(int i = 0; i < NumPipelines; ++i)
                stats.Add(new StatisticModel(this, Display, i));
            Statistics = stats;

            ZoomBox = new BoxOverlay(this);
            Overlay.Overlays.Add(ZoomBox);

            resizeController = new ResizeController(this);
            computeImageController = new ComputeImageController(this);
            paintController = new PaintController(this);
            clientDropController = new ClientDropController(this);
            cropController = new CropController(this);
        }

        public override void Dispose()
        {
            Settings?.Save();
            ViewData?.Dispose();
            paintController?.Dispose();
            Display?.Dispose();
            Window?.Dispose();
            base.Dispose();
        }
    }
}
