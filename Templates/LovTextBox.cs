using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls.Primitives;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows;

namespace SIMA.Templates
{
    public class LovTextBox : Control
    {
        private TextBox _textBox;
        private Popup _popup;
        private ListBox _listBox;

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
            set => SetValue(ItemsSourceProperty, value);
        }
        public static readonly DependencyProperty ItemsSourceProperty =
            DependencyProperty.Register(nameof(ItemsSource), typeof(IEnumerable), typeof(LovTextBox));

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
        #endregion

        #region Events
        private void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Down)
            {
                _listBox?.Focus();
                if (_listBox?.Items.Count > 0)
                    _listBox.SelectedIndex = 0;
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
        #endregion

        #region Base Overriding Abstration
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _textBox = GetTemplateChild("PART_TextBox") as TextBox;
            _popup = GetTemplateChild("PART_Popup") as Popup;
            _listBox = GetTemplateChild("PART_ListBox") as ListBox;

            if (_textBox != null)
            {
                _textBox.TextChanged += (s, e) => OpenPopup();
                _textBox.PreviewKeyDown += TextBox_KeyDown;
            }

            if (_listBox != null)
            {
                _listBox.MouseDoubleClick += (s, e) => CommitSelection();
                _listBox.PreviewKeyDown += ListBox_KeyDown;
            }
        }
        #endregion

        #region Popup LOV Methods
        private void OpenPopup()
        {
            if (_popup != null)
                _popup.IsOpen = true;
        }
        private void ClosePopup()
        {
            if (_popup != null)
                _popup.IsOpen = false;
        }
        private void CommitSelection()
        {
            if (_listBox?.SelectedItem != null)
            {
                SelectedItem = _listBox.SelectedItem;
                Text = SelectedItem?.ToString();
                ClosePopup();
            }
        }
        #endregion


    }

}
