using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using SIMA.Domain.Models.Intefaces;
using SIMA.Domain.Models.Objects;
using SIMA.Domain.Models.Params;
using SIMA.Domain.Models.Views;
using SIMA.Helper;
using SIMA.Helper.Interfaces;
using SIMA.Infrastructure.Repositories.Interfaces;
using SIMA.Presentation.ViewModel;
using SIMA.Templates;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace SIMA.Presentation.Repository
{
    public partial class WindowServices<T, V,P> 
    {

        private IPaging _page;
        private IContextservices<T, V, P> _service;
        

        private ViewModelBase? _dataContext;
        private PageNavigation? _pageControl;
        private LovTextBox? _parentlovtextbox;
        private LovTextBox? _childLovtextbox;
        private TextBox? _txtSearch;
        private TextBlock? _txtResults;
        private Util? _util;
        private ListView? _gridListView;
        private GridView? _gridView;
        private Border _form;
        private Func<V,bool>? _ingorePredicate;
        private Window _currentWindow;
        private Dictionary<int, string> _columnwidth;
        private ListBox? _status;
        private List<StatusItem> statusItems; 

        private readonly IConfiguration _config;
        private bool _isloaded;
        private bool _editmode = false;
        private bool _fromMain = false;
        private bool _formState = false;
        private string _domain;
        private int _stscnt = 0;

        public IPaging Page { get => _page; set => _page = value; }
        public IConfiguration Config => _config;

        public Window CurrentWindow { set => _currentWindow = value; }
        public ViewModelBase? DataContext { get => _dataContext; set => _dataContext = value; }
        public PageNavigation? PageControl { get => _pageControl; set => _pageControl = value; }
        public LovTextBox? ParentLovtextbox { get => _parentlovtextbox; set => _parentlovtextbox = value; }
        public LovTextBox? ChildLovtextbox { get => _childLovtextbox; set => _childLovtextbox = value; }
        public TextBox? TxtSearch { set => _txtSearch = value; }
        public TextBlock? TxtResults { set => _txtResults = value; }
        public ListView? GridListView { get => _gridListView; set => _gridListView = value; }
        public GridView? GridView { get => _gridView; set => _gridView = value; }
        public Border Form { get => _form; set => _form = value; }
        public ListBox? Status { get => _status; set => _status = value; }
        public Type ModelType { private get; set; }
        public Func<V, bool>? IngorePredicate { get => _ingorePredicate; set => _ingorePredicate = value; }
        public Dictionary<int, string> ColumnsWidth { get => _columnwidth; set => _columnwidth = value; }

        public bool Isloaded { get => _isloaded; set => _isloaded = value; }
        public bool EditMode { get => _editmode ;  set => _editmode = value; }
        public bool FromMain { get => _fromMain; set => _fromMain = value; }
        public string Domain { set => _domain = value; }
        public bool FormState { get => _formState; private set => _formState = value; }

        public ObservableCollection<StatusItem> ObsrverStatus { get; set; }

        public WindowServices(IConfiguration config, IPaging page, IContextservices<T, V, P> services)
        {
            statusItems = new List<StatusItem>();
            ObsrverStatus = new ObservableCollection<StatusItem>();
  
            InsertStatus("Initializing Window Services.......", Brushes.Blue);
            _config = config;
            _page = page;
            _util = new Util();
            _service = services;
            _formState = true;
        }

        public void InsertStatus(string message,Brush color) {
            if (_status != null)
            {
                var item = new StatusItem
                {
                    Message = message,
                    Color = color
                };
                ObsrverStatus.Insert(0, item);
                _status.SelectedItem = item;
            }
        }


        #region Utils
        public void SupressEventComboBox(bool val = true)
        {
            _isloaded = val;
            if (_dataContext != null)
                _dataContext.IsSupressed = val;

        }
        /// <summary>
        /// Return and Update the message of the render Stock length given the domain Name
        /// </summary>
        /// <param name="total"></param>
        /// <returns></returns>
        public string getResultMessage(int total, string domain)
        {
            _pageControl.TotalFound = total;
            return $"📊 Show {total} {domain} found";
        }
        /// <summary>
        /// Return and Update the message of the render Stock length 
        /// </summary>
        /// <param name="total"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public string getResultMessage(int total)
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// Update the Paging Labels given the cuurent Offset and Limit Values
        /// </summary>
        /// <param name="total"></param>
        public void pagingLabels(int total)
        {
            if (_page.isvalidPaging())
            {
                _pageControl.TotalFound = total;
                _pageControl.PageNumber = _page.Pagenumber;
            }
        }
        /// <summary>
        /// Control the Paging Previous and Next Page Number,Offset and Limit 
        /// </summary>
        /// <param name="direction"></param>
        public void NavigationGrid(DIRECTION direction)
        {

            if (_page.isvalidPaging())
            {
                _page.movePage(direction);
                _pageControl.ItemsPerPage = _page.Offset;
                // Filter(activeFilters());
            }
        }
        /// <summary>
        /// Restore Initial set of Category and Stock  
        /// </summary>
        public async Task ClearFilters(string rizenumber = "60%",Action? clearaction = null,bool stateformclose = true)
        {
            if (clearaction != null)
            {
                await Task.Delay(300);
                clearaction.Invoke();
            }

            _txtSearch.Clear();

            //clear all Lov TexBox
            if (_childLovtextbox != null)
            {
                _childLovtextbox.Clear();
                _childLovtextbox.Text = " "; //dumny select
                _childLovtextbox.SelectedItem = null;
                _childLovtextbox.Close();
                _childLovtextbox.SelectedIndex = 0;
            }

            if (_parentlovtextbox != null)
            {
                _parentlovtextbox.Text = " "; //dumny select
                var parent = _parentlovtextbox.OriginalSource.FirstOrDefault(p => p.Id == null);
                _parentlovtextbox.SelectedItem = parent ?? new LovObject { Id = null, Value = "Select" } ;
                _parentlovtextbox.Close();
                _parentlovtextbox.SelectedIndex = 0;
            }

            //Edit panel Closing
            _formState = !stateformclose ? stateformclose : _formState;
            FormIsOpen(_formState);
            ResizeGrid(rizenumber);
            _formState = !_formState;
        }
        /// <summary>
        /// Update the Total result of the rows and update the labels on the grid 
        /// </summary>
        /// <param name="total"></param>
        public void UpdatePaging(int total = 0)
        {
            InsertStatus("Updating Paging.......", Brushes.Blue);
            int intotal = (total == 0) ? _service.TotalFound : total;
            _page.parsePageData(intotal);
            _txtResults.Text = getResultMessage(intotal, _domain);
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


            _util.ResponsiveListViewHeight(_gridListView, actualHeight, gridheight);
            _util.ResponsiveGridWidth(_gridView, actualWidth - 220, columns_width);
        }
        /// <summary>
        /// Managment of Responsive Windows given the Height and Width of the current windows
        /// </summary>
        /// <param name="gridheight">Size Height = Only Numeric string percent {Size}% or Size}</param>
        public void ResizeGrid(string gridheight, double actualHeight, double actualWidth, Dictionary<int, string> columns_width)
        {
            _util.ResponsiveListViewHeight(_gridListView, actualHeight, gridheight);
            _util.ResponsiveGridWidth(_gridView, actualWidth - 220, (columns_width ?? _columnwidth) ?? _util.CommonSizeGrid);
        }
        /// <summary>
        /// Managment of Responsive Windows given the Height and Width of the current windows
        /// </summary>
        /// <param name="gridheight"></param>
        /// <exception cref="NotImplementedException"></exception>
        public void ResizeGrid(string gridheight)
        {
            gridheight = _currentWindow.WindowState == WindowState.Maximized ? "60%" : gridheight;
            _util.ResponsiveListViewHeight(_gridListView, _currentWindow.ActualHeight, gridheight);
            _util.ResponsiveGridWidth(_gridView, _currentWindow.ActualWidth, _columnwidth ?? _util.CommonSizeGrid);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="isopen"></param>
        public void FormIsOpen(bool isopen = true) {
            if (_form != null)
            {
                _form.Visibility = isopen ? Visibility.Visible : Visibility.Hidden;
                _form.Height = isopen ? Double.NaN : 0;
            }

        }
        #endregion


        #region Filling Methods
        /// <summary>
        /// Returns the object StockProduct with passing the values of the Active Filter controls 
        /// </summary>
        /// <returns></returns>
        public P activeFilters() 
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// Returns the object StockProduct with passing the values of the Active Filter controls 
        /// </summary>
        /// <returns></returns>
        public P activeFilters(Func<P> actparam)
        {
            return actparam.Invoke();
        }
        /// <summary>
        /// Returns the object StockProduct with passing the values of the Active Filter controls 
        /// </summary>
        /// <returns></returns>
        public P activeFilters(string FilterFieldClass) 
       {
            Type type = typeof(P);
            P obj = (P)Activator.CreateInstance(type);

            var fields = new Dictionary<string, object>
            {
                { "offset", _page.Offset },
                { "limit", _page.Limit }
            };
            fields.Add(FilterFieldClass, _txtResults.Text);

            foreach (var field in fields)
            {
                var prop = type.GetProperty(field.Key);

                if (prop != null && prop.CanWrite)
                {
                    object value = field.Value;

                    if (value != null && !prop.PropertyType.IsInstanceOfType(value))
                    {
                        value = Convert.ChangeType(value, prop.PropertyType);
                    }

                    prop.SetValue(obj, value);
                }
            }
            return obj;

        }
        /// <summary>
        /// Fill the Category ComboBox
        /// </summary>
        public void FillChildCombobox(IEnumerable<LovObject> result)
        {
            try
            {
                _childLovtextbox.ItemsSource = result;
                if (!_editmode && !_fromMain)
                {
                    _childLovtextbox.SelectedIndex = 0;
                }
            }
            catch (Microsoft.Data.SqlClient.SqlException ex)
            {
                _editmode = false;

            }
            catch (Exception)
            {
                _editmode = false;

            }
        }
        /// <summary>
        /// Fill the stock
        /// </summary>
        public void Fill(IEnumerable<V> result)
        {
              _gridListView.ItemsSource = result;
                UpdatePaging();
        }
        /// <summary>
        /// Open the Edit Form for the Stock select in th gridview
        /// </summary>
        /// <param name="param"></param>
        public async Task Edit(string rizenumber = "40%", Action? editraction = null)
        {

            //Edit panel visualization
            FormIsOpen(true);
            ResizeGrid(rizenumber);

            if (_editmode || _fromMain)
            {
                //await Task.Delay(500);
                editraction?.Invoke();
            }
            else
            {
                if (_childLovtextbox != null)
                {
                    _childLovtextbox.Clear();
                    _childLovtextbox.SelectedIndex = 0;
                }

                if (_parentlovtextbox != null)
                    _parentlovtextbox.SelectedIndex = 0;

            }

            _editmode = false;
            _fromMain = false;
        }
        #endregion

    }
}
