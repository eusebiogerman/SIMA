using Microsoft.Extensions.Configuration;
using SIMA.Domain.Models;
using SIMA.ExtensionsHelper;
using SIMA.Helper;
using SIMA.Infrastructure.Repositories;
using SIMA.Presentation.ViewModel;
using SIMA.Templates;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
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
        private StockProductView? _rowData;
        private StockViewModel _vm;
        private IConfiguration _config;
        private bool _isloaded;
        private bool _editmode = false;

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
        /// <summary>
        /// Initialize Window
        /// </summary>
        /// <param name="rowData"></param>
        private void InitializeWstock(StockProductView? rowData = null)
        {

            FormStock.Visibility = Visibility.Hidden;
            FormStock.Height = 0;

            _page = new Paging();
            _util = new Util();
            _config = _util.CustomConfiguration();
            _vm = new StockViewModel(_page, _config);
            this.DataContext = _vm;
            _vm.ShowErrorFromModel += Vm_ShowErrorFromModel;
            _stockservices = new StockProductServices(_config);
            _brandsrervices = new BrandServices(_config);
            _rowData = rowData;   
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

            //Edit panel Closing
            FormStock.Visibility = Visibility.Hidden;
            FormStock.Height = 0;
            ResizeGrid("60%");

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
            gridheight = this.WindowState == WindowState.Maximized ? "60%" : gridheight;
            _util.ResponsiveListViewHeight(gridStocks, this.ActualHeight, gridheight);
            _util.ResponsiveGridWidth(gridCellStocks, this.ActualWidth, _util.CommonSizeGrid);
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
            int? idprod = cmbPropduct.getSelectedItem().Id;
            int? idbrand = cmbBrand.getSelectedItem().Id;
            int? didstock = !string.IsNullOrEmpty(txtIdStock.Text) ? int.Parse(txtIdStock.Text.ToString()) : null;
            int? dstock = !string.IsNullOrEmpty(txtStock.Text) ? int.Parse(txtStock.Text.ToString()) : null;
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
            try
            {

                int? dummy = (id == 0 || !id.HasValue) ? -1 : null;
                var param = new BrandParam { idProduct = id, idBrand = dummy };
                IEnumerable<BrandView> cat = await _brandsrervices.GetByFilter(param);
                cmbBrand.ItemsSource = cat.Select(p => new LovObject { Id = p.IdBrand, Value = p.Name });
                if (!_editmode)
                {
                    cmbBrand.SelectedIndex = 0;
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
        /// Fill the Gridview
        /// </summary>
        /// <exception cref="NotImplementedException"></exception>
        public void Fill()
        {
            progress.SetLoadingStateDataBase(async () =>
            {
                IEnumerable<StockProductView> cat = await _stockservices.GetViewAll(_page);
                gridStocks.ItemsSource = cat.Where(p => p.IdBrand != null);
                UpdatePaging();
            }, _config, true, "Loading Stock...");
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
                progress.SetLoadingStateDataBase(async () =>
                {
                    IEnumerable<StockProductView> cat = await _stockservices.GetByFilter(param);
                    gridStocks.ItemsSource = cat.Where(p => p.IdBrand != null);
                    UpdatePaging();
                }, _config, true, "Loading Stock...");

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
        /// Filter the GridView given the Search text description
        /// </summary>
        /// <param name="param"></param>
        public async void FilterbyText(StockProductParam param)
        {
            try
            {

                IEnumerable<StockProductView> cat = await _stockservices.GetByFilter(param);
                gridStocks.ItemsSource = cat.Where(p => p.IdBrand != null);
                UpdatePaging();


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
                : _page.DefaulLimit;
            _page.Limit = cmblimit;
        }
        /// <summary>
        /// Open the Edit Form for the Stock select in th gridview
        /// </summary>
        /// <param name="param"></param>
        public void Edit(StockProductParam param)
        {

            //Edit panel visualization
            FormStock.Visibility = Visibility.Visible;
            FormStock.Height = Double.NaN;
            ResizeGrid("40%");

            //Set the Field Values from the grid
            txtIdStock.Text = param.idStock.ToString();
            txtStock.Text = param.stock.ToString();

            if (_editmode)
            {
                //Set the Field Values for Product
                cmbPropduct.Text = param.products; //dumny select
                var itemProd = cmbPropduct.OriginalSource.FirstOrDefault(p => p.Id == param.idProduct);
                cmbPropduct.SelectedItem = itemProd;
                cmbPropduct.Close();

                //Set the Field Values for Brand
                Thread.Sleep(500);
                cmbBrand.Text = param.brands; //dumny select
                var itemBrand = (cmbBrand.OriginalSource)?.FirstOrDefault(p => p.Id == param.idBrand);
                if (itemBrand != null)
                {
                    cmbBrand.SelectedItem = itemBrand;
                }
                cmbBrand.Close();
            }
            else {
                cmbPropduct.SelectedIndex = 0;
                cmbBrand.SelectedIndex = 0;
            }
       
            _editmode = false;
        }
        #endregion

        #region Events
        private async void btnsSaveStock_Click(object sender, RoutedEventArgs e)
        {
            int? id = string.IsNullOrEmpty(txtIdStock.Text) ? null : int.Parse(txtIdStock.Text.ToString());
            int? idbrand = cmbBrand.getSelectedItem().Id;
            int stock = int.Parse(txtStock.Text.ToString());
            try
            {
                bool isset = progress.SetLoadingStateDataBaseResult
                    (async () => await _stockservices.Set(new StockProduct
                    {
                        IdStock = id,
                        IdBrand = idbrand,
                        Stock = stock
                    }), _config, true, "Saving Stock...");

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
        private async void cmbPropduct_SelectionChanged(object sender, RoutedEventArgs e)
        {
            if (!_isloaded && !_editmode)
            {
                var sendobj = cmbPropduct.getSelectedItem(sender);
                FillCombobox(sendobj.Id);
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
        private async void btnUpdate_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Button btn = sender as Button;
                StockProductView rowData = (StockProductView)btn.DataContext;

                _editmode = true;
                Edit(new StockProductParam
                {
                    idStock = rowData.IdStock,
                    idProduct = rowData.IdProduct,
                    idBrand = rowData.IdBrand,
                    stock = rowData.Stock,
                    products = rowData.Products,
                    brands = rowData.Brands
                });
                _editmode = false;
            }
            catch (Exception)
            {
                _editmode = false;

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

            _page.Offset = (int?)pageControl.GetSelectedItemsPerPage() ?? _page.DefaulOffset;
             FillLimitPageVal();
            UpdatePaging();
            this.SupressEventComboBox(false);

        }
        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            ResizeGrid("60%");
        }
        private void Vm_ShowErrorFromModel(string mensaje)
        {
            MessageBox.Show(this, mensaje, "Model Error", MessageBoxButton.OK, MessageBoxImage.Warning);
        }



        #endregion



        private void Window_ContentRendered(object sender, EventArgs e)
        {
            if (_rowData != null)
            {
                _editmode = true;
                Edit(new StockProductParam
                {
                    idStock = _rowData.IdStock,
                    idProduct = _rowData.IdProduct,
                    idBrand = _rowData.IdBrand,
                    stock = _rowData.Stock,
                    products = _rowData.Products,
                    brands = _rowData.Brands
                });
            }

        }
    }
}

