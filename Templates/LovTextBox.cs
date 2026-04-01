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
using System.Windows.Data;
using System.Windows.Documents;

namespace SIMA.Templates
{
    public class LovObject
    {
        public int? Id { get; set; }
        public string? Value { get; set; }
        
    }
    public class LovTextBox : Control
    {
        private TextBox? _textBox;
        private Popup? _popup;
        private ListBox? _listBox;
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
             DependencyProperty.Register(nameof(ItemsSource),typeof(IEnumerable),typeof(LovTextBox),new PropertyMetadata(null, OnItemsSourceChanged));

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

        public IEnumerable<LovObject> OriginalSource
        {
            get => (IEnumerable<LovObject>)GetValue(OriginalSourceProperty);
            private set => SetValue(OriginalSourceProperty, value);
        }
        public static readonly DependencyProperty OriginalSourceProperty =
            DependencyProperty.Register(nameof(OriginalSource),typeof(IEnumerable<LovObject>),typeof(LovTextBox),new PropertyMetadata(null));
        
        public bool IsChild
        {
            get => (bool)GetValue(IsChildProperty);
            set => SetValue(IsChildProperty, value);
        }
        public static readonly DependencyProperty IsChildProperty =
            DependencyProperty.Register(nameof(IsChild), typeof(bool), typeof(LovTextBox));




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
        private static void OnItemsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (LovTextBox)d;

            if (control.OriginalSource == null || control.IsChild)
                control.OriginalSource = (IEnumerable<LovObject>)e.NewValue;

            if (control._listBox != null)
                control._listBox.ItemsSource = (IEnumerable)e.NewValue;
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
        /// <summary>
        /// Set and Raise the Routed Event Property Change fot the Texbox  
        /// </summary>
        /// <param name="mlistbox"></param>
        private void selectValue(ListBox mlistbox) {
            if (mlistbox.SelectedItem != null) {
                LovObject? lovItem = (LovObject)mlistbox.SelectedItem;
                SelectedItem = lovItem;
                Text = lovItem?.Value;

                if(lovItem != null)
                    RaiseEvent(new RoutedEventArgs(LovTextBoxChangedEvent));
            }
        }
        /// <summary>
        /// Open the popup and Load the source given the value of the Textbox Changed Event
        /// </summary>
        /// <param name="_renderAll"></param>
        private void OpenPopup(bool _renderAll = false)
        {
            if (_popup != null)
            {
                IEnumerable<LovObject> list = null;
                _popup.IsOpen = true;
                _button.Content = _ButtonOpen;
                if (_isready)
                {
                    list = OriginalSource.Where(p => (_renderAll || string.IsNullOrEmpty(_textBox.Text) || p.Value.Contains(_textBox.Text, StringComparison.OrdinalIgnoreCase)));
                    SetValue(ItemsSourceProperty, list);
                }


                if (_listBox != null)
                {
                    _listBox.ItemsSource = list;
                    _isready = true;
                }

            }
        }
        /// <summary>
        /// Cloase the state of the popup
        /// </summary>
        private void ClosePopup()
        {
            if (_popup != null)
            {
                _popup.IsOpen = false;
                _button.Content = _ButtonClose;
            }
        }
        /// <summary>
        /// select the value clicked or key press enter in the listbox
        /// </summary>
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
        /// <summary>
        /// Return the (LovTextBox)object selected
        /// </summary>
        /// <returns></returns>
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
        /// <summary>
        /// Return the (LovTextBox)object selected(to handle Event sender when changeEvent fore ) 
        /// </summary>
        /// <returns></returns>
        public LovObject getSelectedItem(object sender) {
            if (sender != null)
                return (LovObject)((LovTextBox)sender).SelectedItem;
            else
                return new LovObject();
        }
        /// <summary>
        /// 
        /// </summary>
        public void Close() {
            ClosePopup();
        }
        #endregion

    }

}
