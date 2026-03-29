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
    /// Interaction logic for Wproduct.xaml
    /// </summary>
    public partial class Wproduct : Window, IUtilServices<Product, ProductParam>, IUtil
    {

        private ProductServices _productservices;
        private CategoryServices _categoryervices;
        private StockProductServices _StockProductservices;
        private Product _currentProduct;
        private Paging _page;
        private Util _util;
        private CancellationTokenSource _cts;
        private readonly IConfiguration _config;
        private bool _isloaded;
        private bool _isNewStock = false;

        public Wproduct()
        {

            InitializeComponent();
            FormProduct.Visibility = Visibility.Hidden;
            _page = new Paging();
            _util = new Util();
            _config = _util.CustomConfiguration();
            var Vm = new ProductViewModel(_page, _config);
            this.DataContext = Vm;
            Vm.ShowErrorFromModel += Vm_ShowErrorFromModel;
            _productservices = new ProductServices(_config);
            _categoryervices = new CategoryServices(_config);
        }

        #region Util
        /// <summary>
        /// 
        /// </summary>
        /// <param name="val"></param>
        public void SupressEventComboBox(bool val = true)
        {
            _isloaded = val;
            if (this.DataContext != null)
                ((ProductViewModel)this.DataContext).IsSupressed = val;
       
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
            txtIdProduct.Text = string.Empty;
            txtName.Text = string.Empty;
            txtPrice.Text = string.Empty;
            txtSearch.Text = string.Empty;
            cmbCategory.SelectedIndex = 0;
            FormProduct.Visibility = Visibility.Hidden;
        }
        /// <summary>
        /// Update the Total result of the rows and update the labels on the grid 
        /// </summary>
        /// <param name="total"></param>
        public async void UpdatePaging(int total = 0)
        {
            int intotal = (total == 0) ? await _productservices.GetTotalFound(activeFilters()) : total;
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
        public Product Result()
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// Returns the object CategoryParam with passing values of the Active Filter controls 
        /// </summary>
        /// <returns></returns>
        public ProductParam activeFilters()
        {
            return new ProductParam
            {
                name = txtSearch.Text,
                offset = _page.Offset,
                limit = _page.Limit
            };

        }
        /// <summary>
        /// Fill the Category ComboBox
        /// </summary>
        public async void FillCombobox(int? id = null)
        {
            throw new NotImplementedException();
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
        public async void Filter(ProductParam param)
        {
            try
            {
                _util.Loading_spimmer(wloading, true, 1000);
                IEnumerable<ProductView> cat = await _productservices.GetByFilter(param);
                gridProducts.ItemsSource = cat.Where(p => p.IdProduct != null);
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
        public async void FilterbyText(ProductParam param)
        {
            try
            {
                _util.Loading_spimmer(wloading, true, 1000);
                IEnumerable<ProductView> cat = await _productservices.GetByFilter(param);
                gridProducts.ItemsSource = cat.Where(p => p.IdProduct != null);
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
        public void Edit(ProductParam param)
        {
            FormProduct.Visibility = Visibility.Visible;
            txtIdProduct.Text = param.idProduct.ToString();
            txtName.Text = param.name;
            cmbCategory.Text = param.categorys; //var dummy  
            var itemCat = cmbCategory.OrignalSource.First(p => p.Id == param.idCategory);
            cmbCategory.SelectedItem = itemCat;
            txtPrice.Text = param.price.ToString(); 
        }
        #endregion

        #region Events
        private async void btnsSaveProduct_Click(object sender, RoutedEventArgs e)
        {
            _util.Loading_spimmer(wloading, true, 1000);
            int? id = string.IsNullOrEmpty(txtIdProduct.Text) ? null : int.Parse(txtIdProduct.Text);
            int? idcat = ((LovObject)cmbCategory.SelectedItem).Id;
            bool isset = await _productservices.Set(new Product
            {
                IdProduct = id,
                IdCategory = idcat,
                Name = txtName.Text,
                Price = decimal.Parse(txtPrice.Text) 
            });

            try
            {
                if (isset)
                {
                    MessageBox.Show(this, "Product Sucessfully saved", "Save Product", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    ClearFilters();
                    Filter(activeFilters());
                }
                else
                {
                    MessageBox.Show(this, "Error Saving Product", "Save Product", MessageBoxButton.OK, MessageBoxImage.Exclamation);
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
        private void btnNewProduct_Click(object sender, RoutedEventArgs e)
        {
            Edit(new ProductParam { idProduct = null, name = null, idCategory = null,categorys = null});
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
                // Ignorar: se canceló porque el usuario siguió escribiendo
            }

        }
        private async void btnRemove_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show(this, "Confirm remove Product ?", "Remove Product", MessageBoxButton.YesNoCancel, MessageBoxImage.Warning);
            try
            {
                if (result == MessageBoxResult.Yes)
                {
                    _util.Loading_spimmer(wloading, true, 1000);
                    Button btn = sender as Button;
                    ProductView rowData = (ProductView)btn.DataContext;
                    bool valid = await _productservices.Delete(rowData.IdProduct) > 0;
                    if (valid)
                    {
                        Filter(activeFilters());
                        MessageBox.Show(this, "Product Succesfully removed", "Product Stock", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show(this, "Error removing the Product", "Product Stock", MessageBoxButton.OK, MessageBoxImage.Information);
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
            Button btn = sender as Button;
            ProductView rowData = (ProductView)btn.DataContext;
            Edit(new ProductParam 
            { idProduct = rowData.IdProduct
            , idCategory = rowData.IdCategory
            , name = rowData.Name 
            , categorys = rowData.Categorys
            , price = rowData.Price
            });

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
            this.SupressEventComboBox();
            FillLimitPageVal();
            UpdatePaging();
            _util.Loading_spimmer(wloading, false);
            this.SupressEventComboBox(false);
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            cmbCategory.SelectedIndex = 0;

        }
        #endregion


    }
}
