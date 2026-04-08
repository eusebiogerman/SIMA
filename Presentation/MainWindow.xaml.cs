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

namespace SIMA.Presentation
{
    //public partial class MainWindow : Window, IUtilServices<StockProduct, StockProductParam>, IUtil
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private IContextservices<StockProduct, StockProductView, StockProductParam> _service;
        private IContextservices<Category, Category, CategoryParam> _categoryservices;
        private WindowServices<StockProduct, StockProductView, StockProductParam> _windowservices;

     
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


        private async Task FillGrid()
        {
            var prod = await _service.GetByFilter(_windowservices.activeFilters("Name"));
            _windowservices.Fill(prod);
        }


        #region Events
        private void txtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!_windowservices.Isloaded)
            {
                progress.SetLoadingStateDataBase(async () =>
                {
                   await FillGrid();
                }, _windowservices.Config, true, "Loading Stocks...");
            }

        }
        private async void cmbCategory_SelectionChanged(object sender, RoutedEventArgs e)
        {
            LovObject sendobj = _windowservices.ParentLovtextbox.getSelectedItem(sender);
            if (!_windowservices.Isloaded && sendobj != null)
            {
                var prod = await _service.GetByFilter(new StockProductParam
                {
                    idCategory = sendobj.Id
                });
                _windowservices.Page.resetPage();
                _windowservices.Fill(prod);
            }
        }
        private async void PageNavigation_SelectionChanged(object sender, RoutedEventArgs e)
        {
            if (!_windowservices.Isloaded)
            {
                _windowservices.Page.Limit = (int)_windowservices.PageControl.GetSelectedItemsPerPage();
                await FillGrid();
                _windowservices.UpdatePaging();
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
                _windowStock = new Wstocks();
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
                _wproduct = new Wproduct();
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
                _wbrand = new Wbrand();
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
                _wcategory = new Wcategory();
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

                _windowStock = new Wstocks(rowData);
                _windowStock.Owner = this;
                _windowStock.Activate();
                _windowStock.Show();
            }

        }
        private async void btnRemove_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show(this, "Confirm removing Stock ?", "Remove Stock", MessageBoxButton.YesNoCancel, MessageBoxImage.Warning);
            try
            {
                if (result == MessageBoxResult.Yes)
                {

                    Button btn = sender as Button;
                    StockProductView rowData = (StockProductView)btn.DataContext;
                    bool valid = await _service.Delete(rowData.IdStock) > 0;
                    if (valid)
                    {
                        await FillGrid(); 
                        MessageBox.Show(this, "Stock Succesfully removed", "Remove Stock", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show(this, "Error removing the Brand", "Remove Stock", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }

            }
            catch (Microsoft.Data.SqlClient.SqlException ex)
            {

                MessageBox.Show(this, "DataBase Error Failed", "Fatal Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            catch (Exception)
            {

                MessageBox.Show(this, "System Error Failed", "Fatal Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }

        }
        private async void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            _service = null;
            _service = new StockProductServices();
            _windowservices.GridListView.ItemsSource = null;
            _windowservices.GridListView.Items.Clear();
            await FillGrid();
        }
        private void Window_Initialized(object sender, EventArgs e)
        {
            IPaging _page = new Paging();
            var _util = new Util();
            IConfiguration _config = _util.CustomConfiguration();
            _vm = new MainViewModel(_page, _config);
            this.DataContext = _vm;
            _vm.ShowErrorFromModel += Vm_ShowErrorFromModel;
            _service = new StockProductServices(_config);
            _categoryservices = new CategoryServices(_config);
            _windowservices = new WindowServices<StockProduct, StockProductView, StockProductParam>(_config,_page, _service);
            _windowservices.SupressEventComboBox();
        }
        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            _windowservices.DataContext = _vm;
            _windowservices.PageControl = pageControl;
            _windowservices.ParentLovtextbox = cmbCategory;
            _windowservices.TxtSearch = txtSearch;
            _windowservices.TxtResults = txtResults;
            _windowservices.IngorePredicate = (x) => x.IdProduct != null;

            progress.RunProgress(true, "Init Window....");
            _windowservices.Page.Offset = (int?)_windowservices.PageControl.GetSelectedItemsPerPage() ?? _windowservices.Page.DefaultOffset;
            _windowservices.SupressEventComboBox();
            await _windowservices.Page.FillLimitPageVal(_windowservices.PageControl);
            _windowservices.UpdatePaging();
            _windowservices.SupressEventComboBox(false);

        }
        private void Vm_ShowErrorFromModel(string mensaje)
        {
            MessageBox.Show(this, mensaje, "Model Error", MessageBoxButton.OK, MessageBoxImage.Warning);
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
            progress.StopProgress();
        }


        #endregion


    }

}
