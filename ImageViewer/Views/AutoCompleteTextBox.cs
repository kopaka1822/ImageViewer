using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace ImageViewer.Views
{
    public class AutoCompleteTextBox : TextBox
    {
        private Popup popup;
        private ListBox listBox;
        private Func<object, string, bool> filter;
        private string textCache = null;
        private bool suppressEvent = false;
        readonly FrameworkElement dummy = new FrameworkElement();


        public Func<object, string, bool> Filter
        {
            get => filter;
            set
            {
                if (filter != value)
                {
                    filter = value;
                    if (listBox != null)
                    {
                        if (filter != null)
                            listBox.Items.Filter = FilterFunc;
                        else
                            listBox.Items.Filter = null;
                    }
                }
            }
        }

        #region ItemsSource Dependency Property

        public IEnumerable ItemsSource
        {
            get => (IEnumerable)GetValue(ItemsSourceProperty);
            set => SetValue(ItemsSourceProperty, value);
        }

        // Using a DependencyProperty as the backing store for ItemsSource.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ItemsSourceProperty =
            ItemsControl.ItemsSourceProperty.AddOwner(
                typeof(AutoCompleteTextBox),
                new UIPropertyMetadata(null, OnItemsSourceChanged));

        private static void OnItemsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            AutoCompleteTextBox actb = d as AutoCompleteTextBox;
            if (actb == null) return;
            actb.OnItemsSourceChanged(e.NewValue as IEnumerable);
        }

        protected void OnItemsSourceChanged(IEnumerable itemsSource)
        {
            if (listBox == null) return;
            if (itemsSource is ListCollectionView)
            {
                listBox.ItemsSource = new LimitedListCollectionView((IList)((ListCollectionView)itemsSource).SourceCollection) { Limit = MaxCompletions };
            }
            else if (itemsSource is CollectionView)
            {
                listBox.ItemsSource = new LimitedListCollectionView(((CollectionView)itemsSource).SourceCollection) { Limit = MaxCompletions };
            }
            else if (itemsSource is IList)
            {
                listBox.ItemsSource = new LimitedListCollectionView((IList)itemsSource) { Limit = MaxCompletions };
            }
            else
            {
                listBox.ItemsSource = new LimitedCollectionView(itemsSource) { Limit = MaxCompletions };
            }
            if (listBox.Items.Count == 0) InternalClosePopup();
        }

        #endregion

        #region Binding Dependency Property

        public string Binding
        {
            get => (string)GetValue(BindingProperty);
            set => SetValue(BindingProperty, value);
        }

        // Using a DependencyProperty as the backing store for Binding.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty BindingProperty =
            DependencyProperty.Register("Binding", typeof(string), typeof(AutoCompleteTextBox), new UIPropertyMetadata(null));

        #endregion

        #region ItemTemplate Dependency Property

        public DataTemplate ItemTemplate
        {
            get => (DataTemplate)GetValue(ItemTemplateProperty);
            set => SetValue(ItemTemplateProperty, value);
        }

        // Using a DependencyProperty as the backing store for ItemTemplate.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ItemTemplateProperty =
            ItemsControl.ItemTemplateProperty.AddOwner(
                typeof(AutoCompleteTextBox),
                new UIPropertyMetadata(null, OnItemTemplateChanged));

        private static void OnItemTemplateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            AutoCompleteTextBox actb = d as AutoCompleteTextBox;
            if (actb == null) return;
            actb.OnItemTemplateChanged(e.NewValue as DataTemplate);
        }

        private void OnItemTemplateChanged(DataTemplate p)
        {
            if (listBox == null) return;
            listBox.ItemTemplate = p;
        }

        #endregion

        #region ItemContainerStyle Dependency Property

        public Style ItemContainerStyle
        {
            get => (Style)GetValue(ItemContainerStyleProperty);
            set => SetValue(ItemContainerStyleProperty, value);
        }

        // Using a DependencyProperty as the backing store for ItemContainerStyle.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ItemContainerStyleProperty =
            ItemsControl.ItemContainerStyleProperty.AddOwner(
                typeof(AutoCompleteTextBox),
                new UIPropertyMetadata(null, OnItemContainerStyleChanged));

        private static void OnItemContainerStyleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            AutoCompleteTextBox actb = d as AutoCompleteTextBox;
            actb?.OnItemContainerStyleChanged(e.NewValue as Style);
        }

        private void OnItemContainerStyleChanged(Style p)
        {
            if (listBox == null) return;
            listBox.ItemContainerStyle = p;
        }

        #endregion

        #region MaxCompletions Dependency Property

        public int MaxCompletions
        {
            get => (int)GetValue(MaxCompletionsProperty);
            set => SetValue(MaxCompletionsProperty, value);
        }

        // Using a DependencyProperty as the backing store for MaxCompletions.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MaxCompletionsProperty =
            DependencyProperty.Register("MaxCompletions", typeof(int), typeof(AutoCompleteTextBox), new UIPropertyMetadata(int.MaxValue));

        #endregion

        #region ItemTemplateSelector Dependency Property

        public DataTemplateSelector ItemTemplateSelector
        {
            get => (DataTemplateSelector)GetValue(ItemTemplateSelectorProperty);
            set => SetValue(ItemTemplateSelectorProperty, value);
        }

        // Using a DependencyProperty as the backing store for ItemTemplateSelector.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ItemTemplateSelectorProperty =
            ItemsControl.ItemTemplateSelectorProperty.AddOwner(typeof(AutoCompleteTextBox), new UIPropertyMetadata(null, OnItemTemplateSelectorChanged));

        private static void OnItemTemplateSelectorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            AutoCompleteTextBox actb = d as AutoCompleteTextBox;
            actb?.OnItemTemplateSelectorChanged(e.NewValue as DataTemplateSelector);
        }

        private void OnItemTemplateSelectorChanged(DataTemplateSelector p)
        {
            if (listBox == null) return;
            listBox.ItemTemplateSelector = p;
        }

        #endregion

        static AutoCompleteTextBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(AutoCompleteTextBox), new FrameworkPropertyMetadata(typeof(AutoCompleteTextBox)));
        }

        private void InternalClosePopup()
        {
            if (popup != null)
                popup.IsOpen = false;
        }
        private void InternalOpenPopup()
        {
            popup.IsOpen = true;
            if (listBox != null) listBox.SelectedIndex = -1;
        }
        public void ShowPopup()
        {
            if (listBox == null || popup == null) InternalClosePopup();
            else if (listBox.Items.Count == 0) InternalClosePopup();
            else InternalOpenPopup();
        }
        private void SetTextValueBySelection(object obj, bool moveFocus)
        {
            if (popup != null)
            {
                InternalClosePopup();
                Dispatcher.Invoke(new Action(() =>
                {
                    Focus();
                    if (moveFocus)
                        MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
                }), System.Windows.Threading.DispatcherPriority.Background);
            }

            // Retrieve the Binding object from the control.
            var originalBinding = BindingOperations.GetBinding(this, BindingProperty);
            if (originalBinding == null) return;

            // Binding hack - not really necessary.
            //Binding newBinding = new Binding()
            //{
            //    Path = new PropertyPath(originalBinding.Path.Path, originalBinding.Path.PathParameters),
            //    XPath = originalBinding.XPath,
            //    Converter = originalBinding.Converter,
            //    ConverterParameter = originalBinding.ConverterParameter,
            //    ConverterCulture = originalBinding.ConverterCulture,
            //    StringFormat = originalBinding.StringFormat,
            //    TargetNullValue = originalBinding.TargetNullValue,
            //    FallbackValue = originalBinding.FallbackValue
            //};
            //newBinding.Source = obj;
            //BindingOperations.SetBinding(dummy, TextProperty, newBinding);

            // Set the dummy's DataContext to our selected object.
            dummy.DataContext = obj;

            // Apply the binding to the dummy FrameworkElement.
            BindingOperations.SetBinding(dummy, TextProperty, originalBinding);
            suppressEvent = true;

            // Get the binding's resulting value.
            Text = dummy.GetValue(TextProperty).ToString();
            suppressEvent = false;
            listBox.SelectedIndex = -1;
            SelectAll();
        }
        protected override void OnTextChanged(TextChangedEventArgs e)
        {
            base.OnTextChanged(e);
            if (suppressEvent) return;
            textCache = Text ?? "";

            if (popup != null && textCache == "")
            {
                InternalClosePopup();
            }
            else if (listBox != null)
            {
                if (filter != null)
                    listBox.Items.Filter = FilterFunc;

                if (popup != null)
                {
                    if (listBox.Items.Count == 0)
                        InternalClosePopup();
                    else
                        InternalOpenPopup();
                }
            }
        }

        private bool FilterFunc(object obj)
        {
            return filter(obj, textCache);
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            popup = Template.FindName("PART_Popup", this) as Popup;
            listBox = Template.FindName("PART_ListBox", this) as ListBox;
            if (listBox != null)
            {
                listBox.PreviewMouseDown += new MouseButtonEventHandler(ListBox_MouseUp);
                listBox.KeyDown += new KeyEventHandler(ListBox_KeyDown);
                OnItemsSourceChanged(ItemsSource);
                OnItemTemplateChanged(ItemTemplate);
                OnItemContainerStyleChanged(ItemContainerStyle);
                OnItemTemplateSelectorChanged(ItemTemplateSelector);
                if (filter != null)
                    listBox.Items.Filter = FilterFunc;
            }
        }
        protected override void OnLostFocus(RoutedEventArgs e)
        {
            base.OnLostFocus(e);
            if (suppressEvent) return;
            if (popup != null)
            {
                InternalClosePopup();
            }
        }
        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            base.OnPreviewKeyDown(e);
            var fs = FocusManager.GetFocusScope(this);
            var o = FocusManager.GetFocusedElement(fs);
            if (e.Key == Key.Escape)
            {
                InternalClosePopup();
                Focus();
            }
            else if (e.Key == Key.Down)
            {
                if (listBox != null && ReferenceEquals(o, this))
                {
                    suppressEvent = true;
                    listBox.Focus();
                    suppressEvent = false;
                }
            }
        }

        private void ListBox_MouseUp(object sender, MouseButtonEventArgs e)
        {
            var dep = (DependencyObject)e.OriginalSource;
            while ((dep != null) && !(dep is ListBoxItem))
            {
                dep = VisualTreeHelper.GetParent(dep);
            }
            if (dep == null) return;
            var item = listBox.ItemContainerGenerator.ItemFromContainer(dep);
            if (item == null) return;
            SetTextValueBySelection(item, false);
        }

        private void ListBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter || e.Key == Key.Return)
                SetTextValueBySelection(listBox.SelectedItem, false);
            else if (e.Key == Key.Tab)
                SetTextValueBySelection(listBox.SelectedItem, true);
        }

        private class LimitedListCollectionView : CollectionView, IEnumerable
        {
            public int Limit { get; set; }

            public LimitedListCollectionView(IEnumerable list)
                : base(list)
            {
                Limit = int.MaxValue;
            }

            public override int Count { get { return Math.Min(base.Count, Limit); } }

            public override bool MoveCurrentToLast()
            {
                return base.MoveCurrentToPosition(Count - 1);
            }

            public override bool MoveCurrentToNext()
            {
                if (base.CurrentPosition == Count - 1)
                    return base.MoveCurrentToPosition(base.Count);
                else
                    return base.MoveCurrentToNext();
            }

            public override bool MoveCurrentToPrevious()
            {
                if (base.IsCurrentAfterLast)
                    return base.MoveCurrentToPosition(Count - 1);
                else
                    return base.MoveCurrentToPrevious();
            }

            public override bool MoveCurrentToPosition(int position)
            {
                if (position < Count)
                    return base.MoveCurrentToPosition(position);
                else
                    return base.MoveCurrentToPosition(base.Count);
            }

            #region IEnumerable Members

            IEnumerator IEnumerable.GetEnumerator()
            {
                do
                {
                    yield return CurrentItem;
                } while (MoveCurrentToNext());
            }

            #endregion
        }

        private class LimitedCollectionView : CollectionView, IEnumerable
        {
            public int Limit { get; set; }

            public LimitedCollectionView(IEnumerable list)
                : base(list)
            {
                Limit = int.MaxValue;
            }

            public override int Count { get { return Math.Min(base.Count, Limit); } }

            public override bool MoveCurrentToLast()
            {
                return base.MoveCurrentToPosition(Count - 1);
            }

            public override bool MoveCurrentToNext()
            {
                if (base.CurrentPosition == Count - 1)
                    return base.MoveCurrentToPosition(base.Count);
                else
                    return base.MoveCurrentToNext();
            }

            public override bool MoveCurrentToPrevious()
            {
                if (base.IsCurrentAfterLast)
                    return base.MoveCurrentToPosition(Count - 1);
                else
                    return base.MoveCurrentToPrevious();
            }

            public override bool MoveCurrentToPosition(int position)
            {
                if (position < Count)
                    return base.MoveCurrentToPosition(position);
                else
                    return base.MoveCurrentToPosition(base.Count);
            }

            #region IEnumerable Members

            IEnumerator IEnumerable.GetEnumerator()
            {
                do
                {
                    yield return CurrentItem;
                } while (MoveCurrentToNext());
            }

            #endregion
        }
    }
}
