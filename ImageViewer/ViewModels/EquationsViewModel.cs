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
using ImageViewer.Views.List;
using ImageViewer.Commands.Helper;

namespace ImageViewer.ViewModels
{
    public class EquationsViewModel : INotifyPropertyChanged
    {
        private readonly ModelsEx models;
        private readonly SolidColorBrush changesBrush;
        private readonly SolidColorBrush noChangesBrush;
        public EquationsViewModel(ModelsEx models)
        {
            this.models = models;
            this.ApplyCommand = new ActionCommand(ApplyFormulas);

            changesBrush = new SolidColorBrush(Color.FromRgb(237, 28, 36));
            noChangesBrush = (SolidColorBrush) models.Window.Window.FindResource("FontBrush");

            var res = new List<EquationViewModel>();
            for (var i = 0; i < models.NumPipelines; ++i)
            {
                var vm = new EquationViewModel(models, i);
                vm.PropertyChanged += OnPropertyChanged;
                res.Add(vm);
            }

            Equations = res;
            OnPropertyChanged(nameof(Equations));
        }

        private void UpdateHasChanges()
        {
            HasChanges = Equations.Any(eq => eq.HasChanges && eq.IsVisible);
        }

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            if (args.PropertyName != nameof(EquationViewModel.HasChanges)) return;
            UpdateHasChanges();
        }

        public ICommand ApplyCommand { get; }

        public void ApplyFormulas()
        {
            foreach (var eq in Equations)
            {
                try
                {
                    if (eq.IsVisible)
                        eq.ApplyFormulas();
                }
                catch (Exception e)
                {
                    models.Window.ShowErrorDialog(e);
                }
            }
            // also force shedule recompute
            models.SheduleRecompute();
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

        public Brush TabItemColor => HasChanges ? changesBrush : noChangesBrush;

        public List<EquationViewModel> Equations { get; private set; }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
