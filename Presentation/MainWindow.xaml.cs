using SIMA.Domain.Models;
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

namespace SIMA.Presentation
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, IUtilServices<StockProduct, StockProductParam>, IUtil
    {
        private StockProductServices _stockservices;
        private CategoryServices _categoryservices;
        private Paging _page;
        private Wstocks _windowStock;
        private Wproduct _wproduct;
        private Wcategory _wcategory;
        private Wbrand _wbrand;
        private Util _util;
        private Task _process;
        private bool _isloaded;
        private readonly IConfiguration _config;
        private readonly ViewModelBase _vm;

        private bool _isloadedCat;
        private bool _isloadedStock;

        public MainWindow()
        {
            InitializeComponent();
            _page = new Paging();
            _util = new Util();
            _config = _util.CustomConfiguration();
            _vm = new MainViewModel(_page, _config);
            this.DataContext = _vm;
            _vm.ShowErrorFromModel += Vm_ShowErrorFromModel;

            _stockservices = new StockProductServices(_config);
            _categoryservices = new CategoryServices(_config);

        }

        #region Utils
        public void SupressEventComboBox(bool val = true)
        {
            _isloadedCat = val;
            _isloadedStock = val;
            _isloaded = val;
            if (this.DataContext != null)
                ((MainViewModel)this.DataContext).IsSupressed = val;

        }
        /// <summary>
        /// Return and Update the message of the render Stock length
        /// </summary>
        /// <param name="total"></param>
        /// <returns></returns>
        public string getResultMsgAsync(int total)
        {
            pageControl.TotalFound = total;
            return $"📊 Show {total} products found";
        }
        /// <summary>
        /// Update the Paging Labels given the cuurent Offset and Limit Values
        /// </summary>
        /// <param name="total"></param>
        public void pagingLabels(int total)
        {
            if (_page.isvalidPaging())
            {
                pageControl.TotalFound = total;
                pageControl.PageNumber = _page.Pagenumber;
            }
        }
        /// <summary>
        /// Control the Paging Previous and Next Page Number,Offset and Limit 
        /// </summary>
        /// <param name="direction"></param>
        public async void NavigationGrid(DIRECTION direction)
        {

            if (_page.isvalidPaging())
            {
                _page.movePage(direction);
                pageControl.ItemsPerPage = _page.Offset;
                await Filter(activeFilters());
            }


        }
        /// <summary>
        /// Restore Initial set of Category and Stock  
        /// </summary>
        public void ClearFilters()
        {
            cmbCategory.SelectedIndex = 0;
            txtSearch.Clear();
        }
        /// <summary>
        /// Update the Total result of the rows and update the labels on the grid 
        /// </summary>
        /// <param name="total"></param>
        public async void UpdatePaging(int total = 0)
        {
            int intotal = (total == 0) ? await _stockservices.GetTotalFound(activeFilters()) : total;
            _page.parsePageData(intotal);
            txtResults.Text = getResultMsgAsync(intotal);
            pagingLabels(intotal);
        }
        /// <summary>
        /// Managment of Responsive Windows
        /// </summary>
        /// <param name="gridheight">Size Height = Only Numeric string percent {Size}% or Size}</param>
        public void ResizeGrid(string gridheight)
        {
            Dictionary<int, string> columns_width = new Dictionary<int, string>();
            columns_width.Add(0, "4%");
            columns_width.Add(1, "25%");
            columns_width.Add(2, "18%");
            columns_width.Add(3, "18%");
            columns_width.Add(4, "12%");
            columns_width.Add(5, "12%");
            columns_width.Add(6, "7%");
            _util.ResponsiveListViewHeight(gridProducts, this.ActualHeight, gridheight);
            _util.ResponsiveGridWidth(gridCellProduct, this.ActualWidth - 220, columns_width);
        }
        #endregion

        #region Filling Methods
        public StockProduct Result()
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// Returns the object StockProduct with passing the values of the Active Filter controls 
        /// </summary>
        /// <returns></returns>
        public StockProductParam activeFilters()
        {
            //Category catcmb = ((Category)cmbCategory.SelectedItem);
            LovObject catcmb = ((LovObject)cmbCategory.SelectedItem);
            return new StockProductParam
            {

                idCategory = catcmb != null ? catcmb.Id : null,
                offset = _page.Offset,
                limit = _page.Limit
                //,Name = txtSearch.Text
            };
        }
        /// <summary>
        /// Fill the Category ComboBox
        /// </summary>
        public void FillCombobox(int? id = null)
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// Fill the stock
        /// </summary>
        public async void Fill()
        {
            try
            {
                IEnumerable<StockProductView> prod = await _stockservices.GetViewAll(_page);
                int total = await _stockservices.GetTotalFound(activeFilters());
                gridProducts.ItemsSource = prod;
                UpdatePaging(total);

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
        /// <summary>
        /// Filter the GridView given the activeFilters() : function
        /// </summary>
        public void Filter()
        {
            try
            {
                progress.SetLoadingStateDataBase(async () =>
                {
                    IEnumerable<StockProductView> prod = await _stockservices.GetViewAll(_page);
                    int total = await _stockservices.GetTotalFound(activeFilters());
                    gridProducts.ItemsSource = prod;
                    UpdatePaging(total);
                },_config,true,"Loading stock...");
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
        /// <summary>
        /// Filter the GridView given the Stock Product param
        /// </summary>
        /// <param name="param"></param>
        public async Task Filter(StockProductParam param)
        {
            try
            {

                 progress.SetLoadingStateDataBase( async () =>
                {
                    param.SetPage(_page);
                    IEnumerable<StockProductView> prod = await _stockservices.GetByFilter(param);
                    int total = await _stockservices.GetTotalFound(param);
                    gridProducts.ItemsSource = prod;
                    UpdatePaging(total);
                }, _config, true, "Loading Stock... ");

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
        /// <summary>
        /// Filter the GridView given the Description of product
        /// </summary>
        /// <param name="param"></param>
        public async void FilterbyText(StockProductParam param)
        {
            try
            {
                _page.resetPage();
                await Filter(param);

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
        /// <summary>
        /// Fill the Page Limit Values
        /// </summary>
        public async void FillLimitPageVal()
        {
            IEnumerable<string> lim = await _page.GetLimitPaging();
            pageControl.SetItemsPerPageSource(lim);
            var selected = pageControl.GetSelectedItemsPerPage();
            var cmblimit = selected != null
                ? int.Parse(selected.ToString())
                : _page.DefaultLimit;
            _page.Limit = cmblimit;

        }
        /// <summary>
        /// Open the Edit Form for the Stock select in th gridview
        /// </summary>
        /// <param name="param"></param>
        /// <exception cref="NotImplementedException"></exception>
        public void Edit(StockProductParam param)
        {
            throw new NotImplementedException();
        }
        #endregion

        #region Events
        private void txtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!_isloaded)
            {
                FilterbyText(activeFilters());
            }


        }
        private async void cmbCategory_SelectionChanged(object sender, RoutedEventArgs e)
        {
            var sendobj = cmbCategory.getSelectedItem(sender);
            if (!_isloaded && sendobj != null)
            {
                var param = new StockProductParam
                {
                    idCategory = sendobj.Id
                };
                _page.resetPage();
                await Filter(param);
            }
        }
        private async void PageNavigation_SelectionChanged(object sender, RoutedEventArgs e)
        {
            if (!_isloaded)
            {
                _page.Limit = (int)pageControl.GetSelectedItemsPerPage();
                await Filter(activeFilters());
                UpdatePaging();
            }
        }
        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            ClearFilters();
        }
        private void btnNewStock_Click(object sender, RoutedEventArgs e)
        {
            if (_windowStock == null)
            {
                _windowStock = new Wstocks();
            }
            _windowStock.Owner = this;
            _windowStock.SupressEventComboBox();
            _windowStock.Activate();
            _windowStock.Show();
            _windowStock.SupressEventComboBox(false);


        }
        private void btnProduct_Click(object sender, RoutedEventArgs e)
        {
            if (_wproduct == null)
            {
                _wproduct = new Wproduct();
            }
            _wproduct.Owner = this;
            _wproduct.SupressEventComboBox();
            _wproduct.Activate();
            _wproduct.Show();
            _wproduct.SupressEventComboBox(false);

        }
        private void btnBrand_Click(object sender, RoutedEventArgs e)
        {
            if (_wbrand == null)
            {
                _wbrand = new Wbrand();
            }
            _wbrand.Owner = this;
            _wbrand.SupressEventComboBox();
            _wbrand.Activate();
            _wbrand.Show();
            _wbrand.SupressEventComboBox(false);


        }
        private void btnCategory_Click(object sender, RoutedEventArgs e)
        {
            if (_wcategory == null)
            {
                _wcategory = new Wcategory();
            }
            _wcategory.Owner = this;
            _wcategory.SupressEventComboBox();
            _wcategory.Activate();
            _wcategory.Show();
            _wcategory.SupressEventComboBox(false);

        }
        private void PagePrevious_Click(object sender, RoutedEventArgs e)
        {
            NavigationGrid(DIRECTION.previous);
        }
        private void PageNext_Click(object sender, RoutedEventArgs e)
        {
            NavigationGrid(DIRECTION.next);

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
            MessageBoxResult result = MessageBox.Show(this, "Confirm remove Stock ?", "Remove Stock", MessageBoxButton.YesNoCancel, MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                Button btn = sender as Button;
                StockProductView rowData = (StockProductView)btn.DataContext;
                bool valid = await _stockservices.Delete(rowData.IdStock) > 0;
                if (valid)
                {
                    Filter();
                    MessageBox.Show(this, "Stock Succesfully removed", "Remove Stock", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }

        }
        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
                _stockservices = null;
                _stockservices = new StockProductServices();
                gridProducts.ItemsSource = null;
                gridProducts.Items.Clear();
                FilterbyText(activeFilters());

        }
        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            _page.Offset = (int?)pageControl.GetSelectedItemsPerPage() ?? _page.DefaultOffset;
            this.SupressEventComboBox();
            await _page.FillLimitPageVal(pageControl);
            UpdatePaging();
            this.SupressEventComboBox(false);

        }
        private void Window_Initialized(object sender, EventArgs e)
        {
            this.SupressEventComboBox();

        }
        private void Vm_ShowErrorFromModel(string mensaje)
        {
            MessageBox.Show(this, mensaje, "Model Error", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            ResizeGrid("60%");
        }
        #endregion










    }

}
