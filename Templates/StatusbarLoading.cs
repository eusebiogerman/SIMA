using Microsoft.Extensions.Configuration;
using SIMA.Helper;
using SIMA.Infrastructure.Repositories;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace SIMA.Templates
{
    public class StatusItem
    {
        public string Message { get; set; }
        public Brush Color { get; set; }
        public string TraceError { get; set; }
    }

    public class StatusbarLoading : Control
    {
        private IEnumerable _orignalSource;
        private Popup? _popup;
        private ListBox? _listBox;
        private Border? _border;
        private Ellipse? _ellipse;
        SIMA.Helper.BoolToVisibilityConverter _utilConvert;
        public const int DefaulDelay = 500;

        static StatusbarLoading()
        {

            DefaultStyleKeyProperty.OverrideMetadata(
              typeof(StatusbarLoading),
              new FrameworkPropertyMetadata(typeof(StatusbarLoading)));
        }

        #region Loading Dependecy Properties
        public bool IsLoading
        {
            get => (bool)GetValue(IsLoadingProperty);
            set { SetValue(IsLoadingProperty, value); }
        }
        public static readonly DependencyProperty IsLoadingProperty =
            DependencyProperty.Register(nameof(IsLoading), typeof(bool), typeof(StatusbarLoading));

        public string Text
        {
            get => (string)GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register(nameof(Text), typeof(string), typeof(StatusbarLoading));

        public int Delay
        {
            get => (int)GetValue(IsDelayProperty);
            set { SetValue(IsDelayProperty, value); }
        }
        public static readonly DependencyProperty IsDelayProperty =
            DependencyProperty.Register(nameof(Delay), typeof(int), typeof(StatusbarLoading));
        #endregion

        #region StatusBar Dependecy Properties
        public string Message
        {
            get => (string)GetValue(MessageProperty);
            set => SetValue(MessageProperty, value);
        }
        public static readonly DependencyProperty MessageProperty =
            DependencyProperty.Register(nameof(Message), typeof(string), typeof(StatusbarLoading));

        public Brush Color
        {
            get => (Brush)GetValue(ColorProperty);
            set { SetValue(ColorProperty, value); }
        }
        public static readonly DependencyProperty ColorProperty =
            DependencyProperty.Register(nameof(Color), typeof(Brush), typeof(StatusbarLoading));

        public bool IsPin
        {
            get => (bool)GetValue(IsPinProperty);
            set { SetValue(IsPinProperty, value); }
        }
        public static readonly DependencyProperty IsPinProperty =
            DependencyProperty.Register(nameof(IsPin), typeof(bool), typeof(StatusbarLoading));

        public IEnumerable ItemsSource
        {
            get => (IEnumerable)GetValue(ItemsSourceProperty);
            set
            {
                _orignalSource = value;
                SetValue(ItemsSourceProperty, value);
            }
        }
        public static readonly DependencyProperty ItemsSourceProperty =
             DependencyProperty.Register(nameof(ItemsSource), typeof(IEnumerable), typeof(StatusbarLoading), new PropertyMetadata(null, OnItemsSourceChanged));

        public string DisplayMemberPath
        {
            get => (string)GetValue(DisplayMemberPathProperty);
            set => SetValue(DisplayMemberPathProperty, value);
        }
        public static readonly DependencyProperty DisplayMemberPathProperty =
            DependencyProperty.Register(nameof(DisplayMemberPath), typeof(string), typeof(StatusbarLoading));

        public string SelectedValuePath
        {
            get => (string)GetValue(SelectedValuePathProperty);
            set => SetValue(SelectedValuePathProperty, value);
        }
        public static readonly DependencyProperty SelectedValuePathProperty =
            DependencyProperty.Register(nameof(SelectedValuePath), typeof(string), typeof(StatusbarLoading));

        public int SelectedIndex
        {
            get => (int)GetValue(SelectedIndexProperty);
            set
            {
                SetValue(SelectedIndexProperty, value);
            }
        }
        public static readonly DependencyProperty SelectedIndexProperty =
            DependencyProperty.Register(nameof(SelectedIndex), typeof(int), typeof(StatusbarLoading));

        public object SelectedItem
        {
            get => GetValue(SelectedItemProperty);
            set => SetValue(SelectedItemProperty, value);
        }
        public static readonly DependencyProperty SelectedItemProperty =
            DependencyProperty.Register(nameof(SelectedItem), typeof(object), typeof(StatusbarLoading));
        
        public IEnumerable<StatusItem> OriginalSource
        {
            get => (IEnumerable<StatusItem>)GetValue(OriginalSourceProperty);
            private set => SetValue(OriginalSourceProperty, value);
        }
        public static readonly DependencyProperty OriginalSourceProperty =
            DependencyProperty.Register(nameof(OriginalSource), typeof(IEnumerable<StatusItem>), typeof(StatusbarLoading), new PropertyMetadata(null));
    
        #endregion

        #region Events
        /// <summary>
        /// 
        /// </summary>
        /// <param name="d"></param>
        /// <param name="e"></param>
        private static void OnItemsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var control = (StatusbarLoading)d;

            if (control._listBox != null)
                control._listBox.ItemsSource = (IEnumerable)e.NewValue;
            else
                control._orignalSource = (IEnumerable)e.NewValue;
        }
        /// <summary>
        /// /
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _border_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            var control = (Border)sender;
            if (control != null && _ellipse != null) 
                    _ellipse.Width = e.NewSize.Height * 0.5;

        }
        #endregion

        #region Base Overriding Abstration
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _utilConvert = new SIMA.Helper.BoolToVisibilityConverter();

            _ellipse = GetTemplateChild("PART_Ellipse") as Ellipse;
            _popup = GetTemplateChild("PART_Popup") as Popup;
            _listBox = GetTemplateChild("PART_ListBox") as ListBox;
            _border = GetTemplateChild("PART_Border") as Border;

            if (_border != null) 
                _border.SizeChanged += _border_SizeChanged;

            if (_listBox != null)
                _listBox.ItemsSource = ItemsSource ?? _orignalSource;

            if (_ellipse != null)
                _ellipse.Visibility = (Visibility)_utilConvert.Convert(IsLoading,typeof(Visibility), null,null); 

        }
        #endregion

        #region Loading Public Methods
        /// <summary>
        /// Open StatusbarLoading given the action delegated
        /// </summary>
        /// <param name="process">Action delegated</param>
        /// <param name="isloading">Open load popup {True/False}</param>
        /// <param name="text">Loadind Message</param>
        /// <param name="delay">Decent Time to delay</param>
        public void SetLoadingState(Action process, bool isloading, string text = "StatusbarLoading....", int delay = DefaulDelay)
        {

            Delay = delay;
            IsLoading =  isloading;
            Text = text;

            if (_popup != null)
            {
                try
                {
                    RunProgress(isloading, text, delay);
                    new Task(() => { }).WaitAsync(TimeSpan.FromMilliseconds(Delay)).GetAwaiter().OnCompleted(() =>
                    {
                        process.Invoke();
                        StopProgress();
                    });
                }
                catch (Exception ex)
                {
                    StopProgress();
                    throw ex;
                }

            }
        }
        /// <summary>
        ///  Open StatusbarLoading for Data Base Processind(Delay = ping the DB result) Async given the action delegated
        /// </summary>
        /// <param name="process">Action delegated</param>
        /// <param name="isloading">Open load popup {True/False}</param>
        /// <param name="text">Loadind Message</param>
        public void SetLoadingStateDataBase(Action process, IConfiguration config, bool isloading = true, string text = "StatusbarLoading....")
        {

            int delay = DataBaseServices.PingSqlServer(config) + DefaulDelay;
            if (_popup != null)
            {
                try
                {
                    RunProgress(isloading, text, delay);
                    new Task(() => { }).WaitAsync(TimeSpan.FromMilliseconds(Delay)).GetAwaiter().OnCompleted(() =>
                    {
                        process.Invoke();
                        StopProgress();
                    });
                }
                catch (Exception ex)
                {
                    StopProgress();
                    throw ex;
                }
            }
        }
        /// <summary>
        ///  Open StatusbarLoading for Not Async Data Base Processing(Delay = ping the DB result) Async given the action delegated
        /// </summary>
        /// <param name="process">Action delegated</param>
        /// <param name="isloading">Open load popup {True/False}</param>
        /// <param name="text">Loadind Message</param>
        public async Task SetLoadingStateDataBaseASync(Action process, IConfiguration config, bool isloading = true, string text = "StatusbarLoading....")
        {

            int delay = DataBaseServices.PingSqlServer(config) + DefaulDelay;
            if (_popup != null)
            {
                try
                {
                    RunProgress(isloading, text, delay);
                    await Task.Delay(delay);
                    process.Invoke();
                    StopProgress();

                }
                catch (Exception ex)
                {
                    StopProgress();
                    throw ex;
                }
            }
        }
        /// <summary>
        ///  Open StatusbarLoading for Data Base Processind(Delay = ping the DB result) Async given the action delegated
        /// </summary>
        /// <param name="process">Function boolean delegated </param>
        /// <param name="isloading">Open load popup {True/False}</param>
        /// <param name="text">Loadind Message</param>
        /// <returns>Scalar Boolean </returns>
        public bool SetLoadingStateDataBaseResult(Func<Task<bool>> process, IConfiguration config, bool isloading = true, string text = "StatusbarLoading....")
        {

            int delay = DataBaseServices.PingSqlServer(config) + DefaulDelay;
            bool _result = false;

            if (_popup != null)
            {
                try
                {
                    RunProgress(isloading, text, delay);
                    new Task(() => { }).WaitAsync(TimeSpan.FromMilliseconds(Delay)).GetAwaiter().OnCompleted(async () =>
                    {
                        _result = await process.Invoke();
                        StopProgress();
                    });
                    return _result;
                }
                catch (Exception ex)
                {
                    StopProgress();
                    throw ex;
                }
            }
            return _result;
        }
        /// <summary>
        /// Open the StatusbarLoading
        /// </summary>
        public bool RunProgress(bool isloading, string text = "StatusbarLoading....", int delay = DefaulDelay)
        {
            IsLoading = isloading;
            if (_ellipse != null && IsLoading)
            {
                _ellipse.Visibility = (Visibility)_utilConvert.Convert(IsLoading, typeof(Visibility), null, null); 
                Delay = delay;
                Text = text;
            }
            return IsLoading;
        }
        /// <summary>
        /// Close or desactivate the StatusbarLoading
        /// </summary>
        public void StopProgress()
        {
            if (_ellipse != null)
            {
                _ellipse.Visibility = (Visibility)_utilConvert.Convert(false, typeof(Visibility), null, null); 
                IsLoading = false;
                Text = string.Empty;
                Delay = 0;
            }
        }
        /// <summary>
        /// Delay process given the Delay property setted or Default Dealy
        /// </summary>
        /// <returns></returns>
        public async Task DelayProgress()
        {
            if (_popup != null)
            {
                if (IsLoading)
                    await Task.Delay(Delay);
            }
        }
        /// <summary>
        /// Delay process given the Data Base Delay property setted or Default Delay
        /// </summary>
        /// <returns></returns>
        public async Task DelayDBProgress()
        {
            if (_popup != null)
            {
                if (IsLoading)
                {
                    Delay = DataBaseServices.PingSqlServer(new Util().CustomConfiguration()) + DefaulDelay;
                    await Task.Delay(Delay);
                }
            }
        }
        #endregion

        #region Lixbox Public Methods
        /// <summary>
        /// Return the (LovTextBox)object selected
        /// </summary>
        /// <returns></returns>
        public StatusItem getSelectedItem()
        {
            var sender = this;
            var EmptyItem = new StatusItem();
            if (sender != null)
            {
                return sender.SelectedItem != null
                    ? (StatusItem)sender.SelectedItem
                    : EmptyItem;
            }
            else
                return EmptyItem;
        }
        /// <summary>
        /// Return the (LovTextBox)object selected(to handle Event sender when changeEvent fore ) 
        /// </summary>
        /// <returns></returns>
        public StatusItem getSelectedItem(object sender)
        {
            if (sender != null)
                return (StatusItem)((StatusbarLoading)sender).SelectedItem;
            else
                return new StatusItem();
        }
        #endregion





    }
}
