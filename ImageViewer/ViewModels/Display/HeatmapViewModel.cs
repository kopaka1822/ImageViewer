using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using ImageFramework.Annotations;
using ImageFramework.Model.Filter;
using ImageFramework.Model.Filter.Parameter;
using ImageFramework.Model.Overlay;
using ImageFramework.Utility;
using ImageViewer.Commands.Helper;
using ImageViewer.Controller.Overlays;
using ImageViewer.Models;
using ImageViewer.Models.Display;
using ImageViewer.Models.Display.Overlays;

namespace ImageViewer.ViewModels.Display
{
    public class HeatmapViewModel : INotifyPropertyChanged
    {
        private readonly ModelsEx models;
        private readonly ViewModels viewModels;
        private FilterModel curFilter = null; // current heatmap filter
        private Action unsubFilter = null; // unsibscirbe from filter events

        public HeatmapViewModel(ModelsEx models, ViewModels viewModels)
        {
            this.models = models;
            this.viewModels = viewModels;

            SetPositionCommand = new ActionCommand(SetPosition);
            LoadFilterCommand = new ActionCommand(LoadFilter);

            // listen to filters to synchronize filter with heatmap
            models.Filter.PropertyChanged += FiltersPropertyChanged;
        }

        private void FiltersPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(FiltersModel.Filter):
                    UnregisterFilter();
                    var newHeatmapFilter = models.Filter.Filter.FirstOrDefault(filter => filter.Name == "Heatmap");
                    RegisterFilter(newHeatmapFilter);
                    break;
            }
        }

        // unregister current filter
        private void UnregisterFilter()
        { ;
            if (unsubFilter == null)
            {
                Debug.Assert(unsubFilter == null);
                return;
            }

            unsubFilter();
            curFilter = null;
            unsubFilter = null;
        }

        private void RegisterFilter(FilterModel filter)
        {
            Debug.Assert(filter != null);
            Debug.Assert(filter.Name == "Heatmap");

            // look for the relevant parameters
            var typeParam = filter.Parameters.FirstOrDefault(p => p.GetBase().Name == "Type") as IntFilterParameterModel;
            if(typeParam != null)
            {
                typeParam.PropertyChanged += TypeParameterOnChange;
                models.Heatmap.Style = (HeatmapModel.ColorStyle)typeParam.Value;
            }

            // TODO subscribe to base model for "enabled" property
            curFilter = filter;
            unsubFilter = () =>
            {
                if (typeParam != null)
                    typeParam.PropertyChanged -= TypeParameterOnChange;
                
            };
        }

        // callback for the type paramter (heatmap style)
        private void TypeParameterOnChange(object sender, PropertyChangedEventArgs e)
        {
            var model = (IntFilterParameterModel)sender;
            models.Heatmap.Style = (HeatmapModel.ColorStyle)model.Value;
            
        }

        public void SetPosition()
        {
            Debug.Assert(IsEnabled);

            var overlay = new HeatmapBoxOverlay(models);
            models.Display.ActiveOverlay = overlay;
        }

        public void LoadFilter()
        {
            // first check if filter is already loaded (in the view model)
            var filter = viewModels.Filter.AvailableFilter.FirstOrDefault(filterView => filterView.Filter.Name == "Heatmap")?.Filter;

            if(filter == null)
            {
                // filter not found, load from disc
                try
                {
                    var filterPath = models.Window.ExecutionPath + "\\Filter\\heatmap.hlsl";
                    filter = models.CreateFilter(filterPath);
                    viewModels.Filter.AddFilter(filter);
                }
                catch(Exception e)
                {
                    models.Window.ShowErrorDialog(e);
                    return;
                }
            }

            // switch to filters tab and select this filter
            viewModels.SetViewerTab(ViewModels.ViewerTab.Filters);
            viewModels.Filter.SelectedFilter = viewModels.Filter.AvailableFilter.First(filterView => ReferenceEquals(filterView.Filter, filter));
        }

        public ICommand SetPositionCommand { get; }

        public ICommand ConfigureCommand { get; }

        public ICommand LoadFilterCommand { get; }

        public bool IsEnabled
        {
            get => models.Heatmap.IsEnabled;
            set => models.Heatmap.IsEnabled = value;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
