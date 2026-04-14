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
    public class Loading : Control
    {
        private Popup? _popup;
        private Ellipse? _ellipse;
        public const int DefaulDelay = 500; 

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

        #region Public Methods
        /// <summary>
        /// Open loading given the action delegated
        /// </summary>
        /// <param name="process">Action delegated</param>
        /// <param name="isloading">Open load popup {True/False}</param>
        /// <param name="message">Loadind Message</param>
        /// <param name="delay">Decent Time to delay</param>
        public void SetLoadingState(Action process, bool isloading,string message="Loading....",int delay= DefaulDelay)
        {
            if (_popup != null)
            {
                try
                {
                    RunProgress(isloading, message, delay);
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
        /// <summary>
        ///  Open loading for Data Base Processind(Delay = ping the DB result) Async given the action delegated
        /// </summary>
        /// <param name="process">Action delegated</param>
        /// <param name="isloading">Open load popup {True/False}</param>
        /// <param name="message">Loadind Message</param>
        public void SetLoadingStateDataBase(Action process, IConfiguration config, bool isloading=true, string message = "Loading...." )
        {
            int delay = DataBaseServices.PingSqlServer(config) + DefaulDelay;
            if (_popup != null )
            {
                try
                {
                   RunProgress(isloading, message, delay);
                   new Task(() => {}).WaitAsync(TimeSpan.FromMilliseconds(Delay)).GetAwaiter().OnCompleted(() =>
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
        ///  Open loading for Not Async Data Base Processing(Delay = ping the DB result) Async given the action delegated
        /// </summary>
        /// <param name="process">Action delegated</param>
        /// <param name="isloading">Open load popup {True/False}</param>
        /// <param name="message">Loadind Message</param>
        public async Task SetLoadingStateDataBaseASync(Action process, IConfiguration config, bool isloading = true, string message = "Loading....")
        {

            int delay = DataBaseServices.PingSqlServer(config) + DefaulDelay;
            if (_popup != null)
            {
                try
                {
                    RunProgress(isloading, message, delay);
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
        ///  Open loading for Data Base Processind(Delay = ping the DB result) Async given the action delegated
        /// </summary>
        /// <param name="process">Function boolean delegated </param>
        /// <param name="isloading">Open load popup {True/False}</param>
        /// <param name="message">Loadind Message</param>
        /// <returns>Scalar Boolean </returns>
        public bool SetLoadingStateDataBaseResult(Func<Task<bool>> process, IConfiguration config, bool isloading = true, string message = "Loading....")
        {

            int delay = DataBaseServices.PingSqlServer(config) + DefaulDelay;
            bool _result = false;
            if (_popup != null)
            {
                try
                {
                    RunProgress(isloading, message, delay);
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
        /// Open the Loading
        /// </summary>
        public bool RunProgress(bool isloading, string message = "Loading....", int delay = DefaulDelay)
        {
            IsLoading = isloading;
            if (_popup != null && IsLoading)
            {
                _popup.IsOpen = IsLoading;
                Delay = delay;
                Message = message;
            }
            return IsLoading;
        }
        /// <summary>
        /// Close or desactivate the Loading
        /// </summary>
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
        /// <summary>
        /// Delay process given the Delay property setted or Default Dealy
        /// </summary>
        /// <returns></returns>
        public async Task DelayProgress() {
            if (_popup != null)
            {
                if (IsLoading)
                    await Task.Delay(Delay);
            }
        }
        #endregion
    }

}

