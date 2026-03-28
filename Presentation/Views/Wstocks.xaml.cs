using Microsoft.Extensions.Configuration;
using SIMA.Domain.Models;
using SIMA.ExtensionsHelper;
using SIMA.Helper;
using SIMA.Infrastructure.Repositories;
using SIMA.Presentation.ViewModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
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
using static System.Net.Mime.MediaTypeNames;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace SIMA.Presentation.Views
{
    /// <summary>
    /// Interaction logic for Wbrand.xaml
    /// </summary>
    public partial class Wstocks : Window, IUtilServices<StockProduct, StockProductParam>, IUtil
    {

        private StockProductServices _stockservices;
        private BrandServices _brandsrervices;
        private Paging _page;
        private Util _util;
        private CancellationTokenSource _cts;
        private IConfiguration _config;
        private bool _isloaded;
        private bool _editmode = false;
        private StockProductView _rowData;

        public Wstocks()
        {
            InitializeComponent();
            InitializeWstock();

        }

        public Wstocks(StockProductView rowData)
        {
            InitializeComponent();
            InitializeWstock(rowData);
        }

        #region Util

        private void InitializeWstock(StockProductView rowData = null) {

            FormBrand.Visibility = Visibility.Hidden;
            _page = new Paging();
            _util = new Util();
            _config = _util.CustomConfiguration();
            this.DataContext = new StockViewModel(_page, _config);
            _stockservices = new StockProductServices(_config);
            _brandsrervices = new BrandServices(_config);
        } 

        /// <summary>
        /// 
        /// </summary>
        /// <param name="val"></param>
        public void SupressEventComboBox(bool val = true)
        {
            _isloaded = val;
            if (this.DataContext != null)
                ((StockViewModel)this.DataContext).IsSupressed = val;

        }
        /// <summary>
        /// Return and Update the message of the render Category result
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
            throw new NotImplementedException();
        }
        /// <summary>
        ///  Restore Initial set of Category and Stock  
        /// </summary>
        public void ClearFilters()
        {
            txtIdStock.Text = string.Empty;
            txtStock.Text = "0";
            txtSearch.Text = string.Empty;
            cmbPropduct.SelectedIndex = 0;
            FormBrand.Visibility = Visibility.Hidden;
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
        #endregion

        #region Filling Methods
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public StockProduct Result()
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// Returns the object CategoryParam with passing values of the Active Filter controls 
        /// </summary>
        /// <returns></returns>
        public StockProductParam activeFilters()
        {
            int? idprod   = cmbPropduct.SelectedItem != null ? ((ProductView)cmbPropduct.SelectedItem).IdProduct : null ;
            int? idbrand  = cmbBrand.SelectedItem != null ? ((Brand)cmbBrand.SelectedItem).IdBrand : null;
            int? didstock = !string.IsNullOrEmpty(txtIdStock.Text) ? int.Parse(txtIdStock.Text.ToString()) : null ;
            int? dstock   = !string.IsNullOrEmpty(txtStock.Text)  ? int.Parse(txtStock.Text.ToString()) : null;
            return new StockProductParam
            {
                idStock = didstock, 
                idProduct = idprod,
                idBrand = idbrand,
                stock = dstock,
                offset = 0,
                limit = 10
            };

        }
        /// <summary>
        /// Fill the Category ComboBox
        /// </summary>
        public async void FillCombobox(int? id = null)
        {


            wloading.Visibility = Visibility.Hidden;
            try
            {
                _util.Loading_spimmer(wloading, true, 1500);
                int? cmbid = cmbPropduct.SelectedItem != null ? ((ProductView)cmbPropduct.SelectedItem).IdProduct : 0;
                int? idprod = id.HasValue ? id : cmbid;  
                int? dummy = (idprod == 0 || !idprod.HasValue) ? -1 : null;
                var param = new BrandParam { idProduct = idprod,idBrand = dummy };
                IEnumerable<BrandView> cat = await _brandsrervices.GetByFilter(param);
                cmbBrand.ItemsSource = cat;
                if (!_editmode)
                {
                    cmbBrand.SelectedIndex = 0;
                }
                _util.Loading_spimmer(wloading, false);
            }
            catch (Microsoft.Data.SqlClient.SqlException ex)
            {
                _editmode = false;
                _util.Loading_spimmer(wloading, false);
            }
            catch (Exception)
            {
                _editmode = false;
                _util.Loading_spimmer(wloading, false);
            }

        }
        /// <summary>
        /// Fill the Gridview
        /// </summary>
        /// <exception cref="NotImplementedException"></exception>
        public void Fill()
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// Filter the GridView given the activeFilters() : function
        /// </summary>
        public async void Filter()
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// Filter the GridView given the Stock Category param
        /// </summary>
        /// <param name="param"></param>
        public async void Filter(StockProductParam param)
        {
            try
            {
                _util.Loading_spimmer(wloading, true, 1000);
                IEnumerable<StockProductView> cat = await _stockservices.GetByFilter(param);
                gridBrands.ItemsSource = cat.Where(p => p.IdBrand != null);
                UpdatePaging();
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
        /// Filter the GridView given the Search text description
        /// </summary>
        /// <param name="param"></param>
        public async void FilterbyText(StockProductParam param)
        {
            try
            {
                _util.Loading_spimmer(wloading, true, 1000);
                IEnumerable<StockProductView> cat = await _stockservices.GetByFilter(param);
                gridBrands.ItemsSource = cat.Where(p => p.IdBrand != null);
                UpdatePaging();
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
        public void Edit(StockProductParam param)
        {

            FormBrand.Visibility = Visibility.Visible;
            txtIdStock.Text = param.idStock.ToString();
            txtStock.Text = param.stock.ToString();
            var itemProd = cmbPropduct.Items.Cast<ProductView>().FirstOrDefault(p => p.IdProduct == param.idProduct);
            cmbPropduct.SelectedItem = itemProd;
            FillCombobox(itemProd.IdProduct);
            var itemBrand = cmbBrand.Items.Cast<BrandView>().FirstOrDefault(p => p.IdBrand == param.idBrand);
            cmbPropduct.SelectedItem = itemBrand;

       }
        #endregion


        #region Events
        private async void btnsSaveBrand_Click(object sender, RoutedEventArgs e)
        {
            _util.Loading_spimmer(wloading, true, 1000);
            int? id = string.IsNullOrEmpty(txtIdStock.Text) ? null : int.Parse(txtIdStock.Text.ToString());
            int? idbrand = ((BrandView)cmbBrand.SelectedItem).IdBrand;
            try
            {

                bool isset = await _stockservices.Set(new StockProduct
                {
                    IdStock = id,
                    IdBrand = idbrand,
                    Stock = int.Parse(txtStock.Text.ToString())
                });

                if (isset)
                {
                    MessageBox.Show(this, "Stock Sucessfully saved", "Save Stock", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    ClearFilters();
                    Filter(activeFilters());
                }
                else
                {
                    MessageBox.Show(this, "Error Saving Stock", "Save Stock", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                }
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
        private void btnNewBrand_Click(object sender, RoutedEventArgs e)
        {
            Edit(new StockProductParam());
        }
        private void btnsClear_Click(object sender, RoutedEventArgs e)
        {
            ClearFilters();
        }
        private void btnsClose_Click(object sender, RoutedEventArgs e)
        {
            ClearFilters();
            this.Close();
        }
        private void PagePrevious_Click(object sender, RoutedEventArgs e)
        {
            NavigationGrid(Paging.DIRECTION.previous);
        }
        private void PageNext_Click(object sender, RoutedEventArgs e)
        {
            NavigationGrid(Paging.DIRECTION.next);
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
        private void cmbPropduct_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!_isloaded && !_editmode)
            {
                FillCombobox();
            }
        }
        private async void txtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {

            _cts?.Cancel();
            _cts = new CancellationTokenSource();

            try
            {
                if (!_isloaded)
                {
                    await Task.Delay(300, _cts.Token);
                    FilterbyText(activeFilters());
                }
            }
            catch (TaskCanceledException)
            {
                //Cancel
            }

        }
        private async void btnRemove_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show(this, "Confirm remove Stock ?", "Remove Stock", MessageBoxButton.YesNoCancel, MessageBoxImage.Warning);
            try
            {
                if (result == MessageBoxResult.Yes)
                {
                    _util.Loading_spimmer(wloading, true, 1000);
                    Button btn = sender as Button;
                    StockProductView rowData = (StockProductView)btn.DataContext;
                    bool valid = await _stockservices.Delete(rowData.IdStock) > 0;
                    if (valid)
                    {
                        Filter(activeFilters());
                        MessageBox.Show(this, "Stock Succesfully removed", "Remove Stock", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show(this, "Error removing the Brand", "Remove Stock", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    _util.Loading_spimmer(wloading, false, 100);

                }
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
        private void btnUpdate_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Button btn = sender as Button;
                StockProductView rowData = (StockProductView)btn.DataContext;
                _util.Loading_spimmer(wloading, true, 1000);
                _editmode = true;
                Edit(new StockProductParam
                {
                    idStock = rowData.IdStock,
                    idProduct = rowData.IdProduct,
                    idBrand = rowData.IdBrand,
                    stock = rowData.Stock,
                });
                _editmode = false;
                _util.Loading_spimmer(wloading, false);

            }
            catch (Exception)
            {
                _editmode = false;
                _util.Loading_spimmer(wloading, false);
            }




        }
        private void Window_Initialized(object sender, EventArgs e)
        {
            this.SupressEventComboBox();
        }
        private void Window_Closing(object sender, CancelEventArgs e)
        {
            e.Cancel = true;
            this.Visibility = Visibility.Hidden;
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            _util.Loading_spimmer(wloading, true);
            _page.Offset = (int?)pageControl.GetSelectedItemsPerPage() ?? _page.DefaulOffset;
            UpdatePaging();
            this.SupressEventComboBox();
            FillLimitPageVal();

            if (_rowData != null)
            {
                _editmode = true;
                Edit(new StockProductParam
                {
                    idStock = _rowData.IdStock,
                    idProduct = _rowData.IdProduct,
                    idBrand = _rowData.IdBrand,
                    stock = _rowData.Stock,
                });

            }
            _util.Loading_spimmer(wloading, false);
            this.SupressEventComboBox(false);

        }


        #endregion


    }
}

