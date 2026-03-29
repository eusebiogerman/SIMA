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
    public partial class Wbrand : Window, IUtilServices<Brand, BrandParam>, IUtil
    {

        private BrandServices _Brandservices;
        private ProductServices _productervices;
        private Paging _page;
        private Util _util;
        private CancellationTokenSource _cts;
        private readonly IConfiguration _config;
        private bool _isloaded;
        private bool _isNewStock = false;
        private bool _editmode = false;

        public Wbrand()
        {

            InitializeComponent();

            FormBrand.Visibility = Visibility.Hidden;
            _page = new Paging();
            _util = new Util();
            _config = _util.CustomConfiguration();
            this.DataContext = new BrandViewModel(_page, _config);
            _Brandservices = new BrandServices(_config);
            _productervices = new ProductServices(_config);
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
                ((BrandViewModel)this.DataContext).IsSupressed = val;

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

            if (_page.isvalidPaging())
            {
                _page.movePage(direction);
                pageControl.ItemsPerPage = _page.Offset;
                Filter(activeFilters());
            }
        }
        /// <summary>
        ///  Restore Initial set of Category and Stock  
        /// </summary>
        public void ClearFilters()
        {
            txtIdBrand.Text = string.Empty;
            txtName.Text = string.Empty;
            txtPrice.Text = string.Empty;
            txtSearch.Text = string.Empty;
            cmbCategory.SelectedIndex = 0;
            FormBrand.Visibility = Visibility.Hidden;
        }
        /// <summary>
        /// Update the Total result of the rows and update the labels on the grid 
        /// </summary>
        /// <param name="total"></param>
        public async void UpdatePaging(int total = 0)
        {
            int intotal = (total == 0) ? await _Brandservices.GetTotalFound(activeFilters()) : total;
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
        public Brand Result()
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// Returns the object CategoryParam with passing values of the Active Filter controls 
        /// </summary>
        /// <returns></returns>
        public BrandParam activeFilters()
        {
            return new BrandParam
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
            wloading.Visibility = Visibility.Hidden;
            try
            {
                _util.Loading_spimmer(wloading, true, 1500);
                int? idcat = ((Category)cmbCategory.SelectedItem).IdCategory;
                int? dummy = (idcat == 0 || !idcat.HasValue) ? -1 : null;
                var param = new ProductParam { idCategory = idcat, idProduct = dummy};
                IEnumerable<ProductView> cat = await _productervices.GetByFilter(param);
                cmbPropduct.ItemsSource = cat;
                if (!_editmode)
                {
                    cmbPropduct.SelectedIndex = 0;
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
        public async void Filter(BrandParam param)
        {
            try
            {
                _util.Loading_spimmer(wloading, true, 1000);
                IEnumerable<BrandView> cat = await _Brandservices.GetByFilter(param);
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
        public async void FilterbyText(BrandParam param)
        {
            try
            {
                _util.Loading_spimmer(wloading, true, 1000);
                IEnumerable<BrandView> cat = await _Brandservices.GetByFilter(param);
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
        public void Edit(BrandParam param)
        {

            FormBrand.Visibility = Visibility.Visible;
            txtIdBrand.Text = param.idBrand.ToString();
            txtName.Text = param.name;
            var itemCat = cmbCategory.Items.Cast<Category>().FirstOrDefault(p => p.IdCategory == param.idCategory);
            cmbCategory.SelectedItem = itemCat;
            txtPrice.Text = param.price.ToString();
            var itemProd = cmbPropduct.Items.Cast<ProductView>().FirstOrDefault(p => p.IdProduct == param.idProduct);
            cmbPropduct.SelectedItem = itemProd;
            _editmode = false;
        }
        #endregion


        #region Events
        private async void btnsSaveBrand_Click(object sender, RoutedEventArgs e)
        {
            _util.Loading_spimmer(wloading, true, 1000);
            int? id = string.IsNullOrEmpty(txtIdBrand.Text) ? null : int.Parse(txtIdBrand.Text);
            int? idprod = ((ProductView)cmbPropduct.SelectedItem).IdProduct;
            try
            {

                bool isset = await _Brandservices.Set(new Brand
                {
                    IdBrand = id,
                    IdProduct = idprod,
                    Name = txtName.Text,
                    Price = decimal.Parse(txtPrice.Text)
                });

                if (isset)
                {
                    MessageBox.Show(this, "Brand Sucessfully saved", "Save Brand", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    ClearFilters();
                    Filter(activeFilters());
                }
                else
                {
                    MessageBox.Show(this, "Error Saving Brand", "Save Brand", MessageBoxButton.OK, MessageBoxImage.Exclamation);
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
            Edit(new BrandParam { idBrand = null, name = null, idCategory = null, categorys = null });
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
        private async void cmbCategory_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!_isloaded)
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
                // Ignorar: se canceló porque el usuario siguió escribiendo
            }

        }
        private async void btnRemove_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show(this, "Confirm remove Brand ?", "Remove Brand", MessageBoxButton.YesNoCancel, MessageBoxImage.Warning);
            try
            {
                if (result == MessageBoxResult.Yes)
                {
                    _util.Loading_spimmer(wloading, true, 1000);
                    Button btn = sender as Button;
                    BrandView rowData = (BrandView)btn.DataContext;
                    bool valid = await _Brandservices.Delete(rowData.IdBrand) > 0;
                    if (valid)
                    {
                        Filter(activeFilters());
                        MessageBox.Show(this, "Brand Succesfully removed", "Brand Stock", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show(this, "Error removing the Brand", "Brand Stock", MessageBoxButton.OK, MessageBoxImage.Information);
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
                BrandView rowData = (BrandView)btn.DataContext;
                _util.Loading_spimmer(wloading, true, 1000);
                _editmode = true;
                Edit(new BrandParam
                {
                    idBrand = rowData.IdBrand,
                    idCategory = rowData.IdCategory,
                    idProduct = rowData.IdProduct,
                    name = rowData.Name,
                    categorys = rowData.Categorys,
                    products = rowData.Products,
                    price = rowData.Price
                });
                _util.Loading_spimmer(wloading, false);

            }
            catch (Exception)
            {
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
            this.SupressEventComboBox();
            FillLimitPageVal();
            UpdatePaging();
            _util.Loading_spimmer(wloading, false);
            this.SupressEventComboBox(false);


        }

        #endregion


    }
}

