using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media;
using ImageFramework.Annotations;
using ImageViewer.Commands;
using ImageViewer.Models;

namespace ImageViewer.ViewModels
{
    public class EquationsViewModel : INotifyPropertyChanged
    {
        private readonly EquationViewModel[] viewModels;
        private readonly ModelsEx models;

        public EquationsViewModel(ModelsEx models)
        {
            this.models = models;
            this.Apply = new ApplyImageFormulasCommand(this);
            viewModels = new EquationViewModel[models.NumPipelines];
            for (var i = 0; i < viewModels.Length; ++i)
            {
                viewModels[i] = new EquationViewModel(models, i);
                viewModels[i].PropertyChanged += OnPropertyChanged;
            }
        }

        private void UpdateHasChanges()
        {
            HasChanges = viewModels.Any(eq => eq.HasChanges && eq.IsVisible);
        }

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            if (args.PropertyName != nameof(EquationViewModel.HasChanges)) return;
            UpdateHasChanges();
        }

        public EquationViewModel Equation1 => viewModels[0];
        public EquationViewModel Equation2 => viewModels[1];
        public EquationViewModel Equation3 => viewModels[2];
        public EquationViewModel Equation4 => viewModels[3];

        public ICommand Apply { get; }

        public void ApplyFormulas()
        {
            foreach (var eq in viewModels)
            {
                try
                {
                    if (eq.IsVisible)
                        eq.ApplyFormulas();
                }
                catch (Exception e)
                {
                    models.Window.ShowErrorDialog(e.Message);
                }
            }
            UpdateHasChanges();
        }

        private bool hasChanges = false;
        public bool HasChanges
        {
            get => hasChanges;
            private set
            {
                if (value == hasChanges) return;
                hasChanges = value;
                OnPropertyChanged(nameof(HasChanges));
                OnPropertyChanged(nameof(TabItemColor));
            }
        }

        public Brush TabItemColor => HasChanges ? new SolidColorBrush(Color.FromRgb(237, 28, 36)) : new SolidColorBrush(Color.FromRgb(0, 0, 0));

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
