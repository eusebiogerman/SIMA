using Microsoft.Extensions.Configuration;
using SIMA.Domain.Models;
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
            FormBrand.Height = 0;
            _page = new Paging();
            _util = new Util();
            _config = _util.CustomConfiguration();
            var Vm = new BrandViewModel(_page, _config);
            this.DataContext = Vm;
            Vm.ShowErrorFromModel += Vm_ShowErrorFromModel;
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
        ///  Restore Initial set of Category and Stock  
        /// </summary>
        public void ClearFilters()
        {
            txtIdBrand.Text = string.Empty;
            txtName.Text = string.Empty;
            txtPrice.Text = string.Empty;
            txtSearch.Text = string.Empty;

            cmbPropduct.Clear();

            //Set the Field Default Values for Category
            cmbCategory.Text = " "; //dumny select
            LovObject? itemCat = cmbCategory.OriginalSource.FirstOrDefault(p => p.Id == null);
            cmbCategory.SelectedItem = itemCat;
            cmbCategory.Commit();
            cmbCategory.Close();

            //Set the Field Default Values for Propducts
            cmbPropduct.Text = " "; //dumny select
            LovObject? itemProd = ((IEnumerable<LovObject>)cmbPropduct.ItemsSource)?.FirstOrDefault(p => p.Id == null);
            cmbPropduct.SelectedItem = itemProd;
            cmbPropduct.Commit();
            cmbPropduct.Close();

            FormBrand.Visibility = Visibility.Hidden;
            FormBrand.Height = 0;
            ResizeGrid("60%");

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
        /// <summary>
        /// Managment of Responsive Windows
        /// </summary>
        /// <param name="gridheight">Size Height = Only Numeric string percent {Size}% or Size}</param>
        public void ResizeGrid(string gridheight)
        {
            _util.ResponsiveListViewHeight(gridBrands, this.ActualHeight, gridheight);
            _util.ResponsiveGridWidth(gridCellBrands, this.ActualWidth, _util.CommonSizeGrid);
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

            try
            {
                progress.SetLoadingStateDataBase(async () =>
                {
                    int? dummy = (id == 0 || !id.HasValue) ? -1 : null;
                    var param = new ProductParam { idCategory = id, idProduct = dummy };
                    IEnumerable<ProductView> cat = await _productervices.GetByFilter(param);
                    cmbPropduct.ItemsSource = cat.Select((p) => new LovObject { Id = p.IdProduct, Value = p.Name });
                    if (!_editmode)
                    {
                        cmbPropduct.SelectedIndex = 0;
                    }
                }, _config, true, "Loading Product list...");
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
        public async void Filter()
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// Filter the GridView given the Stock Category param
        /// </summary>
        /// <param name="param"></param>
        public async Task Filter(BrandParam param)
        {
            try
            {
                progress.SetLoadingStateDataBase(async () =>
                {
                    IEnumerable<BrandView> cat = await _Brandservices.GetByFilter(param);
                    gridBrands.ItemsSource = cat.Where(p => p.IdBrand != null);
                    UpdatePaging();
                }, _config, true, "Loading Brands...");
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
        public async void FilterbyText(BrandParam param)
        {
            try
            {

                IEnumerable<BrandView> cat = await _Brandservices.GetByFilter(param);
                gridBrands.ItemsSource = cat.Where(p => p.IdBrand != null);
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
        /// Open the Edit Form for the Stock select in th gridview
        /// </summary>
        /// <param name="param"></param>
        public async void Edit(BrandParam param)
        {

            FormBrand.Visibility = Visibility.Visible;
            FormBrand.Height = Double.NaN;
            ResizeGrid("40%");

            //Set the Field Values from the grid
            txtIdBrand.Text = param.idBrand.ToString();
            txtName.Text = param.name;
            txtPrice.Text = param.price.ToString();

            if (_editmode)
            {
                //Set the Field Values for Category
                cmbCategory.Text = param.categorys; //dumny select
                var itemCat = cmbCategory.OriginalSource.FirstOrDefault(p => p.Id == param.idCategory);
                cmbCategory.SelectedItem = itemCat;
                cmbCategory.Close();

                //Set the Field Values for Propducts
                cmbPropduct.Text = param.products; //dumny select
                var itemProd = ((IEnumerable<LovObject>)cmbPropduct.ItemsSource)?.FirstOrDefault(p => p.Id == param.idProduct);
                cmbPropduct.SelectedItem = itemProd;
                cmbPropduct.Close();
                _editmode = false;
            }
            else
            {
                cmbPropduct.SelectedIndex = 0;
                cmbPropduct.SelectedIndex = 0;
            }
        }
        #endregion

        #region Events
        private async void btnsSaveBrand_Click(object sender, RoutedEventArgs e)
        {

            int? id = string.IsNullOrEmpty(txtIdBrand.Text) ? null : int.Parse(txtIdBrand.Text);
            int? idprod = cmbPropduct.getSelectedItem().Id;
            string name = txtName.Text;
            decimal price = decimal.Parse(txtPrice.Text);
            try
            {
                bool isset = progress.SetLoadingStateDataBaseResult
                    (async () => await _Brandservices.Set(new Brand
                    {
                        IdBrand = id,
                        IdProduct = idprod,
                        Name = name,
                        Price = price,
                    }), _config, true, "Saving Brands...");


                if (isset)
                {
                    MessageBox.Show(this, "Brand Sucessfully saved", "Save Brand", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    ClearFilters();
                    await Filter(activeFilters());
                }
                else
                {
                    MessageBox.Show(this, "Error Saving Brand", "Save Brand", MessageBoxButton.OK, MessageBoxImage.Exclamation);
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
         
            Edit(new BrandParam());
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
                await Filter(activeFilters());
                UpdatePaging();
            }
        }
        private async void cmbCategory_SelectionChanged(object sender, RoutedEventArgs e)
        {
            if (!_isloaded)
            {
                var sendobj = cmbCategory.getSelectedItem(sender);
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

                    Button btn = sender as Button;
                    BrandView rowData = (BrandView)btn.DataContext;
                    bool valid = await _Brandservices.Delete(rowData.IdBrand) > 0;
                    if (valid)
                    {
                        await Filter(activeFilters());
                        MessageBox.Show(this, "Brand Succesfully removed", "Brand Stock", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show(this, "Error removing the Brand", "Brand Stock", MessageBoxButton.OK, MessageBoxImage.Information);
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
                BrandView rowData = (BrandView)btn.DataContext;
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

            }
            catch (Exception)
            {

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
            this.SupressEventComboBox();
            await _page.FillLimitPageVal(pageControl);
            UpdatePaging();
            this.SupressEventComboBox(false);
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

