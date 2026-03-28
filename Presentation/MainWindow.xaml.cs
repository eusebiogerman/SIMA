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

namespace SIMA.Presentation
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, IUtilServices<StockProduct, StockProductParam>,IUtil
    {
        private StockProductServices _stockservices;
        private CategoryServices _categoryservices;
        private bool _isloaded;
        private Paging _page;
        private Wstocks _windowStock;
        private Wproduct _wproduct;
        private Wcategory _wcategory;
        private Wbrand _wbrand;
        private Util _util;
        private bool _isloadedCat;
        private bool _isloadedStock;
        private readonly IConfiguration _config;

        public MainWindow()
        {
            InitializeComponent();
            _page = new Paging();
            _util = new Util();
            _config = _util.CustomConfiguration();
            var Vm =  new MainViewModel(_page, _config);
            this.DataContext = (MainViewModel)Vm;
            Vm.ShowErrorFromModel += Vm_ShowErrorFromModel;
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
        public void NavigationGrid(Paging.DIRECTION direction)
        {

            if (_page.isvalidPaging())
            {
                _page.movePage(direction);
                pageControl.ItemsPerPage = _page.Offset;
                Filter(activeFilters());
            }


        }
        /// <summary>
        /// Restore Initial set of Category and Stock  
        /// </summary>
        public void ClearFilters()
        {
            FillCombobox();
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
            _util.Loading_spimmer(wloading, false);
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
            Category catcmb = ((Category)cmbCategory.SelectedItem);
            return new StockProductParam
            {

                idCategory = catcmb != null ? catcmb.IdCategory : null,
                offset = _page.Offset,
                limit = _page.Limit
                //,Name = txtSearch.Text
            };
        }
        /// <summary>
        /// Fill the Category ComboBox
        /// </summary>
        public async void FillCombobox(int? id = null)
        {
            try
            {
                _util.Loading_spimmer(wloading, true,200);
                IEnumerable<Category> cat = await _categoryservices.GetAll(_page);
                cmbCategory.ItemsSource = cat;
                _isloaded = false;
                cmbCategory.SelectedIndex = 0;
            }
            catch (Microsoft.Data.SqlClient.SqlException ex) {

                _util.Loading_spimmer(wloading, false);
                MessageBox.Show(this, "DataBase Error Failed", "Fatal Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            catch (Exception)
            {
                _util.Loading_spimmer(wloading, false);
                MessageBox.Show(this, "System Error Failed", "Fatal Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
        /// <summary>
        /// Fill the stock
        /// </summary>
        public async void Fill()
        {
            try
            {
                _util.Loading_spimmer(wloading, true, 1000);
                IEnumerable<StockProductView> prod = await _stockservices.GetViewAll(_page);
                int total = await _stockservices.GetTotalFound(activeFilters());
                gridProducts.ItemsSource = prod;
                UpdatePaging(total);
                _util.Loading_spimmer(wloading, false);

            }
            catch (Microsoft.Data.SqlClient.SqlException ex)
            {
                _util.Loading_spimmer(wloading, false);
                MessageBox.Show(this, "DataBase Error Failed", "Fatal Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            catch (Exception)
            {
                _util.Loading_spimmer(wloading, false);
                MessageBox.Show(this, "System Error Failed", "Fatal Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }

        }
        /// <summary>
        /// Filter the GridView given the activeFilters() : function
        /// </summary>
        public async void Filter()
        {
            try
            {
                _util.Loading_spimmer(wloading, true, 1000);
                IEnumerable<StockProductView> prod = await _stockservices.GetViewAll(_page);
                int total = await _stockservices.GetTotalFound(activeFilters());
                gridProducts.ItemsSource = prod;
                UpdatePaging(total);
                _util.Loading_spimmer(wloading, false);
            }
            catch (Microsoft.Data.SqlClient.SqlException ex)
            {
                _util.Loading_spimmer(wloading, false);
                MessageBox.Show(this, "DataBase Error Failed", "Fatal Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            catch (Exception)
            {
                _util.Loading_spimmer(wloading, false);
                MessageBox.Show(this, "System Error Failed", "Fatal Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
        /// <summary>
        /// Filter the GridView given the Stock Product param
        /// </summary>
        /// <param name="param"></param>
        public async void Filter(StockProductParam param)
        {
            try
            {
                _util.Loading_spimmer(wloading, true, 1000);
                IEnumerable<StockProductView> prod = await _stockservices.GetViewAll(_page);
                int total = await _stockservices.GetTotalFound(param);
                gridProducts.ItemsSource = prod;
                UpdatePaging(total);
                _util.Loading_spimmer(wloading, false);

            }
            catch (Microsoft.Data.SqlClient.SqlException ex)
            {
                _util.Loading_spimmer(wloading, false);
                MessageBox.Show(this, "DataBase Error Failed", "Fatal Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            catch (Exception)
            {
                _util.Loading_spimmer(wloading, false);
                MessageBox.Show(this, "System Error Failed", "Fatal Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
        /// <summary>
        /// Filter the GridView given the Description of product
        /// </summary>
        /// <param name="param"></param>
        public void FilterbyText(StockProductParam param)
        {
            try
            {
                _util.Loading_spimmer(wloading, true, 1000);
                _page.resetPage();
                Filter(param);
                _util.Loading_spimmer(wloading, false);

            }
            catch (Microsoft.Data.SqlClient.SqlException ex)
            {
                _util.Loading_spimmer(wloading, false);
                MessageBox.Show(this, "DataBase Error Failed", "Fatal Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            catch (Exception)
            {
                _util.Loading_spimmer(wloading, false);
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
                : _page.DefaulLimit;
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
        /// Incomplited <<<<<<<<<<<<<<-------*******
        private void cmbCategory_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!_isloaded && e.AddedItems.Count > 0)
            {
                var param = new StockProductParam
                {
                    idCategory = ((Category)e.AddedItems[0]).IdCategory
                };
                _page.resetPage();
                Filter(param);

            }
        }
        private void PageNavigation_SelectionChanged(object sender, RoutedEventArgs e)
        {
            if (!_isloaded)
            {
                _page.Limit = (int)pageControl.GetSelectedItemsPerPage();
                Filter(activeFilters());
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
            NavigationGrid(Paging.DIRECTION.previous);
        }
        private void PageNext_Click(object sender, RoutedEventArgs e)
        {
            NavigationGrid(Paging.DIRECTION.next);

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

            _util.Loading_spimmer(wloading, true,1000);
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
            _util.Loading_spimmer(wloading, false,100);

        }
        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            _util.Loading_spimmer(wloading, true);
            try
            {
                _stockservices = null;
                _stockservices = new StockProductServices();
                gridProducts.ItemsSource = null;
                gridProducts.Items.Clear();
                FilterbyText(activeFilters());
                _util.Loading_spimmer(wloading, false);
            }
            catch (Exception ex)
            {
                _util.Loading_spimmer(wloading, false);
            }

        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            _util.Loading_spimmer(wloading,true);
            _page.Offset =  (int?)pageControl.GetSelectedItemsPerPage() ?? _page.DefaulOffset;
            this.SupressEventComboBox();
            FillLimitPageVal();
            UpdatePaging();
            _util.Loading_spimmer(wloading, false);
            this.SupressEventComboBox(false);

        }
        private void Window_Initialized(object sender, EventArgs e)
        {
            this.SupressEventComboBox();
       
        }
        private void Vm_ShowErrorFromModel(string mensaje)
        {
            _util.Loading_spimmer(wloading, false);
            MessageBox.Show(this, mensaje, "Model Error", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
        #endregion


    }

}
