using Azure;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using SIMA.Domain.Models.Intefaces;
using SIMA.Domain.Models.Objects;
using SIMA.Domain.Models.Params;
using SIMA.Domain.Models.Views;
using SIMA.ExtensionsHelper;
using SIMA.Helper;
using SIMA.Helper.Interfaces;
using SIMA.Infrastructure.Repositories;
using SIMA.Infrastructure.Repositories.Interfaces;
using SIMA.Presentation.Repository;
using SIMA.Presentation.ViewModel;
using SIMA.Presentation.Views;
using SIMA.Templates;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO.Pipelines;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Xml.Linq;
using static System.Net.Mime.MediaTypeNames;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace SIMA.Presentation.Views
{
    /// <summary>
    /// Interaction logic for Wbrand.xaml
    /// </summary>
    public partial class Wstocks : Window
    {
        private readonly ICacheService _cache;

        private WindowServices<StockProduct, StockProductView, StockProductParam> _windowservices;
        private CancellationTokenSource _cts;
        private StockProductView? _rowData;
        private StockViewModel _vm;
        private const string _editHeight = "40%";

        public WindowServices<StockProduct, StockProductView, StockProductParam> WindowServices { get => _windowservices; }

        public Wstocks(ICacheService cache)
        {
            _cache = cache;
            InitializeComponent();
            InitializeWindow();

        }
        public Wstocks(StockProductView rowData, ICacheService cache)
        {
            _cache = cache;
            InitializeComponent();
            InitializeWindow(rowData);
        }

        #region Util
        /// <summary>
        /// Initialize Window
        /// </summary>
        /// <param name="rowData"></param>
        private void InitializeWindow(StockProductView? rowData = null)
        {

            _rowData = rowData;
            _windowservices.EditMode = _rowData != null;
            _windowservices.FromMain = _windowservices.EditMode;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="rowData"></param>
        /// <returns></returns>
        private Action EditControl(StockProductParam? rowData = null)
        {
            return () =>
            {
                if (rowData.idProduct == null)
                {
                    _windowservices.ChildLovtextbox.Clear();
                }

                //Set the Field Values from the grid
                txtIdStock.Text = rowData.idStock.ToString();
                txtStock.Text = rowData.stock.ToString();

                //Set the Field Values for Product
                _windowservices.SetLoveValueItem(_windowservices?.ParentLovtextbox,rowData.products, rowData.idProduct);

                //Set the Field Values for Brand
                _windowservices.SetLoveValueItem(_windowservices?.ChildLovtextbox, rowData.brands, rowData.idBrand);

            };
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private async Task Clear(string windowheight = _editHeight)
        {
            await _windowservices.ClearFilters(windowheight, () =>
            {
                txtIdStock.Text = string.Empty;
                txtStock.Text = string.Empty;
            }, true);

        }
        #endregion
        
        #region Events
        private async void btnsSave_Click(object sender, RoutedEventArgs e)
        {
            int? id = string.IsNullOrEmpty(txtIdStock.Text) ? null : int.Parse(txtIdStock.Text.ToString());
            int? idbrand = _windowservices.ChildLovtextbox?.getSelectedItem().Id;
            int stock = int.Parse(txtStock.Text.isNull("0"));
            try
            {
                _windowservices.InsertStatus("Saving Stock...", BrushesStatus.DBProcess);
                await _windowservices.Status.DelayProgress();
                bool isset = await _windowservices.SetAsync(new StockProduct
                {
                    IdStock = id,
                    IdBrand = idbrand,
                    Stock = stock
                });

                if (isset)
                {
                    _windowservices.InsertStatus("Stock Sucessfully removed", BrushesStatus.DBProcess, false);
                    MessageBox.Show(this, "Stock Sucessfully saved", "Save Stock", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    await Clear();
                    await _windowservices.FillAsync(_windowservices.activeFilters("product"));
                    _windowservices?.Status?.StopProgress();
                }
                else
                    _windowservices.CatchExceptionAndMsg(new Exception("Error Saving Stock"));
            }
            catch (Microsoft.Data.SqlClient.SqlException ex)
            {
                _windowservices.CatchExceptionAndMsg(ex);
            }
            catch (Exception se)
            {
                _windowservices.CatchExceptionAndMsg(se);
            }
        }
        private async void btnRemove_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show(this, "Confirm removing Stock ?", "Remove Stock", MessageBoxButton.YesNoCancel, MessageBoxImage.Warning);
            try
            {
                if (result == MessageBoxResult.Yes)
                {
                    _windowservices.InsertStatus("Deleting Stock...", BrushesStatus.DBProcess);
                    await _windowservices.Status.DelayProgress();
                    Button btn = sender as Button;
                    StockProductView rowData = (StockProductView)btn.DataContext;
                    bool valid = await _windowservices.DeleteAsync(rowData.IdStock) > 0;
                    if (valid)
                    {
                        _windowservices.InsertStatus("Stock Sucessfully removed", BrushesStatus.DBProcess, false);
                        await _windowservices.FillAsync(_windowservices.activeFilters("product"));
                        MessageBox.Show(this, "Stock Succesfully removed", "Remove Stock", MessageBoxButton.OK, MessageBoxImage.Information);
                       _windowservices.Status.StopProgress();
                    }
                    else
                        _windowservices.CatchExceptionAndMsg(new Exception("Error removing the Brand"));
                }
            }
            catch (SqlException ex)
            {
                _windowservices.CatchExceptionAndMsg(ex);
            }
            catch (Exception se)
            {
                _windowservices.CatchExceptionAndMsg(se);
            }
        }
        private async void btnUpdate_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Button btn = sender as Button;
                StockProductView rowData = (StockProductView)btn.DataContext;
                _windowservices.EditMode = true;
                await _windowservices.Edit(_editHeight, EditControl(new StockProductParam
                {
                    idStock = rowData.IdStock,
                    idProduct = rowData.IdProduct,
                    idBrand = rowData.IdBrand,
                    stock = rowData.Stock,
                    products = rowData.Products,
                    brands = rowData.Brands
                }));
                _windowservices.EditMode = false;
            }
            catch (Exception)
            {
                _windowservices.EditMode = false;
            }

        }
        private async void btnNew_Click(object sender, RoutedEventArgs e)
        {
            await Clear();
        }
        private async void btnsClear_Click(object sender, RoutedEventArgs e)
        {
            await Clear(_windowservices.FormState ? _editHeight : _windowservices.StandarHeight);
       }
        private async void btnsClose_Click(object sender, RoutedEventArgs e)
        {
            await Clear();
            this.Close();
        }
        private void PagePrevious_Click(object sender, RoutedEventArgs e)
        {
            _windowservices.NavigationGrid(DIRECTION.previous);
        }
        private void PageNext_Click(object sender, RoutedEventArgs e)
        {
            _windowservices.NavigationGrid(DIRECTION.next);
        }
        private async void PageNavigation_SelectionChanged(object sender, RoutedEventArgs e)
        {
            if (!_windowservices.Isloaded)
            {
                _windowservices.Page.Limit = (int)_windowservices.PageControl.GetSelectedItemsPerPage();
                if (!_windowservices.FromMain)
                {
                    await _windowservices.FillAsync(_windowservices.activeFilters("product"));
                }
                else
                    _windowservices.UpdatePaging();
            }
        }
        private async void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            _windowservices.Page.Limit = (int)_windowservices.PageControl.GetSelectedItemsPerPage();
            await _windowservices.FillAsync(_windowservices.activeFilters("product"));
        }
        private async void parentCombobox_SelectionChanged(object sender, RoutedEventArgs e)
        {
            await _windowservices.FillChildComboboxAsync<Brand, BrandView, BrandParam>
                (sender,typeof(BrandServices), "IdBrand", "Name", "idBrand", "idProduct");
        }
        private async void txtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {

            _cts?.Cancel();
            _cts = new CancellationTokenSource();

            try
            {
                if (!_windowservices.Isloaded && !_windowservices.FromMain)
                {
                    await Task.Delay(300, _cts.Token);
                    await _windowservices.FillAsync(_windowservices.activeFilters("product"));
                }
            }
            catch (TaskCanceledException)
            {
                //Cancel
            }

        }
        private void Window_Closing(object sender, CancelEventArgs e)
        {
            e.Cancel = true;
            this.Visibility = Visibility.Hidden;
        }
        private void Window_Initialized(object sender, EventArgs e)
        {
            IPaging _page = new Paging();
            IConfiguration _config = new Util().CustomConfiguration();
            _vm = new StockViewModel(_page, _config, _cache,false);
            this.DataContext = _vm;
            _vm.ShowErrorFromModel += Vm_ShowErrorFromModel;
            _windowservices = new WindowServices<StockProduct, StockProductView, StockProductParam>(_cache,_config, _page, new StockProductServices(_config,_cache));
            _windowservices.SupressEventComboBox();
        }
        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {

            _windowservices.DataContext = _vm;
            _windowservices.PageControl = pageControl;
            _windowservices.ParentLovtextbox = cmbPropduct;
            _windowservices.ChildLovtextbox = cmbBrand;
            _windowservices.TxtSearch = txtSearch;
            _windowservices.TxtResults = txtResults;
            _windowservices.Form = FormStock;
            _windowservices.FormIsOpen(false);
            _windowservices.Status = statusbox;
            _windowservices.ObsrverStatus = new ObservableCollection<StatusItem>();
            _windowservices.Status.ItemsSource = _windowservices.ObsrverStatus;

            _windowservices.InsertStatus("Initializing Stock Window.......", BrushesStatus.Progress);
            await _windowservices.Status.DelayProgress();
            _windowservices.Page.Offset = (int?)_windowservices.PageControl.GetSelectedItemsPerPage() ?? _windowservices.Page.DefaultOffset;
            _windowservices.InsertStatus("Retriving Stocks.......", BrushesStatus.Progress, false);
            await _windowservices.Page.FillLimitPageVal(_windowservices.PageControl);
            _windowservices.UpdatePaging();
            _windowservices.SupressEventComboBox(false);
            _windowservices.InsertStatus("Window ready.......", BrushesStatus.Progress);
            _windowservices.Status.StopProgress();
        }
        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            _windowservices.CurrentWindow = this;
            _windowservices.GridListView = _windowservices.GridListView ?? gridStocks;
            _windowservices.GridView = _windowservices.GridView ?? gridCellStocks;
            _windowservices.StandarHeight = "50%";
            _windowservices.ResizeGrid();
        }
        private void Vm_ShowErrorFromModel(string mensaje)
        {
            _windowservices.CatchExceptionAndMsg(new Exception(mensaje), "Model Error");
        }
        private async void Window_ContentRendered(object sender, EventArgs e)
        {
            await _windowservices.InsertStatusDBAsync("Stock Window Ready.......", BrushesStatus.Progress);
           if (_rowData != null)
            {
                _windowservices.SupressEventComboBox(false);
                await _windowservices.Edit(_editHeight, EditControl(new StockProductParam
                {
                    idStock = _rowData.IdStock,
                    idProduct = _rowData.IdProduct,
                    idBrand = _rowData.IdBrand,
                    stock = _rowData.Stock,
                    products = _rowData.Products,
                    brands = _rowData.Brands
                }));
                _windowservices.FromMain = false;
                await _windowservices.FillAsync(new StockProductParam { idBrand = _rowData.IdBrand });
            }
            _windowservices.Status.StopProgress();

        }
        #endregion
    }
}

