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
using System.Security.Policy;
using System.Windows.Shapes;
using System.Threading.Tasks;
using System.Diagnostics;
using SIMA.Helper;
using SIMA.Infrastructure.Repositories;
using System.Threading;
using Microsoft.Extensions.Configuration;

namespace SIMA.Templates
{
    public class Loading :Control
    {
        private Popup? _popup;
        private Ellipse? _ellipse;

        static Loading()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
              typeof(Loading),
              new FrameworkPropertyMetadata(typeof(Loading)));
        }

        #region Dependecy Properties
        public bool IsLoading
        {
            get => (bool)GetValue(IsLoadingProperty);
            set {SetValue(IsLoadingProperty, value);}
        }
        public static readonly DependencyProperty IsLoadingProperty =
            DependencyProperty.Register(nameof(IsLoading), typeof(bool), typeof(Loading));

        public string Message
        {
            get => (string)GetValue(MessageProperty);
            set => SetValue(MessageProperty, value);
        }
        public static readonly DependencyProperty MessageProperty =
            DependencyProperty.Register(nameof(Message), typeof(string), typeof(Loading));


        public int Delay
        {
            get => (int)GetValue(IsDelayProperty);
            set { SetValue(IsDelayProperty, value) ; }
        }
        public static readonly DependencyProperty IsDelayProperty =
            DependencyProperty.Register(nameof(Delay), typeof(int), typeof(Loading));
        #endregion


        #region Base Overriding Abstration
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _ellipse = GetTemplateChild("PART_Ellipse") as Ellipse;
            _popup = GetTemplateChild("PART_Popup") as Popup;
        }
        #endregion


        public void SetLoadingState(Action process, bool isloading,string message="Loading....",int delay=500)
        {

            Delay = delay;
            IsLoading = isloading;
            Message = message;

            if (_popup != null)
            {
                try
                {
                    _popup.IsOpen = IsLoading;
                    new Task(() => { }).WaitAsync(TimeSpan.FromMilliseconds(Delay)).GetAwaiter().OnCompleted(() => 
                    { 
                        process.Invoke();
                        StopProgress();
                    });
                }catch (Exception ex) {
                    StopProgress();
                    throw ex;
                }

            }
          }

        public void SetLoadingStateDataBase(Action process, IConfiguration config, bool isloading=true, string message = "Loading...." )
        {

            Delay = DataBaseServices.PingSqlServer(config);
            IsLoading = isloading;
            Message = message;

            if (_popup != null)
            {
                try
                {
                    _popup.IsOpen = IsLoading;
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
        public bool SetLoadingStateDataBaseResult(Func<Task<bool>> process, IConfiguration config, bool isloading = true, string message = "Loading....")
        {

            Delay = DataBaseServices.PingSqlServer(config);
            IsLoading = isloading;
            Message = message;
            bool _result = false;

            if (_popup != null)
            {
                try
                {
                    _popup.IsOpen = IsLoading;
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





        public void StopProgress()
        {
            if (_popup != null)
            {
                _popup.IsOpen = false;
                IsLoading = false;
                Message = string.Empty;
                Delay = 0;
            }
        }
    }

}

