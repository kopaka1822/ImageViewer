using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Remoting.Channels;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using ImageFramework.Annotations;
using ImageFramework.Model;
using ImageViewer.Commands.Helper;
using ImageViewer.Models;

namespace ImageViewer.ViewModels.Statistics
{
    public class SSIMsViewModel : INotifyPropertyChanged
    {
        private readonly ModelsEx models;

        public class ImageSourceItem
        {
            public string Name { get; set; }
            public bool IsEquation { get; set; }
            public int Id { get; set; }

            public string ToolTip => null;
        }

        public SSIMsViewModel(ModelsEx models)
        {
            this.models = models;
            this.models.Images.PropertyChanged += ImagesOnPropertyChanged;
            var pipeId = 0;
            foreach (var pipe in models.Pipelines)
            {
                int copiedId = pipeId++;
                pipe.PropertyChanged += (sender, e) => PipeOnPropertyChanged(copiedId, e);
            }

            LuminanceCommand = new ActionCommand<int>((int id) => Items[id].ImportLuminance());
            ContrastCommand = new ActionCommand<int>((int id) => Items[id].ImportContrast());
            StructureCommand = new ActionCommand<int>((int id) => Items[id].ImportStructure());
            SSIMCommand = new ActionCommand<int>((int id) => Items[id].ImportSSIM());

            Items.Add(new SSIMViewModel(this, models, 0));
            Items.Add(new SSIMViewModel(this, models, 1));

            RefreshImageSources();
        }

        private void PipeOnPropertyChanged(int id, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(ImagePipeline.IsEnabled):
                    RefreshImageSources();
                    break;
                case nameof(ImagePipeline.Image):
                    foreach (var vm in Items)
                    {
                        vm.OnPipelineImageChanged(id);
                    }
                    break;
            }
        }

        private void ImagesOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(ImagesModel.ImageOrder):
                case nameof(ImagesModel.ImageAlias):
                case nameof(ImagesModel.NumImages):
                    RefreshImageSources();
                    break;
            }
        }

        public List<ImageSourceItem> ImageSources { get; private set; }
        public ObservableCollection<SSIMViewModel> Items { get; } = new ObservableCollection<SSIMViewModel>();

        public ICommand LuminanceCommand { get; }
        public ICommand ContrastCommand { get; }
        public ICommand StructureCommand { get; }
        public ICommand SSIMCommand { get; }

        private void RefreshImageSources()
        {
            var res = new List<ImageSourceItem>();

            for (var i = 0; i < models.Images.Images.Count; i++)
            {
                var imageData = models.Images.Images[i];
                res.Add(new ImageSourceItem
                {
                    Name = $"I{i} - {imageData.Alias}",
                    IsEquation = false,
                    Id = i
                });
            }

            for (var i = 0; i < models.Pipelines.Count; i++)
            {
                var pipe = models.Pipelines[i];
                if (pipe.IsEnabled)
                {
                    res.Add(new ImageSourceItem
                    {
                        Name = $"Equation {i + 1}",
                        IsEquation = true,
                        Id = i
                    });
                }
            }

            ImageSources = res;
            foreach (var vms in Items)
            {
                vms.UpdateImageSources();
            }
            OnPropertyChanged(nameof(ImageSources));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
