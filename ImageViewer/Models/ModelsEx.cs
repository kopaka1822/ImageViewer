using System;
using System.CodeDom;
using System.Collections.Generic;
using System.ComponentModel;
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

        public ArrowOverlay Arrows { get; }

        public HeatmapModel Heatmap { get; }

        public IReadOnlyList<StatisticModel> Statistics { get; }

        public ExportConfigModel ExportConfig { get; }

        private PathManager viewerConfigPath = null;

        public PathManager ViewerConfigPath => viewerConfigPath ??
                                               (viewerConfigPath = new PathManager(Window.ExecutionPath + "\\Configs",
                                                   null, "icfg"));

        private readonly ResizeController resizeController;
        private readonly ComputeImageController computeImageController;
        private readonly ClientDropController clientDropController;
        private readonly PaintController paintController;
        private readonly CropController cropController;

        // transform shader with 1 srv input and 1 uav output => overwrites alpha channel with 1.0
        public ImageFramework.Model.Shader.TransformShader OverwriteAlphaShader => overwriteAlphaShader ?? (overwriteAlphaShader = new ImageFramework.Model.Shader.TransformShader("return float4(value.r, value.g, value.b, 1.0)", "float4", "float4"));
        private ImageFramework.Model.Shader.TransformShader overwriteAlphaShader = null;

        // TODO rename
        public ImportModel Import { get; }

        public ModelsEx(MainWindow window)
            : this(window, new SettingsModel())
        {}

        private ModelsEx(MainWindow window, SettingsModel settings)
            : base(4, settings.CacheFormat)
        {
            // only enabled first pipeline
            for (int i = 1; i < NumPipelines; ++i)
                Pipelines[i].IsEnabled = false;

            Settings = settings;
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
            Arrows = new ArrowOverlay(this);
            Overlay.Overlays.Add(Arrows);
            Heatmap = new HeatmapModel(this);

            resizeController = new ResizeController(this);
            computeImageController = new ComputeImageController(this);
            paintController = new PaintController(this);
            clientDropController = new ClientDropController(this);
            cropController = new CropController(this);

            Import = new ImportModel(this);

            settings.PropertyChanged += SettingsOnPropertyChanged;
        }

        private void SettingsOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(SettingsModel.CacheFormat):
                    SetTextureCacheFormat(Settings.CacheFormat);
                    // force changes to pipelines
                    for (int i = 0; i < NumPipelines; ++i)
                        Pipelines[i].ForceChanges();
                    break;
            }
        }

        public int GetExportPipelineId()
        {
            if (NumEnabled == 0)
            {
                throw new Exception("An image equation must be enabled when exporting");
            }
            var id = GetFirstEnabledPipeline();
            
            if (NumEnabled > 1)
            {
                if(!Window.ShowYesNoDialog($"More than one image equations are enabled. Do you want to use equation {id + 1} for export?", "Export"))
                    throw new Exception("Export canceled. Make sure only one image equation is enabled");
            }

            return id;
        }

        /// <summary>
        /// force shedules a recompute of image equations.
        /// Normally this is done automatically as soon as changes are detected. However,
        /// if the operation was aborted by the uses or some runtime error it needs to be resheduled manually
        /// </summary>
        public void SheduleRecompute()
        {
            computeImageController.ScheduleRecompute();
        }

        public override void Dispose()
        {
            overwriteAlphaShader?.Dispose();
            Settings?.Save();
            ViewData?.Dispose();
            paintController?.Dispose();
            Display?.Dispose();
            Window?.Dispose();
            base.Dispose();
        }
    }
}
