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
            toneSettings = parent.Context.Tonemapper.GetSettings();

            UpdateList();
        }

        private void TonemapWindow_OnClosing(object sender, CancelEventArgs e)
        {
            IsClosing = true;
            parent.TonemapDialog = null;
            parent.Context.Tonemapper.RemoveUnusedShader();
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
            parent.DisableOpenGl();
        }

        private void UpdateList()
        {
            ListBoxMapper.Items.Clear();
            foreach (var toneParameter in toneSettings)
            {
                ListBoxMapper.Items.Add(GenerateItem(toneParameter));
            }
            if (ListBoxMapper.Items.Count > 0)
                ListBoxMapper.SelectedIndex = 0;
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

        private decimal BoolToDecimal(bool? b)
        {
            if (b == null)
                return 0;
            return (bool)b ? 1 : 0;
        }

        private ListBoxItem GenerateItem(ToneParameter p)
        {
            var imgUp = new Image
            {
                Source = new BitmapImage(new Uri(Environment.CurrentDirectory + "/Icons/arrow_up.png"))
            };
            var imgDown = new Image
            {
                Source = new BitmapImage(new Uri(Environment.CurrentDirectory + "/Icons/arrow_down.png"))
            };
            var imgDelete = new Image
            {
                Source = new BitmapImage(new Uri(Environment.CurrentDirectory + "/Icons/cancel.png"))
            };

            var btnUp = new Button
            {
                Height = 8,
                Width = 16,
                Content = imgUp
            };
            var btnDown = new Button
            {
                Height = 8,
                Width = 16,
                Content = imgDown
            };
            var btnDelete = new Button
            {
                Height = 16,
                Width = 16,
                Content = imgDelete
            };

            var upDownPanel = new StackPanel
            {
                HorizontalAlignment = HorizontalAlignment.Right
            };
            upDownPanel.Children.Add(btnUp);
            upDownPanel.Children.Add(btnDown);

            var text = new TextBlock {Text = p.Shader.Name };
            
            var grid = new Grid {Width = 210.0};
            grid.ColumnDefinitions.Add(new ColumnDefinition{Width = new GridLength(1.0, GridUnitType.Star)});
            grid.ColumnDefinitions.Add(new ColumnDefinition{Width = new GridLength(1.0, GridUnitType.Auto)});
            grid.ColumnDefinitions.Add(new ColumnDefinition{Width = new GridLength(1.0, GridUnitType.Auto)});

            Grid.SetColumn(text, 0);
            grid.Children.Add(text);
            Grid.SetColumn(btnDelete, 1);
            grid.Children.Add(btnDelete);
            Grid.SetColumn(upDownPanel, 2);
            grid.Children.Add(upDownPanel);

            // add callbacks
            var item = new ListBoxItem { Content = grid, ToolTip = p.Shader.Description };

            btnUp.Click += (sender, args) => ItemMoveUp(item);
            btnDown.Click += (sender, args) => ItemMoveDown(item);
            btnDelete.Click += (sender, args) => ItemDelete(item);

            return item;
        }

        private int GetItemIndex(ListBoxItem item)
        {
            for (int i = 0; i < ListBoxMapper.Items.Count; ++i)
            {
                if (ReferenceEquals(ListBoxMapper.Items[i], item))
                    return i;
            }
            return -1;
        }

        private void SwapItems(int idx1, int idx2)
        {
            Debug.Assert(idx1 < idx2);

            // remember values
            var box1  = ListBoxMapper.Items[idx1];
            var box2 = ListBoxMapper.Items[idx2];

            // detach
            ListBoxMapper.Items.RemoveAt(idx2);
            ListBoxMapper.Items.RemoveAt(idx1);

            // attach
            ListBoxMapper.Items.Insert(idx1, box2);
            ListBoxMapper.Items.Insert(idx2, box1);

            var tmp2 = toneSettings[idx1];
            toneSettings[idx1] = toneSettings[idx2];
            toneSettings[idx2] = tmp2;
        }

        private void ItemMoveUp(ListBoxItem item)
        {
            var idx = GetItemIndex(item);
            if (idx < 1) return;
            
            SwapItems(idx - 1, idx);
            ListBoxMapper.SelectedIndex = idx - 1;
        }

        private void ItemMoveDown(ListBoxItem item)
        {
            var idx = GetItemIndex(item);
            if (idx < 0 || idx == ListBoxMapper.Items.Count - 1) return;
            
            SwapItems(idx, idx + 1);
            ListBoxMapper.SelectedIndex = idx + 1;
        }

        private void ItemDelete(ListBoxItem item)
        {
            var idx = GetItemIndex(item);
            if (idx < 0) return;
            ListBoxMapper.Items.RemoveAt(idx);
            toneSettings.RemoveAt(idx);
        }

        private void ButtonApply_OnClick(object sender, RoutedEventArgs e)
        {
            // aplly current set of settings
            parent.EnableOpenGl();
            try
            {
                parent.Context.Tonemapper.Apply(toneSettings);
            }
            catch (Exception exception)
            {
                App.ShowErrorDialog(parent, exception.Message);
            }
            parent.DisableOpenGl();
        }

        private void ListBoxMapper_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ListBoxMapper.SelectedIndex >= 0)
            {
                DisplayItem(toneSettings[ListBoxMapper.SelectedIndex]);
            } else StackPanelMapper.Children.Clear();
        }
    }
}
