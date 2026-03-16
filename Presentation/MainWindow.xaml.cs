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

namespace SIMA.Presentation
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private StockProductServices _stockservices;
        private CategoryServices _categoryservices;   
        private bool _isloaded;
        private Paging _page;
        private Wstocks _windowStock;
        private Wproduct _wproduct;
        private Util _util;

        public MainWindow()
        {
            InitializeComponent();
            _page = new Paging();
            this.DataContext =  new MainViewModel(_page);
            _util = new Util();
            _util.Loading_spimmer(wloading, true);
            _isloaded = true;
            _stockservices = new StockProductServices();
            _categoryservices = new CategoryServices();
             FillLimitPageVal();
            // Fillcat();
            //_isloaded = false;
            _util.Loading_spimmer(wloading, false);

        }

        #region Utils
        /// <summary>
        /// Returns the object StockProduct with passing the values of the Active Filter controls 
        /// </summary>
        /// <returns></returns>
        private StockProduct activeFilters()
        {
            return new StockProduct
            {
                Category = cmbCategory.Text.getDefaultEmptyCat()
               ,
                Name = txtSearch.Text
            };
        }
        /// <summary>
        /// Return and Update the message of the render Stock length
        /// </summary>
        /// <param name="total"></param>
        /// <returns></returns>
        private string getResultMsgAsync(int total)
        {
            txtTotal.Text = total.ToString();
            return $"📊 Show {total} products found";
        }
        /// <summary>
        /// Update the Paging Labels given the cuurent Offset and Limit Values
        /// </summary>
        /// <param name="total"></param>
        private void pagingLabels(int total) {
            if (_page.isvalidPaging())
            {
                txtTotal.Text = total.ToString();
                txtPaging.Text = $"Page {_page.Pagenumber.ToString()} de {_page.TotalPage.ToString()}";
            }
        }
        /// <summary>
        /// Control the Paging Previous and Next Page Number,Offset and Limit 
        /// </summary>
        /// <param name="direction"></param>
        private void NavigationGrid(Paging.DIRECTION direction )
        {
       
            if (_page.isvalidPaging())
            {
                _page.movePage(direction);
               // txtoffset.Text = _page.Offset.ToString();
                FilterStock(activeFilters());
            }


        }
        /// <summary>
        /// Restore Initial set of Category and Stock  
        /// </summary>
        private void ClearFilters()
        {
            Fillcat();
            txtSearch.Clear();
        }
        private async void UpdatePaging(int total = 0)
        {
            int intotal = (total == 0) ? await _stockservices.GetTotalFound(activeFilters()) : total;
            _page.parsePageData(intotal);
            txtResults.Text = getResultMsgAsync(intotal);
            pagingLabels(intotal);
        }

        #endregion

        #region Filling Methods
        /// <summary>
        /// Fill the Page Limit Values
        /// </summary>
        private async void FillLimitPageVal() {
            IEnumerable<string> lim = await _page.GetLimitPaging();
            cmbLimitPage.ItemsSource = lim;
            //txtoffset.Text = cmbLimitPage.SelectedItem.ToString();
            _page.Limit = int.Parse(cmbLimitPage.SelectedItem.ToString());

        }
        /// <summary>
        /// Fill the Category ComboBox
        /// </summary>
        private async void Fillcat()
        {
            IEnumerable<Category> cat = await _categoryservices.GetAll(_page);
            cmbCategory.ItemsSource =cat;
            _isloaded = false;
            cmbCategory.SelectedIndex = 0;
        }
        /// <summary>
        /// Filter the GridView given the activeFilters() : function
        /// </summary>
        private async void FilterStock()
        {
            IEnumerable<StockProduct> prod = await _stockservices.GetAll(_page);
            int total = await _stockservices.GetTotalFound(activeFilters());
            gridProducts.ItemsSource = prod;
            UpdatePaging(total);
        }
        /// <summary>
        /// Filter the GridView given the Stock Product param
        /// </summary>
        /// <param name="param"></param>
        private async void FilterStock(StockProduct param)
        {
            IEnumerable<StockProduct> prod = await _stockservices.GetByFilter(param, _page);
            int total = await _stockservices.GetTotalFound(param);
            gridProducts.ItemsSource = prod;
            UpdatePaging(total);
        }
        /// <summary>
        /// Filter the GridView given the Description of product
        /// </summary>
        private void FilterbyText()
        {
            _page.resetPage();
            StockProduct param = activeFilters();
            FilterStock(param);
        }

        #endregion

        #region Events
        private void txtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!_isloaded)
            {
                FilterbyText();
            }


        }
        private void cmbCategory_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!_isloaded && e.AddedItems.Count > 0)
            {
                var param = new StockProduct {
                    Category = ((Category)e.AddedItems[0]).Name.getDefaultEmptyCat()
                   ,Name     = txtSearch.Text  
                };
                _page.resetPage();
                FilterStock(param);

            }
        }
        private void cmbLimitPage_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!_isloaded)
            {
                _page.Offset = int.Parse(cmbLimitPage.SelectedItem.ToString());
                _page.Limit = int.Parse(e.AddedItems[0].ToString());
                StockProduct param = activeFilters();
                FilterStock(param);
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
        private void btnNuevoProducto_Click(object sender, RoutedEventArgs e)
        {
            // Programmatically change what the frame is showing

             if (_wproduct == null)
             {
                 _wproduct = new Wproduct();
             }
             _wproduct.Owner = this;
             _windowStock.SupressEventComboBox();
             _wproduct.Activate();
             _wproduct.Show();
             _windowStock.SupressEventComboBox(false);

        }
        private void btnPrevious_Click(object sender, RoutedEventArgs e)
        {
            NavigationGrid(Paging.DIRECTION.previous);
        }
        private void btnNext_Click(object sender, RoutedEventArgs e)
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
            StockProduct rowData = (StockProduct)btn.DataContext;

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
                _windowStock.EditMode = true;
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
                StockProduct rowData = (StockProduct)btn.DataContext;
                bool valid = await _stockservices.Delete(rowData.IdStock) > 0;
                if (valid)
                {
                    FilterStock();
                    MessageBox.Show(this, "Stock Succesfully removed", "Remove Stock", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }

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
                FilterbyText();
                _util.Loading_spimmer(wloading, false);
            }
            catch (Exception ex)
            {
                _util.Loading_spimmer(wloading, false);
            }

        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            _page.Offset = int.Parse(cmbLimitPage.SelectedItem != null ? cmbLimitPage.SelectedItem.ToString() : _page.DefaulOffset.ToString());
            UpdatePaging();
            _isloaded = false;
        }
        #endregion

    }

}
