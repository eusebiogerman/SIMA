using SIMA.Infrastructure.Repositories;
using SIMA.ExtensionsHelper;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using SIMA.Presentation.Views;
using SIMA.Helper;
using SIMA.Presentation.ViewModel;
using System.Linq;
using System.Windows.Media;
using Microsoft.Extensions.Configuration;
using System.Xml.Linq;
using SIMA.Templates;
using System.Reflection.PortableExecutable;
using System.Windows.Media.Media3D;
using SIMA.Infrastructure.Repositories.Interfaces;
using System.Data.Common;
using SIMA.Domain.Models.Objects;
using SIMA.Domain.Models.Views;
using SIMA.Domain.Models.Params;
using SIMA.Domain.Models.Intefaces;
using SIMA.Presentation.Repository;
using System.Collections.ObjectModel;
using Microsoft.Data.SqlClient;
using System.Threading;

namespace SIMA.Presentation
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private  ICacheService _cache;
        private WindowServices<StockProduct, StockProductView, StockProductParam> _windowservices;
        private CancellationTokenSource _cts;
        private Wstocks _windowStock;
        private Wproduct _wproduct;
        private Wcategory _wcategory;
        private Wbrand _wbrand;
        private ViewModelBase _vm;
        private readonly Dictionary<int, string> columns_width;

        public MainWindow()
        {
            InitializeComponent();
            columns_width = new Dictionary<int, string>
            {
                { 0, "4%" },
                { 1, "25%" },
                { 2, "18%" },
                { 3, "18%" },
                { 4, "12%" },
                { 5, "12%" },
                { 6, "7%" }
            };

        }


        #region Events
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
                        await _windowservices.FillAsync(_windowservices.activeFilters("Name"));
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
        private void btnUpdate_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            StockProductView rowData = (StockProductView)btn.DataContext;

            if (rowData != null)
            {
                if (_windowStock != null)
                {
                    if (_windowStock.IsEnabled)
                    {
                        _windowStock.Close();
                        _windowStock = null;
                    }
                }

                _windowStock = new Wstocks(rowData, _cache);
                _windowStock.Owner = this;
                _windowStock.Activate();
                _windowStock.Show();
            }

        }
        private async void txtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            _cts?.Cancel();
            _cts = new CancellationTokenSource();
            try
            {
                if (!_windowservices.Isloaded)
                {
                    await Task.Delay(300, _cts.Token);
                    await _windowservices.FillAsync(_windowservices.activeFilters("Name"));
                }
            }
            catch (TaskCanceledException)
            {
                //Cancel
            }

        }
        private async void cmbCategory_SelectionChanged(object sender, RoutedEventArgs e)
        {
            LovObject sendobj = _windowservices.ParentLovtextbox.getSelectedItem(sender);
            if (!_windowservices.Isloaded && sendobj != null)
            {
               await _windowservices.FillAsync(new StockProductParam{
                    idCategory = sendobj.Id
                });

            }
        }
        private async void PageNavigation_SelectionChanged(object sender, RoutedEventArgs e)
        {
            if (!_windowservices.Isloaded)
            {
                _windowservices.Page.Limit = (int)_windowservices.PageControl.GetSelectedItemsPerPage();
                await _windowservices.FillAsync(_windowservices.activeFilters("Name"));
            }
        }
        private async void btnClear_Click(object sender, RoutedEventArgs e)
        {
            await _windowservices.ClearFilters();
        }
        private void btnNewStock_Click(object sender, RoutedEventArgs e)
        {
            if (_windowStock == null)
            {
                _windowStock = new Wstocks(_cache);
            }
            _windowStock.Owner = this;
            _windowStock.WindowServices.SupressEventComboBox();
            _windowStock.Activate();
            _windowStock.Show();
          


        }
        private void btnProduct_Click(object sender, RoutedEventArgs e)
        {
            if (_wproduct == null)
            {
                _wproduct = new Wproduct(_cache);
            }
            _wproduct.Owner = this;
            _wproduct.WindowServices.SupressEventComboBox();
            _wproduct.Activate();
            _wproduct.Show();

        }
        private void btnBrand_Click(object sender, RoutedEventArgs e)
        {
            if (_wbrand == null)
            {
                _wbrand = new Wbrand(_cache);
            }
            _wbrand.Owner = this;
            _wbrand.WindowServices.SupressEventComboBox();
            _wbrand.Activate();
            _wbrand.Show();


        }
        private void btnCategory_Click(object sender, RoutedEventArgs e)
        {
            if (_wcategory == null)
            {
                _wcategory = new Wcategory(_cache);
            }
            _wcategory.Owner = this;
            _wcategory.WindowServices.SupressEventComboBox();
            _wcategory.Activate();
            _wcategory.Show();

        }
        private void PagePrevious_Click(object sender, RoutedEventArgs e)
        {
            _windowservices.NavigationGrid(DIRECTION.previous);
        }
        private void PageNext_Click(object sender, RoutedEventArgs e)
        {
            _windowservices.NavigationGrid(DIRECTION.next);

        }
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();

        }
        private async void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            _windowservices.GridListView.ItemsSource = null;
            _windowservices.GridListView.Items.Clear();
            await _windowservices.FillAsync(_windowservices.activeFilters("Name"));
        }
        private void Window_Initialized(object sender, EventArgs e)
        {
            _cache = new MemoryCacheService();
            IPaging _page = new Paging();
            var _util = new Util();
            IConfiguration _config = _util.CustomConfiguration();
            _vm = new MainViewModel(_page, _config, _cache);
            this.DataContext = _vm;
            _vm.ShowErrorFromModel += Vm_ShowErrorFromModel;
            _windowservices = new WindowServices<StockProduct, StockProductView, StockProductParam>(_config,_page, new StockProductServices(_config, _cache));
            _windowservices.SupressEventComboBox();
        }
        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            _windowservices.DataContext = _vm;
            _windowservices.PageControl = pageControl;
            _windowservices.ParentLovtextbox = cmbCategory;
            _windowservices.TxtSearch = txtSearch;
            _windowservices.TxtResults = txtResults;
            _windowservices.Status = statusbox;
            _windowservices.ObsrverStatus = new ObservableCollection<StatusItem>();
            _windowservices.Status.ItemsSource = _windowservices.ObsrverStatus;

            await _windowservices.InsertStatusAsync("Initializing SIMA Window.......", BrushesStatus.Progress);
            _windowservices.Page.Offset = (int?)_windowservices.PageControl.GetSelectedItemsPerPage() ?? _windowservices.Page.DefaultOffset;
            _windowservices.InsertStatus("Retriving Stocks.......", BrushesStatus.Progress, false);
            await _windowservices.Page.FillLimitPageVal(_windowservices.PageControl);
            _windowservices.UpdatePaging();
            _windowservices.SupressEventComboBox(false);
            _windowservices.InsertStatus("Window ready.......", BrushesStatus.Progress);
            _windowservices.Status.StopProgress();
        }
        private void Vm_ShowErrorFromModel(string mensaje)
        {
            _windowservices.CatchExceptionAndMsg(new Exception(mensaje), "Model Error");
        }
        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            _windowservices.CurrentWindow = this;
            _windowservices.GridListView = _windowservices.GridListView ?? gridProducts;
            _windowservices.GridView = _windowservices.GridView ?? gridCellProduct;
            _windowservices.ResizeGrid("60%",this.ActualHeight,this.ActualWidth, columns_width);
        }
        private void Window_ContentRendered(object sender, EventArgs e)
        {
            _windowservices.InsertStatus("SIMA Window Ready.......", BrushesStatus.Progress,false);
            _windowservices.Status.StopProgress();
        }
        #endregion

    }

}
