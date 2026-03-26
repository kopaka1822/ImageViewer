using ImageFramework.Annotations;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using ImageViewer.Commands.Export;
using ImageViewer.Commands.Helper;
using ImageViewer.Models;
using ImageViewer.UtilityEx;
using System.Linq;

namespace ImageViewer.ViewModels.Dialog
{
    public class ExportFramesViewModel : INotifyPropertyChanged, IDisposable
    {
        private readonly ModelsEx models;
        private readonly PathManager path;
        private readonly bool is3D;
        private readonly int numLayers;

        public ExportFramesViewModel(ModelsEx models, bool is3D, PathManager path, int numLayers)
        {
            this.models = models;
            this.is3D = is3D;
            this.path = path;
            this.numLayers = numLayers;

            // init formats
            List<ListItemViewModel<string>> availableFormats = new List<ListItemViewModel<string>>();
            foreach (var format in ExportCommand.Filter)
            {
                if (is3D && !ExportCommand.Is3DFilter(format.Key)) continue;

                var item = new ListItemViewModel<string>
                {
                    Cargo = format.Key,
                    Name = format.Value
                };
                availableFormats.Add(item);
                if (String.Equals(format.Key, path.Extension))
                    selectedFormat = item;
            }
            AvailableFormats = availableFormats;
            if (selectedFormat == null) selectedFormat = availableFormats.FirstOrDefault();
        }

        public void Dispose()
        {
        }

        public string Directory
        {
            get => path.Directory;
            set { }
        }

        public string LayerInfo => $"{numLayers} layers will be exported as individual frames";

        public List<ListItemViewModel<string>> AvailableFormats { get; }

        private ListItemViewModel<string> selectedFormat;

        public ListItemViewModel<string> SelectedFormat
        {
            get => selectedFormat;
            set
            {
                if (value == null || value == selectedFormat) return;
                selectedFormat = value;
                OnPropertyChanged(nameof(SelectedFormat));
            }
        }

        public string ExportExtension => selectedFormat.Cargo;

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}