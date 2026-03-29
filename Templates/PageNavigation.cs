using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace SIMA.Templates
{
    public class PageNavigation : Control
    {
        private ComboBox _pageComboBox;
        private Button _pagePreviousButton;
        private Button _pageNextButton;
  

        static PageNavigation()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(PageNavigation),
                new FrameworkPropertyMetadata(typeof(PageNavigation)));
        }

        #region Dependecy Properties
        public int PageNumber
        {
            get => (int)GetValue(PageNumberProperty);
            set => SetValue(PageNumberProperty, value);
        }
        public static readonly DependencyProperty PageNumberProperty =
            DependencyProperty.Register(nameof(PageNumber), typeof(int), typeof(PageNavigation), new PropertyMetadata(1));

        public int TotalFound
        {
            get => (int)GetValue(TotalPagesProperty);
            set => SetValue(TotalPagesProperty, value);
        }
        public static readonly DependencyProperty TotalPagesProperty =
            DependencyProperty.Register(nameof(TotalFound), typeof(int), typeof(PageNavigation), new PropertyMetadata(1));

        public IEnumerable ItemsPerPageSource
        {
            get => (IEnumerable)GetValue(ItemsPerPageSourceProperty);
            set => SetValue(ItemsPerPageSourceProperty, value);
        }
        public static readonly DependencyProperty ItemsPerPageSourceProperty =
            DependencyProperty.Register(nameof(ItemsPerPageSource), typeof(IEnumerable), typeof(PageNavigation));

        public int ItemsPerPage
        {
            get => (int)GetValue(ItemsPerPageProperty);
            set => SetValue(ItemsPerPageProperty, value);
        }
        public static readonly DependencyProperty ItemsPerPageProperty =
            DependencyProperty.Register(nameof(ItemsPerPage), typeof(int), typeof(PageNavigation));

        public ICommand PreviousCommand
        {
            get => (ICommand)GetValue(PreviousCommandProperty);
            set => SetValue(PreviousCommandProperty, value);
        }
        public static readonly DependencyProperty PreviousCommandProperty =
            DependencyProperty.Register(nameof(PreviousCommand), typeof(ICommand), typeof(PageNavigation));

        public ICommand NextCommand
        {
            get => (ICommand)GetValue(NextCommandProperty);
            set => SetValue(NextCommandProperty, value);
        }
        public static readonly DependencyProperty NextCommandProperty =
            DependencyProperty.Register(nameof(NextCommand), typeof(ICommand), typeof(PageNavigation));

        public ICommand ItemsPerPageChangedCommand
        {
            get => (ICommand)GetValue(ItemsPerPageChangedCommandProperty);
            set => SetValue(ItemsPerPageChangedCommandProperty, value);
        }

        public static readonly DependencyProperty ItemsPerPageChangedCommandProperty =
            DependencyProperty.Register(nameof(ItemsPerPageChangedCommand), typeof(ICommand), typeof(PageNavigation));

        #endregion

        #region Events
        public static readonly RoutedEvent PageComboboxChangedEvent =
        EventManager.RegisterRoutedEvent("PageComboboxChanged", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(PageNavigation));
        public event RoutedEventHandler PageComboboxChanged
        {
            add => AddHandler(PageComboboxChangedEvent, value);
            remove => RemoveHandler(PageComboboxChangedEvent, value);
        }

        public static readonly RoutedEvent PreviousClickEvent =
        EventManager.RegisterRoutedEvent("PreviousClick", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(PageNavigation));
        public event RoutedEventHandler PreviousClick
        {
            add => AddHandler(PreviousClickEvent, value);
            remove => RemoveHandler(PreviousClickEvent, value);
        }

        public static readonly RoutedEvent NextClickEvent =
        EventManager.RegisterRoutedEvent("NextClick", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(PageNavigation));
        public event RoutedEventHandler NextClick
        {
            add => AddHandler(NextClickEvent, value);
            remove => RemoveHandler(NextClickEvent, value);
        }

       #endregion

        #region Base Overriding Abstration
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _pageComboBox = GetTemplateChild("PART_PageComboBox") as ComboBox;
            _pagePreviousButton = GetTemplateChild("PART_PagePreviousButton") as Button;
            _pageNextButton = GetTemplateChild("PART_PageNextButton") as Button;
            
            ///Combo Event
            if (_pageComboBox != null)
            {
                _pageComboBox.SelectionChanged += (s, e) =>
                {
                    RaiseEvent(new RoutedEventArgs(PageComboboxChangedEvent));
                };
            }

            //Previous Navigation Event
            if (_pagePreviousButton != null)
            {

                _pagePreviousButton.Click += (s, e) =>
                {
                    RaiseEvent(new RoutedEventArgs(PreviousClickEvent));
                };
            }

            //Next Navigation Event
            if (_pageNextButton != null)
            {
                _pageNextButton.Click += (s, e) =>
                {
                    RaiseEvent(new RoutedEventArgs(NextClickEvent));
                };
            }

        }
        #endregion

        #region Combobox Methods
        public void SetItemsPerPageSource(IEnumerable items)
        {
            if (_pageComboBox != null)
                _pageComboBox.ItemsSource = items;
        }
        public object GetSelectedItemsPerPage()
        {
            return _pageComboBox?.SelectedItem;
        }
        public void SetSelectedItemsPerPage(object value)
        {
            if (_pageComboBox != null)
                _pageComboBox.SelectedItem = value;
        }
        #endregion

    }
}