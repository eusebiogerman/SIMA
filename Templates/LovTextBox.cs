using System;
using System.Collections;
using System.Linq;
using System.Windows.Controls.Primitives;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using System.Runtime.CompilerServices;
using System.Reflection;

namespace SIMA.Templates
{
    public class LovObject
    {
        public int? Id { get; set; }
        public string? Value { get; set; }
        
    }
    public class LovTextBox : Control
    {
        private TextBox _textBox;
        private Popup _popup;
        private ListBox _listBox;
        private Button? _button;
        private IEnumerable _orignalSource;
        private bool _isready = false;
        private const string _ButtonOpen = "▲";
        private const string _ButtonClose = "▼";

        static LovTextBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(LovTextBox),
                new FrameworkPropertyMetadata(typeof(LovTextBox)));
        }
 
        #region Dependecy Properties
        public IEnumerable ItemsSource
        {
            get => (IEnumerable)GetValue(ItemsSourceProperty);
            set  {
                  _orignalSource = value;
                   SetValue(ItemsSourceProperty, value);
            }
        }
        public static readonly DependencyProperty ItemsSourceProperty =
            DependencyProperty.Register(nameof(ItemsSource), typeof(IEnumerable), typeof(LovTextBox));

        public string DisplayMemberPath
        {
            get => (string)GetValue(DisplayMemberPathProperty);
            set => SetValue(DisplayMemberPathProperty, value);
        }
        public static readonly DependencyProperty DisplayMemberPathProperty =
            DependencyProperty.Register(nameof(DisplayMemberPath), typeof(string), typeof(LovTextBox));

        public string SelectedValuePath
        {
            get => (string)GetValue(SelectedValuePathProperty);
            set => SetValue(SelectedValuePathProperty, value);
        }
        public static readonly DependencyProperty SelectedValuePathProperty =
            DependencyProperty.Register(nameof(SelectedValuePath), typeof(string), typeof(LovTextBox));


        public int SelectedIndex
        {
            get => (int)GetValue(SelectedIndexProperty);
            set => SetValue(SelectedIndexProperty, value);
        }
        public static readonly DependencyProperty SelectedIndexProperty =
            DependencyProperty.Register(nameof(SelectedIndex), typeof(int), typeof(LovTextBox));
               

        public object SelectedItem
        {
            get => GetValue(SelectedItemProperty);
            set => SetValue(SelectedItemProperty, value);
        }
        public static readonly DependencyProperty SelectedItemProperty =
            DependencyProperty.Register(nameof(SelectedItem), typeof(object), typeof(LovTextBox));

        public string Text
        {
            get => (string)GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register(nameof(Text), typeof(string), typeof(LovTextBox));

        public IEnumerable<LovObject> OrignalSource { get => (IEnumerable<LovObject>)_orignalSource; }
        #endregion

        #region Events
        private void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Down)
            {
                if (_popup.IsOpen)
                {
                    _listBox?.Focus();
                   /* if (_listBox?.Items.Count > 0)
                        _listBox.SelectedIndex = 0;*/
                }
            }
        }
        private void ListBox_KeyDown(object sender, KeyEventArgs e)
        {
           
            if (e.Key == Key.Enter)
            {
                CommitSelection();
                e.Handled = true;
            }
            else if (e.Key == Key.Escape)
            {
                ClosePopup();
            }

        }

        private void ListBox_SelectionChange(object sender, RoutedEventArgs e)
        {
            CommitSelection();
        }

        public static readonly RoutedEvent LovTextBoxChangedEvent =
        EventManager.RegisterRoutedEvent("LovTextBoxChanged", RoutingStrategy.Direct, typeof(RoutedEventHandler), typeof(LovTextBox));
        public event RoutedEventHandler LovTextBoxChanged
        {
            add => AddHandler(LovTextBoxChangedEvent, value);
            remove => RemoveHandler(LovTextBoxChangedEvent, value);
        }
        #endregion

        #region Base Overriding Abstration
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _textBox = GetTemplateChild("PART_TextBox") as TextBox;
            _popup = GetTemplateChild("PART_Popup") as Popup;
            _listBox = GetTemplateChild("PART_ListBox") as ListBox;
            _button = GetTemplateChild("PART_Button") as Button;
            

            if (_textBox != null)
            {
                _textBox.TextChanged += (s, e) => OpenPopup();
                _textBox.PreviewKeyDown += TextBox_KeyDown;
            }

            if (_listBox != null)
            {
                _listBox.Width = this.Width;
                _listBox.MouseDoubleClick += (s, e) => CommitSelection();
                 _listBox.PreviewKeyDown += ListBox_KeyDown;
                _listBox.SelectionChanged += ListBox_SelectionChange;
                

                if (_textBox != null) {
                    selectValue(_listBox);
                }
            }

            if (_button != null) { 
                _button.Height = this.Height;
                _button.Width = (this.Width * 0.1) ;
                _button.Click += (s, e) => OpenPopup(true);
            }

        }
        #endregion

        #region Popup LOV Methods
        private void selectValue(ListBox mlistbox) {
            if (mlistbox.SelectedItem != null) {
                LovObject? lovItem = (LovObject)mlistbox.SelectedItem;
                SelectedItem = lovItem;
                Text = lovItem?.Value;

                if(lovItem != null)
                    RaiseEvent(new RoutedEventArgs(LovTextBoxChangedEvent));
            }
        }
        private void OpenPopup(bool _renderAll = false)
        {
            if (_popup != null)
            {
                _popup.IsOpen = true;
                _button.Content = _ButtonOpen;
                if (_isready)
                {
                    IEnumerable<LovObject> list = ((IEnumerable<LovObject>)_orignalSource).Where(p => (_renderAll || string.IsNullOrEmpty(_textBox.Text) || p.Value.Contains(_textBox.Text, StringComparison.OrdinalIgnoreCase)));
                    SetValue(ItemsSourceProperty, list);
                }

                if (_listBox.IsInitialized && !_isready)
                {
                    _orignalSource = ItemsSource;
                    _isready = true;
                }

                
            }
        }
        private void ClosePopup()
        {
            if (_popup != null)
            {
                _popup.IsOpen = false;
                _button.Content = _ButtonClose;
            }
        }
        private void CommitSelection()
        {
            if (_listBox?.SelectedItem != null)
            {
                selectValue(_listBox);
                ClosePopup();
            }
        }
        #endregion

        #region Public Methods
        public LovObject getSelectedItem()
        {
            var sender = this;
            var EmptyItem = new LovObject();
            if (sender != null)
            {
                return sender.SelectedItem != null
                    ? (LovObject)sender.SelectedItem
                    : EmptyItem;
            }else
                return EmptyItem;
        }
        public LovObject getSelectedItem(object sender) {
            if (sender != null)
                return (LovObject)((LovTextBox)sender).SelectedItem;
            else
                return new LovObject();
        }
        public void Close() {
            ClosePopup();
        }
        #endregion

    }

}
