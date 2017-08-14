using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Microsoft.Win32;
using OpenTKImageViewer.Tonemapping;
using Xceed.Wpf.Toolkit;

namespace OpenTKImageViewer.Dialogs
{
    /// <summary>
    /// Interaction logic for TonemapWindow.xaml
    /// </summary>
    public partial class TonemapWindow : Window
    {
        public bool IsClosing { get; set; } = false;
        private MainWindow parent;
        private List<ToneParameter> toneSettings;

        public TonemapWindow(MainWindow parent)
        {
            this.parent = parent;
            InitializeComponent();
            
            // clone current tonemapping settings
            toneSettings = parent.Context.Tonemapper.CloneSettings();

            UpdateList();
        }

        private void TonemapWindow_OnClosing(object sender, CancelEventArgs e)
        {
            IsClosing = true;
            parent.TonemapDialog = null;
        }

        private void ButtonAdd_OnClick(object sender, RoutedEventArgs e)
        {
            var ofd = new OpenFileDialog();
            ofd.Multiselect = false;

            if (ofd.ShowDialog() != true) return;

            // load shader
            parent.EnableOpenGl();
            try
            {
                var param = parent.Context.Tonemapper.LoadShader(ofd.FileName);

                toneSettings.Add(param);
                ListBoxMapper.Items.Add(GenerateItem(param));
                ListBoxMapper.SelectedIndex = ListBoxMapper.Items.Count - 1;
            }
            catch (Exception exception)
            {
                App.ShowErrorDialog(this, exception.Message);
            }
        }

        private void UpdateList()
        {
            ListBoxMapper.Items.Clear();
            foreach (var toneParameter in toneSettings)
            {
                ListBoxMapper.Items.Add(GenerateItem(toneParameter));
            }
        }

        private void DisplayItem(ToneParameter p)
        {
            var list = StackPanelMapper.Children;
            list.Clear();

            var margin = new Thickness(0.0, 0.0, 0.0, 2.0);

            list.Add(new TextBlock {Text = p.Shader.Name, Margin = margin, TextWrapping = TextWrapping.Wrap, FontSize = 18.0});
            if(p.Shader.Description.Length > 0)
                list.Add(new TextBlock { Text = p.Shader.Description, Margin = new Thickness(0.0, 0.0, 0.0, 10.0), TextWrapping = TextWrapping.Wrap});

            // Display settings
            foreach (var para in p.Parameters)
            {
                list.Add(new TextBlock {Text = para.Name + ":", Margin = margin, TextWrapping = TextWrapping.Wrap});

                switch (para.Type)
                {
                    case ShaderLoader.ParameterType.Bool:
                    {
                        // check Box
                        var e = new CheckBox {IsChecked = GetBoolValue(para), Margin = margin};
                        e.Checked += (sender, args) => para.CurrentValue = BoolToDecimal(e.IsChecked);
                        para.ValueChanged += (sender, args) => e.IsChecked = para.CurrentValue != (decimal)0.0;
                        list.Add(e);
                    }
                        break;
                    case ShaderLoader.ParameterType.Int:
                    {
                        // use num up down
                        var e = new IntegerUpDown
                        {
                            Value = (int) para.CurrentValue, Margin = margin,
                            //Maximum = (int)para.Max, Minimum = (int)para.Min
                        };
                        e.ValueChanged += (sender, args) =>
                        {
                            if (e.Value != null) para.CurrentValue = (decimal)e.Value;
                        };
                        para.ValueChanged += (sender, args) => e.Value = (int)para.CurrentValue;
                        list.Add(e);
                    }
                        break;
                    default:
                    {
                        var e = new DecimalUpDown
                        {
                            Value = para.CurrentValue, Margin = margin,
                            //Maximum = para.Max, Minimum = para.Min
                        };
                        e.ValueChanged += (sender, args) =>
                        {
                            if (e.Value != null) para.CurrentValue =(decimal)e.Value;
                        };
                        para.ValueChanged += (sender, args) =>
                        {

                            e.Value = para.CurrentValue;
                            e.Text = para.CurrentValue.ToString(CultureInfo.InvariantCulture);
                        };
                        list.Add(e);
                    }
                        break;
                }
            }

            // default button
            var btn = new Button
            {
                Content = "Restore Defaults",
                HorizontalAlignment = HorizontalAlignment.Right,
                Margin = new Thickness(0.0, 8.0, 0.0, 0.0),
                Padding = new Thickness(2)
            };
            btn.Click += (sender, args) => p.RestoreDefaults();
            list.Add(btn);
        }

        private static bool GetBoolValue(ShaderLoader.Parameter p)
        {
            Debug.Assert(p.Type == ShaderLoader.ParameterType.Bool);
            return p.CurrentValue != (decimal) 0.0;
        }

        private static decimal IntToDecimal(decimal min, decimal max, int val)
        {
            return Math.Min(max, Math.Max(min, (decimal) val));
        }

        private static decimal Clamp(decimal min, decimal max, decimal val)
        {
            return Math.Min(max, Math.Max(min, val));
        }

        private decimal BoolToDecimal(bool? b)
        {
            if (b == null)
                return 0;
            return (bool)b ? 1 : 0;
        }

        private static ListBoxItem GenerateItem(ToneParameter p)
        {
            return new ListBoxItem {Content = p.Shader.Name, ToolTip = p.Shader.Description};
        }

        private void ButtonApply_OnClick(object sender, RoutedEventArgs e)
        {
            // aplly current set of settings
            parent.EnableOpenGl();
            parent.Context.Tonemapper.Apply(toneSettings);
        }

        private void ListBoxMapper_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ListBoxMapper.SelectedIndex >= 0)
            {
                DisplayItem(toneSettings[ListBoxMapper.SelectedIndex]);
            }
        }
    }
}
