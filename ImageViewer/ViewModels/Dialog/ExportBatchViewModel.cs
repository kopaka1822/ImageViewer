using ImageFramework.Annotations;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;
using ImageViewer.Commands.Export;
using ImageViewer.Commands.Helper;
using ImageViewer.Models;
using ImageViewer.UtilityEx;

namespace ImageViewer.ViewModels.Dialog
{
    public class ExportBatchViewModel : INotifyPropertyChanged, IDisposable
    {
        private readonly ModelsEx models;
        private readonly PathManager path;
        private readonly bool is3D;

        private static HashSet<int> _excludeList = new HashSet<int>(); // list with items to exclude
        private static int _lastVariableId = 0;

        public class ImageSelectorViewModel
        {
            public string Name { get; set; }
            public bool IsSelected { get; set; }
        }

        public ExportBatchViewModel(ModelsEx models, bool is3D, PathManager path, List<int> possibleImageVariables)
        {
            this.models = models;
            this.is3D = is3D;
            this.path = path;

            // init available variables
            Debug.Assert(possibleImageVariables.Count > 0);
            AvailableVariables = new List<ListItemViewModel<int>>();
            foreach (var i in possibleImageVariables)
            {
                AvailableVariables.Add(new ListItemViewModel<int>()
                {
                    Cargo = i,
                    Name = $"I{i} - {models.Images.Images[i].Alias}"
                });
                if(i == _lastVariableId)
                    selectedVariable = AvailableVariables.Last();
            }
            if (selectedVariable == null) selectedVariable = AvailableVariables.First();

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
            if (selectedFormat == null) selectedFormat = availableFormats.First();

            // init image list
            var imagesList = new List<ImageSelectorViewModel>();
            for (int i = 0; i < models.Images.NumImages; ++i)
            {
                imagesList.Add(new ImageSelectorViewModel
                {
                    IsSelected = !_excludeList.Contains(i),
                    Name = $"I{i} - {models.Images.Images[i].Alias}"
                });
            }
            ImagesList = imagesList;

            // init commands
            SelectAllCommand = new ActionCommand(SelectAll);
            SelectNoneCommand = new ActionCommand(SelectNone);
        }

        public void Dispose()
        {
            // update the static list with images to exclude
            var excludeList = new HashSet<int>();
            for (int i = 0; i < ImagesList.Count; ++i)
            {
                if (!ImagesList[i].IsSelected) excludeList.Add(i);
            }

            _excludeList = excludeList;
            _lastVariableId = selectedVariable.Cargo;
        }

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

        public List<ListItemViewModel<int>> AvailableVariables { get; }
        private ListItemViewModel<int> selectedVariable;

        public ListItemViewModel<int> SelectedVariable
        {
            get => selectedVariable;
            set
            {
                if (value == null || value == selectedVariable) return;
                selectedVariable = value;
                OnPropertyChanged(nameof(SelectedVariable));
            }
        }

        public bool ShowVariableSelection => AvailableVariables.Count > 1;

        public int SelectedImageVariable => selectedVariable.Cargo;

        public List<ImageSelectorViewModel> ImagesList { get; private set; }

        /// <summary>
        /// returns a list of all images that should be batch exported
        /// </summary>
        public List<int> GetSelectedImages()
        {
            var list = new List<int>();
            for(int i = 0; i < ImagesList.Count; ++i)
                if (ImagesList[i].IsSelected) list.Add(i);
            return list;
        }

        public string Directory
        {
            get => path.Directory;
            set {} // is read only
        }

        public ICommand SelectAllCommand { get; }
        public ICommand SelectNoneCommand { get; }

        public void SelectAll()
        {
            SetSelect(true);
        }

        public void SelectNone()
        {
            SetSelect(false);
        }

        private void SetSelect(bool selected)
        {
            // binding of list needs to change for updates...
            var newList = new List<ImageSelectorViewModel>(ImagesList.Count);
            foreach (var img in ImagesList)
            {
                img.IsSelected = selected;
                newList.Add(img);
            }

            ImagesList = newList;
            OnPropertyChanged(nameof(ImagesList));
        }

        // returns true if valid, otherwise returns false after displaying an error message
        public bool IsValid()
        {
            if (ImagesList.Any(img => img.IsSelected)) return true;

            models.Window.ShowErrorDialog("At least one image needs to be selected for batch export", "Batch Export");

            return false;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
