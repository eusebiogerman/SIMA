using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using SIMA.Domain.Models.Intefaces;
using SIMA.Domain.Models.Objects;
using SIMA.Domain.Models.Params;
using SIMA.Domain.Models.Views;
using SIMA.Helper;
using SIMA.Helper.Interfaces;
using SIMA.Infrastructure.Repositories;
using SIMA.Infrastructure.Repositories.Interfaces;
using SIMA.Presentation.ViewModel;
using SIMA.Templates;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.DirectoryServices.ActiveDirectory;
using System.Drawing;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace SIMA.Presentation.Repository
{
    public partial class WindowServices<T, V,P> : WindowServicesBase<T, V, P> , IUtilServices<V, P>
    {
        private int _stscnt = 0;
        private int maxCapacity = 50;


        /// <summary>
        /// Used for events that may no excute or fill data storing by _isloade 
        /// and  IsSupressed for the Window DataContext
        /// </summary>
        /// <param name="ex"></param>
        /// <param name="title"></param>
        public void SupressEventComboBox(bool val = true)
        {
            Isloaded = val;
            if (DataContext != null)
                DataContext.IsSupressed = val;

        }
        /// <summary>
        /// Return and Update the message of the render Stock length given the domain Name
        /// </summary>
        /// <param name="total">(int) value count of result DB </param>
        /// <returns>string</returns>
        public string getResultMessage(int total, string domain)
        {
            PageControl.TotalFound = total;
            return $"📊 Show {total} {domain} found";
        }
        /// <summary>
        /// Return and Update the message of the render Stock length given the domain Name
        /// </summary>
        /// <param name="total">(int) value count of result DB </param>
        /// <returns>string</returns>
        public string getResultMessage(int total)
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// Update the Paging Labels given the cuurent Offset and Limit Values
        /// </summary>
        /// <param name="total">(int) value count of result DB </param>
        public void pagingLabels(int total)
        {
            if (Page.isvalidPaging())
            {
                PageControl.TotalFound = total;
                PageControl.PageNumber = Page.Pagenumber;
            }
        }
        /// <summary>
        /// Control the Paging Previous and Next Page Number,Offset and Limit 
        /// </summary>
        /// <param name="direction">Previous or Next</param>
        public void NavigationGrid(DIRECTION direction)
        {
            if (Page.isvalidPaging())
            {
                Page.movePage(direction);
                PageControl.ItemsPerPage = Page.Offset;
            }
        }
        /// <summary>
        /// Restore Initial set of Searching Fields 
        /// </summary>
        /// <param name="rizenumber"></param>
        /// <param name="clearaction"></param>
        /// <param name="stateformclose"></param>
        /// <returns>Task</returns>
        public async Task ClearFilters(string rizenumber = "60%", Action? clearaction = null, bool stateformclose = true)
        {
            if (clearaction != null)
            {
                await Task.Delay(300);
                clearaction.Invoke();
            }

            TxtSearch.Clear();

            //clear all Lov TexBox
            if (ChildLovtextbox != null)
            {
                ChildLovtextbox.Clear();
                ChildLovtextbox.Text = " "; //dumny select
                ChildLovtextbox.SelectedItem = null;
                ChildLovtextbox.Close();
                ChildLovtextbox.SelectedIndex = 0;
            }

            if (ParentLovtextbox != null)
            {
                ParentLovtextbox.Text = " "; //dumny select
                var parent = ParentLovtextbox.OriginalSource.FirstOrDefault(p => p.Id == null);
                ParentLovtextbox.SelectedItem = parent ?? new LovObject { Id = null, Value = "Select" };
                ParentLovtextbox.Close();
                ParentLovtextbox.SelectedIndex = 0;
            }

            //Edit panel Closing
            FormState = !stateformclose ? stateformclose : FormState;
            FormIsOpen(FormState);
            ResizeGrid(rizenumber);
            FormState = !FormState;
        }
        /// <summary>
        /// Update the Total result of the rows and update the labels on the grid 
        /// </summary>
        /// <param name="total">(int) value count of result DB </param>
        public void UpdatePaging(int total = 0)
        {
            InsertStatus("Updating Paging.......", Brushes.Blue);
            int intotal = (total == 0) ? Service.TotalFound : total;
            Page.parsePageData(intotal);
            TxtResults.Text = getResultMessage(intotal, Domain);
            pagingLabels(intotal);
        }
        /// <summary>
        /// Managment of Responsive Windows given the Height and Width of the current windows
        /// </summary>
        /// <param name="gridheight">Size Height = Only Numeric string percent {Size}% or Size}</param>
        public void ResizeGrid(string gridheight, double actualHeight, double actualWidth)
        {
            Dictionary<int, string> columns_width = new Dictionary<int, string>
            {
                { 0, "4%" },
                { 1, "25%" },
                { 2, "18%" },
                { 3, "18%" },
                { 4, "12%" },
                { 5, "12%" },
                { 6, "7%" }
            };


            Util.ResponsiveListViewHeight(GridListView, actualHeight, gridheight);
            Util.ResponsiveGridWidth(GridView, actualWidth - 220, columns_width);
        }
        /// <summary>
        /// Managment of Responsive Windows given the Height and Width of the current windows
        /// </summary>
        /// <param name="gridheight">Size Height = Only Numeric string percent {Size}% or Size}</param>
        /// <param name="actualHeight"> Cuurent Windows Actual Height </param>
        /// <param name="actualWidth"> Cuurent Windows Actual Width </param>
        /// <param name="columns_width"> List of dimension of every Template Column of the Gridview  </param>
        public void ResizeGrid(string gridheight, double actualHeight, double actualWidth, Dictionary<int, string> columns_width)
        {
            StandarHeight = gridheight;
            Util.ResponsiveListViewHeight(GridListView, actualHeight, gridheight);
            Util.ResponsiveGridWidth(GridView, actualWidth - 220, (columns_width ?? ColumnsWidth) ?? Util.CommonSizeGrid);
        }
        /// <summary>
        /// Managment of Responsive Windows given the Height and Width of the current windows
        /// </summary>
        /// <param name="gridheight">Size Height = Only Numeric string percent {Size}% or Size}</param>
        public void ResizeGrid(string? gridheight = null)
        {
            gridheight = CurrentWindow.WindowState == WindowState.Maximized ? StandarHeight : gridheight ?? StandarHeight;
            Util.ResponsiveListViewHeight(GridListView, CurrentWindow.ActualHeight, gridheight);
            Util.ResponsiveGridWidth(GridView, CurrentWindow.ActualWidth, ColumnsWidth ?? Util.CommonSizeGrid);
        }
        /// <summary>
        /// Open or Close the Edit Form
        /// </summary>
        /// <param name="isopen">(bool) True(Open) / False(Close)</param>
        public void FormIsOpen(bool isopen = true)
        {
            if (Form != null)
            {
                Form.Visibility = isopen ? Visibility.Visible : Visibility.Hidden;
                Form.Height = isopen ? Double.NaN : 0;
            }
        }
        /// <summary>
        /// Control and Handle every Exception given the Exception and show the message user
        /// </summary>
        /// <param name="ex">(Exception) Handle(DBException,ExternalException,etc) </param>
        /// <param name="title">(String )MessageBox Title</param>
        /// <param name="showmsg">(bool) True(Show the MessageBox) </param>
        public void CatchExceptionAndMsg(Exception ex, string title = "Fatal Error", bool showmsg = true)
        {
            IEnumerable<StatusItem>? items = null;
            string message = string.Empty;
            string exp_msg = string.Empty;
            bool hasColExp = false;
            ExceptionType exp_type = ExceptionType.None;
            switch (ex)
            {
                case SqlException sqlEx:
                    items = sqlEx.Errors?.Cast<SqlError>().Select(x => new StatusItem
                    {
                        Message = x.Message,
                        Color = BrushesStatus.Error,
                        TraceError = $"Number : {x.Number} " +
                      $", LineNumber : {x.LineNumber} " +
                      $", Server : {x.Server} " +
                      $", Class : {x.Class} " +
                      $", Procedure {x.Procedure}" +
                      $", Source : {x.Source}" +
                      $", State : {x.State}"
                    });
                    exp_msg = CustomException.ExcetionMessage.SystemErrorFailed;
                    exp_type = ExceptionType.DBErrorFailed;
                    hasColExp = true;
                    break;

                case Exception Ex:
                    message = Ex.Message;
                    exp_msg = CustomException.ExcetionMessage.SystemErrorFailed;
                    exp_type = ExceptionType.SystemErrorFailed;
                    break;
                default:
                    message = "Error Process";
                    exp_msg = CustomException.ExcetionMessage.CustomErrorFailed;
                    exp_type = ExceptionType.CustomError;
                    break;
            }

            if (hasColExp)
                InsertStatus(items, exp_type, ex);
            else
            {
                InsertStatus(message, BrushesStatus.Error, false, exp_type, ex);
            }
            Status?.StopProgress();
            if (showmsg)
                MessageBox.Show(CurrentWindow, exp_msg, title, MessageBoxButton.OK, MessageBoxImage.Warning);

        }
        /// <summary>
        /// Insert or Report Message or Error in the statusBar  
        /// </summary>
        /// <param name="message">The string message should fallow this rule of concatenation <time>|<message |...|></param>
        /// <param name="color">Mark the color of the message</param>
        /// <param name="ErrorType">Classify the Message if its a Error Exception, recomended in the catch </param>
        /// <param name="ex">Exception catching </param>
        public void InsertStatus(string message, Brush color, bool isprogress = true, ExceptionType ErrorType = ExceptionType.None, Exception? ex = null,int delay = 500)
        {
            var traceError = string.Empty;
            if (Status != null)
            {
                if (isprogress && ex == null)
                {
                    var msgloading = message.Length > 15 ? message.Substring(0, 15) : message;
                    Status.RunProgress(isloading:isprogress,text: msgloading + "...",delay);
                }

                //Writing Status Error Message in the bar 
                switch (ErrorType)
                {
                    case ExceptionType.DBErrorFailed:
                        message = $"{CustomException.ExcetionMessage.DBErrorFailed} | {message}";
                        break;
                    case ExceptionType.SystemErrorFailed:
                        message = $"{CustomException.ExcetionMessage.SystemErrorFailed} | {message}";
                        break;
                    case ExceptionType.CustomError:
                        message = $"{CustomException.ExcetionMessage.CustomErrorFailed} | {message}";
                        break;
                    case ExceptionType.None:
                        break;
                    default:
                        break;
                }

                //if Error Occur Then store the Trace for reporting
                if (ex != null)
                    traceError = ($"{ex.Message}{ex.Source}{ex.StackTrace}");

                //Set the Status message in the SatatusBar 
                var item = new StatusItem
                {
                    Message = $"{_stscnt++} | {DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ")} | {message}",
                    Color = color,
                    TraceError = traceError
                };
                ObsrverStatus.Insert(0, item);

                //FIFO Status Message
                if (ObsrverStatus.Count > maxCapacity)
                    ObsrverStatus.RemoveAt(ObsrverStatus.Count - 1);

            }
        }
        /// <summary>
        /// Insert or Report a Collectrion Message or Error in the statusBar  
        /// </summary>
        /// <param name="items"></param>
        /// <param name="ErrorType"></param>
        /// <param name="ex"></param>
        public void InsertStatus(IEnumerable<StatusItem> items, ExceptionType ErrorType = ExceptionType.None, Exception? ex = null)
        {
            foreach (var item in items)
                InsertStatus(item.Message, item.Color, false, ErrorType, ex);
        }
        /// <summary>
        /// Insert or Report Message or Error in the statusBar Asynchrony
        /// </summary>
        /// <param name="message">The string message should fallow this rule of concatenation <time>|<message |...|></param>
        /// <param name="color">Mark the color of the message</param>
        /// <param name="ErrorType">Classify the Message if its a Error Exception, recomended in the catch </param>
        /// <param name="ex">Exception catching </param>
        /// <returns>Task</returns>
        public async Task InsertStatusAsync(string message, Brush color, bool isprogress = true, ExceptionType ErrorType = ExceptionType.None, Exception? ex = null)
        {
            InsertStatus(message, color, true, ErrorType, ex,1000);
            await Status.DelayProgress();

        }
        /// <summary>
        /// Insert or Report Message or Error in the statusBar  Database ping Asynchrony 
        /// </summary>
        /// <param name="message">The string message should fallow this rule of concatenation <time>|<message |...|></param>
        /// <param name="color">Mark the color of the message</param>
        /// <param name="ErrorType">Classify the Message if its a Error Exception, recomended in the catch </param>
        /// <param name="ex">Exception catching </param>
        /// <returns>Task</returns>
        public async Task InsertStatusDBAsync(string message, Brush color, ExceptionType ErrorType = ExceptionType.None, Exception? ex = null)
        {
            InsertStatus(message, color, true, ErrorType, ex);
            await Status.DelayDBProgress();
        }
        /// <summary>
        /// Set the Cuurent Item Value int the LovTextBox obj given the Text and id(LovObject.Id) 
        /// </summary>
        /// <param name="obj">(LovTextBox) Object Window Services(Paranet,Child) Window or Any </param>
        /// <param name="searchtext">Mark the Text for the popup</param>
        /// <param name="id">Id to find in Source IEnumeralble(LovObject)</param>
        public void SetLoveValueItem(LovTextBox? obj,string? searchtext = null,int? id = null )
        {
            if (obj != null)
            {
                obj.Text = searchtext ?? " "; //dumny select
                var itemProd = obj.OriginalSource?.FirstOrDefault(p => p.Id == id);
                obj.SelectedItem = itemProd;
                obj.Close();
            }
        }


    }
}
