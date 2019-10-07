using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using ImageFramework.Annotations;
using ImageFramework.Model;
using ImageFramework.Model.Filter.Parameter;
using ImageViewer.Views;

namespace ImageViewer.ViewModels.Filter
{
    public class TextureFilterParameterViewModel : INotifyPropertyChanged, IFilterParameterViewModel
    {
        private readonly TextureFilterParameterModel model;
        private readonly ImagesModel images;

        public ObservableCollection<ComboBoxItem<int>> AvailableTextures { get; } = new ObservableCollection<ComboBoxItem<int>>();
        private ComboBoxItem<int> selectedTexture;

        public ComboBoxItem<int> SelectedTexture
        {
            get => selectedTexture;
            set
            {
                if (ReferenceEquals(value, selectedTexture) || value == null) return;

                var prevChanged = HasChanges();

                selectedTexture = value;
                OnPropertyChanged(nameof(SelectedTexture));

                if (prevChanged != HasChanges())
                    OnChanged();
            }
        }

        public TextureFilterParameterViewModel(TextureFilterParameterModel model, ImagesModel images)
        {
            this.model = model;
            this.images = images;
            model.PropertyChanged += ParameterOnPropertyChanged;
            images.PropertyChanged += ImagesOnPropertyChanged;
            RefreshImageList(false);
        }

        public void Dispose()
        {
            // unregister listener
            images.PropertyChanged -= ImagesOnPropertyChanged;
            model.PropertyChanged -= ParameterOnPropertyChanged;
        }

        private void ImagesOnPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            switch (args.PropertyName)
            {
                case nameof(ImagesModel.NumImages):
                case nameof(ImagesModel.ImageOrder):
                    RefreshImageList(HasChanges());
                    break;
            }
        }

        private void ParameterOnPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            if (args.PropertyName == nameof(TextureFilterParameterModel.Source))
            {
                SelectedTexture = AvailableTextures[model.Source];
            }
        }

        public void Apply()
        {
            model.Source = SelectedTexture.Cargo;
        }

        public void Cancel()
        {
            SelectedTexture = AvailableTextures[model.Source];
        }

        public bool HasChanges()
        {
            return model.Source != SelectedTexture.Cargo;
        }

        private void RefreshImageList(bool prevChanged)
        {
            AvailableTextures.Clear();
            for (int i = 0; i < images.NumImages; ++i)
            {
                AvailableTextures.Add(new ComboBoxItem<int>($"I{i} - {System.IO.Path.GetFileNameWithoutExtension(images.Images[i].Filename)}", i));
            }
            if (AvailableTextures.Count == 0)
                // add dummy texture
                AvailableTextures.Add(new ComboBoxItem<int>("I0", 0));

            // set the correct selected texture
            // is the source from model still in a valid range?
            if (model.Source >= AvailableTextures.Count)
                model.Source = AvailableTextures.Count - 1;
            Debug.Assert(model.Source >= 0);

            selectedTexture = AvailableTextures[model.Source];

            OnPropertyChanged(nameof(AvailableTextures));
            OnPropertyChanged(nameof(SelectedTexture));

            if (prevChanged != HasChanges())
                OnChanged();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged(string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public event EventHandler Changed;

        protected virtual void OnChanged()
        {
            Changed?.Invoke(this, EventArgs.Empty);
        }

        // unused interface methods

        public void RestoreDefaults()
        {

        }

        public void InvokeAction(ActionType action)
        {

        }

        public bool HasKeyToInvoke(Key key)
        {
            return false;
        }

        public void InvokeKey(Key key)
        {

        }
    }
}
