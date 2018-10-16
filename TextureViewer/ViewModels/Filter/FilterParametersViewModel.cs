using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using TextureViewer.Annotations;
using TextureViewer.Models;
using TextureViewer.Models.Filter;
using TextureViewer.Views;

namespace TextureViewer.ViewModels.Filter
{
    public class FilterParametersViewModel : INotifyPropertyChanged
    {
        private readonly FilterModel model;

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

        private bool isVisible;
        public bool IsVisible
        {
            get => isVisible;
            set
            {
                if (isVisible == value) return;
                isVisible = value;
                HasChanged = HasChanges();
                OnPropertyChanged(nameof(IsVisible));
            }
        }

        private readonly List<bool> isEquationVisible = new List<bool>();
        public IReadOnlyList<bool> IsEquationVisible => isEquationVisible;

        private readonly List<CheckBox> equationCheckBoxes = new List<CheckBox>();

        public FilterParametersViewModel(FilterModel item, ImagesModel images)
        {
            model = item;
            isVisible = item.IsVisible;
            for(var i = 0; i < App.MaxImageViews; ++i)
                isEquationVisible.Add(item.IsEquationVisible[i]);

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

            // default value change handler
            var enabledBinding = new Binding
            {
                Source = this,
                Path = new PropertyPath("IsVisible"),
                Mode = BindingMode.OneWay,
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
            };

            // add all settings
            foreach (var tex in item.TextureParameters)
            {
                View.Add(new TextBlock
                {
                    Text = tex.Name + ":",
                    Margin = margin,
                    TextWrapping = TextWrapping.Wrap
                });

                var vm = new FilterTextureParameterViewModel(tex, images);
                ViewModels.Add(vm);
                View.Add(new FilterTextureParameterView(vm, enabledBinding));

                // register on changed callback                
                vm.Changed += (sender, args) => HasChanged = HasChanges();
            }

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
                        View.Add(new FloatFilterParameterView(viewModel, enabledBinding));
                        vm = viewModel;
                    }
                        break;
                    case ParameterType.Int:
                    {
                        var viewModel = new IntFilterParameterViewModel(para.GetIntModel());
                        View.Add(new IntFilterParameterView(viewModel, enabledBinding));
                        vm = viewModel;
                        }
                        break;
                    case ParameterType.Bool:
                    {
                        var viewModel = new BoolFilterParameterViewModel(para.GetBoolModel());
                        View.Add(new BoolFilterParameterView(viewModel, enabledBinding));
                        vm = viewModel;
                        }      
                        break;
                }

                Debug.Assert(vm != null);

                ViewModels.Add(vm);     
                // register on changed callback                
                vm.Changed += (sender, args) => HasChanged = HasChanges();
            }

            // add checkboxes for filter equation visibility
            View.Add(new TextBlock
            {
                Text = "Equation Visibility:",
                TextWrapping = TextWrapping.Wrap,
                ToolTip = "This can disable the filter for a single image equation"
            });

            var fevPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(0.0, 2.0, 0.0, 2.0),
            };

            for (var i = 0; i < App.MaxImageViews; ++i)
            {
                fevPanel.Children.Add(new TextBlock
                {
                    Text = "E" + (i + 1) + ":",
                });
                var cb = new CheckBox
                {
                    VerticalAlignment = VerticalAlignment.Center,
                    IsChecked = IsEquationVisible[i],
                    Margin = new Thickness(5.0, 0.0, 5.0, 0.0),
                };
                BindingOperations.SetBinding(cb, UIElement.IsEnabledProperty, enabledBinding);

                // callbacks for checkbox actions
                var cbIndex = i;
                cb.Checked += (sender, args) => { SetEquationVisible(cbIndex, true); };
                cb.Unchecked += (sender, args) => { SetEquationVisible(cbIndex, false); };

                fevPanel.Children.Add(cb);
                equationCheckBoxes.Add(cb);
            }
            View.Add(fevPanel);

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

        public void SetEquationVisible(int index, bool value)
        {
            Debug.Assert(index >= 0);
            Debug.Assert(index < App.MaxImageViews);
            if (IsEquationVisible[index] == value) return;
            isEquationVisible[index] = value;
            HasChanged = HasChanges();
        }

        public void Apply()
        {
            // apply visibility
            model.IsVisible = IsVisible;
            for (var i = 0; i < App.MaxImageViews; ++i)
                model.IsEquationVisible[i] = IsEquationVisible[i];

            // apply parameters
            foreach (var filter in ViewModels)
            {
                filter.Apply();
            }

            hasChanged = false;
        }

        public void Cancel()
        {
            // restore visibility
            isVisible = model.IsVisible;
            for (var i = 0; i < App.MaxImageViews; ++i)
                isEquationVisible[i] = model.IsEquationVisible[i];

            UpdateEquationVisible();

            // restore parameters
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

        public void Dispose()
        {
            foreach(var i in ViewModels)
            {
                i.Dispose();
            }
        }

        /// <summary>
        /// returns true if any parameters including the visibility and equation visibility has changed
        /// </summary>
        /// <returns></returns>
        private bool HasChanges()
        {
            // test for visibility change
            if (model.IsVisible != IsVisible) return true;

            if (model.IsEquationVisible.Where((t, i) => t != IsEquationVisible[i]).Any())
                return true;
            
            return HasParameterChanges();
        }

        /// <summary>
        /// returns true if any parameters beside the visibility and equation visibility has changed
        /// </summary>
        /// <returns></returns>
        public bool HasParameterChanges()
        {
            // test for parameter change
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

        public bool HasKeyToInvoke(Key key)
        {
            return ViewModels.Any(p => p.HasKeyToInvoke(key));
        }

        public void InvokeKey(Key key)
        {
            foreach (var p in ViewModels)
            {
                p.InvokeKey(key);
            }
        }

        // updates equation visible boxes from view model values
        private void UpdateEquationVisible()
        {
            for (var i = 0; i < App.MaxImageViews; ++i)
                equationCheckBoxes[i].IsChecked = IsEquationVisible[i];
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
