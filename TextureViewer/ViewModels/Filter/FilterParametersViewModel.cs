using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using TextureViewer.Models.Filter;
using TextureViewer.Views;

namespace TextureViewer.ViewModels.Filter
{
    public class FilterParametersViewModel
    {
        public ObservableCollection<object> View { get; } = new ObservableCollection<object>();
        public List<IFilterParameterViewModel> ViewModels { get; } = new List<IFilterParameterViewModel>();

        public event EventHandler Changed;

        private bool hasChanged = false;
        public bool HasChanged
        {
            get => hasChanged;
            set
            {
                if (hasChanged == value) return;
                hasChanged = value;
                OnChanged();
            }
        }

        public FilterParametersViewModel(FilterModel item)
        {
            // create view and view models
            var margin = new Thickness(0.0, 0.0, 0.0, 2.0);

            View.Add(new TextBlock
            {
                Text = item.Name,
                Margin = margin,
                TextWrapping = TextWrapping.Wrap,
                FontSize = 18.0
            });

            if (item.Description.Length > 0)
                View.Add(new TextBlock
                {
                    Text = item.Description,
                    Margin = new Thickness(0.0, 0.0, 0.0, 10.0),
                    TextWrapping = TextWrapping.Wrap
                });

            // add all settings
            foreach (var para in item.Parameters)
            {
                View.Add(new TextBlock
                {
                    Text = para.GetBase().Name + ":",
                    Margin = margin,
                    TextWrapping = TextWrapping.Wrap
                });

                IFilterParameterViewModel vm = null;
                switch (para.GetParamterType())
                {
                    case ParameterType.Float:
                    {
                        var viewModel = new FloatFilterParameterViewModel(para.GetFloatModel());
                        View.Add(new FloatFilterParameterView(viewModel));
                        vm = viewModel;
                    }
                        break;
                    case ParameterType.Int:
                    {
                        var viewModel = new IntFilterParameterViewModel(para.GetIntModel());
                        View.Add(new IntFilterParameterView(viewModel));
                        vm = viewModel;
                        }
                        break;
                    case ParameterType.Bool:
                    {
                        var viewModel = new BoolFilterParameterViewModel(para.GetBoolModel());
                        View.Add(new BoolFilterParameterView(viewModel));
                        vm = viewModel;
                        }      
                        break;
                }

                Debug.Assert(vm != null);

                ViewModels.Add(vm);     
                // register on changed callback                
                vm.Changed += (sender, args) => HasChanged = HasChanges();
            }

            if (item.Parameters.Count <= 0) return;
            
            // restore default button
            var btn = new Button
            {
                Content = "Restore Defaults",
                HorizontalAlignment = HorizontalAlignment.Right,
                Margin = new Thickness(0.0, 8.0, 0.0, 0.0),
                Padding = new Thickness(2)
            };
            btn.Click += (sender, args) => RestoreDefaults();
            View.Add(btn);       
        }

        public void Apply()
        {
            foreach (var filter in ViewModels)
            {
                filter.Apply();
            }

            hasChanged = false;
        }

        public void Cancel()
        {
            foreach (var filter in ViewModels)
            {
                filter.Cancel();
            }

            hasChanged = false;
        }

        public void RestoreDefaults()
        {
            foreach (var filter in ViewModels)
            {
                filter.RestoreDefaults();
            }

            HasChanged = HasChanges();
        }

        private bool HasChanges()
        {
            foreach (var filter in ViewModels)
            {
                if (filter.HasChanges()) return true;
            }

            return false;
        }

        protected virtual void OnChanged()
        {
            Changed?.Invoke(this, EventArgs.Empty);
        }
    }
}
