using Microsoft.Extensions.Configuration;
using SIMA.Domain.Models.Intefaces;
using SIMA.Domain.Models.Objects;
using SIMA.Domain.Models.Params;
using SIMA.Domain.Models.Views;
using SIMA.ExtensionsHelper;
using SIMA.Helper;
using SIMA.Infrastructure.Repositories;
using SIMA.Infrastructure.Repositories.Interfaces;
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

        private IContextservices<StockProduct, StockProductView, StockProductParam> _service;
        private IContextservices<Brand, BrandView, BrandParam> _brandsrervices;
        private IPaging _page;
        private IConfiguration _config;

        private Util _util;
        private CancellationTokenSource _cts;
        private StockProductView? _rowData;
        private StockViewModel _vm;
        private bool _isloaded;
        private bool _editmode = false;
        private bool _fromMain = false;

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
            _service = new StockProductServices(_config);
            _brandsrervices = new BrandServices(_config);
            _rowData = rowData;
            _editmode = _rowData != null;
            _fromMain = _editmode;
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
        public string getResultMessage(int total)
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
        public void NavigationGrid(DIRECTION direction)
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

            cmbBrand.Clear();

            //Set the Field Default Values for Product
            cmbPropduct.Text = " ";
            LovObject? itemProd = cmbPropduct.OriginalSource.FirstOrDefault(p => p.Id == null);
            cmbPropduct.SelectedItem = itemProd;
            cmbPropduct.Commit();
            cmbPropduct.Close();

            //Set the Field Default Values for Brands
            cmbBrand.Text = " "; //dumny select
            LovObject? itemBrand = ((IEnumerable<LovObject>)cmbBrand.ItemsSource)?.FirstOrDefault(p => p.Id == null);
            cmbBrand.SelectedItem = itemBrand;
            cmbBrand.Commit();
            cmbBrand.Close();

            //Edit panel Closing
            FormStock.Visibility = Visibility.Hidden;
            FormStock.Height = 0;
            ResizeGrid("60%");


        }
        /// <summary>
        /// Update the Total result of the rows and update the labels on the grid 
        /// </summary>
        /// <param name="total"></param>
        public void UpdatePaging(int total = 0)
        {
            int intotal = (total == 0) ? _service.TotalFound : total;
            _page.parsePageData(intotal);
            txtResults.Text = getResultMessage(intotal);
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
            return new StockProductParam
            {
                brands = txtSearch.Text,
                offset = 0,
                limit = 10
            };

        }
        /// <summary>
        /// Fill the Category ComboBox
        /// </summary>
        public void FillCombobox(int? id = null)
        {
            try
            {
                progress.SetLoadingStateDataBase(async () =>
                {
                    int? dummy = (id == 0 || !id.HasValue) ? -1 : null;
                    var param = new BrandParam { idProduct = id, idBrand = dummy };
                    IEnumerable<BrandView> cat = await _brandsrervices.GetByFilter(param);
                    cmbBrand.ItemsSource = cat.Select(p => new LovObject { Id = p.IdBrand, Value = p.Name });
                    if (!_editmode || !_fromMain)
                    {
                        cmbBrand.SelectedIndex = 0;
                    }
                }, _config, true, "Loading Brand list...");
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
            throw new NotImplementedException();
        }
        /// <summary>
        /// Filter the GridView given the activeFilters() : function
        /// </summary>
        public void Filter()
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// Filter the GridView given the Stock Category param
        /// </summary>
        /// <param name="param"></param>
        public void Filter(StockProductParam param)
        {
            try
            {
                progress.SetLoadingStateDataBase(async () =>
                {
                    IEnumerable<StockProductView> cat = await _service.GetByFilter(param);
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
        public void FilterbyText(StockProductParam param)
        {
            try
            {
                progress.SetLoadingStateDataBase(async () =>
                {
                    IEnumerable<StockProductView> cat = await _service.GetByFilter(param);
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

            if (_editmode || _fromMain)
            {
                //Set the Field Values for Product
                cmbPropduct.Text = param.products; //dumny select
                var itemProd = cmbPropduct.OriginalSource.FirstOrDefault(p => p.Id == param.idProduct);
                cmbPropduct.SelectedItem = itemProd;
                cmbPropduct.Close();

                //Set the Field Values for Brand
                cmbBrand.Text = param.brands; //dumny select
                var itemBrand = ((IEnumerable<LovObject>)cmbBrand.ItemsSource)?.FirstOrDefault(p => p.Id == param.idBrand);
                cmbBrand.SelectedItem = itemBrand;
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
                progress.RunProgress(true, "Saving Stock...");
                await progress.DelayProgress();
                bool isset = await _service.Set(new StockProduct
                {
                    IdStock = id,
                    IdBrand = idbrand,
                    Stock = stock
                });
                progress.StopProgress();

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
                progress.StopProgress();
                MessageBox.Show(this, "DataBase Error Failed", "Fatal Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            catch (Exception)
            {
                progress.StopProgress();
                MessageBox.Show(this, "System Error Failed", "Fatal Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
        private void btnNewBrand_Click(object sender, RoutedEventArgs e)
        {
            ClearFilters();
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
            NavigationGrid(DIRECTION.previous);
        }
        private void PageNext_Click(object sender, RoutedEventArgs e)
        {
            NavigationGrid(DIRECTION.next);
        }
        private async void PageNavigation_SelectionChanged(object sender, RoutedEventArgs e)
        {
            if (!_isloaded)
            {
                _page.Limit = (int)pageControl.GetSelectedItemsPerPage();
                if(!_fromMain) {
                    Filter(activeFilters());
                }
                 UpdatePaging();
            }
        }
        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            _page.Limit = (int)pageControl.GetSelectedItemsPerPage();
            Filter(activeFilters());
            UpdatePaging();
        }
        private void cmbPropduct_SelectionChanged(object sender, RoutedEventArgs e)
        {
            if (!_isloaded && !_editmode)
            {
                var sendobj = cmbPropduct.getSelectedItem(sender);
                FillCombobox(sendobj.Id);
            }
        }
        private void txtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {

            _cts?.Cancel();
            _cts = new CancellationTokenSource();

            try
            {
                if (!_isloaded && !_fromMain)
                {
                    Task.Delay(300, _cts.Token);
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
        private void btnUpdate_Click(object sender, RoutedEventArgs e)
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
        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {

            _page.Offset = (int?)pageControl.GetSelectedItemsPerPage() ?? _page.DefaultOffset;
            await _page.FillLimitPageVal(pageControl);
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
        private void Window_ContentRendered(object sender, EventArgs e)
        {
            if (_rowData != null)
            {
                this.SupressEventComboBox(false);
                Edit(new StockProductParam
                {
                    idStock = _rowData.IdStock,
                    idProduct = _rowData.IdProduct,
                    idBrand = _rowData.IdBrand,
                    stock = _rowData.Stock,
                    products = _rowData.Products,
                    brands = _rowData.Brands
                });
                _fromMain = false;
                Filter(new StockProductParam { idBrand = _rowData.IdBrand });
            }

        }
        #endregion
    }
}

