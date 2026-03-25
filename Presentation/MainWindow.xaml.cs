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
<<<<<<< Updated upstream
=======
using SIMA.Presentation.ViewModel;
using System.Linq;
using System.Windows.Media;
using Microsoft.Extensions.Configuration;
>>>>>>> Stashed changes

namespace SIMA.Presentation
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, IUtilHandle 
    {
        private StockProductServices _stockservices;
        private CategoryServices _categoryservices;   
        private bool _isloaded;
        private Paging _page;
        private Wstocks _windowStock;
        private Wproduct _wproduct;
        private Util _util;
        private bool _isloadedCat;
        private bool _isloadedStock;
        private readonly IConfiguration _config;

        public MainWindow()
        {
            InitializeComponent();
<<<<<<< Updated upstream
=======
            _page = new Paging();
>>>>>>> Stashed changes
            _util = new Util();
            _config = _util.CustomConfiguration();
            this.DataContext =  new MainViewModel(_page, _config);

            this.SupressEventComboBox();
            _util.Loading_spimmer(wloading, true);
            _stockservices = new StockProductServices(_config);
            _categoryservices = new CategoryServices();
<<<<<<< Updated upstream
             _page = new Paging();
            txtoffset.Text = _page.Offset.ToString();
            FillLimitPageVal();
            Fillcat();
            FillStock();
=======
             FillLimitPageVal();
>>>>>>> Stashed changes
            _util.Loading_spimmer(wloading, false);
            this.SupressEventComboBox(false);

        }

        #region Utils
<<<<<<< Updated upstream
        private enum DIRECTION { 
            previous = 1,
            next = 2 
        }
=======
        public void SupressEventComboBox(bool val = true)
        {
            _isloadedCat = val;
            _isloadedStock = val;
            _isloaded = val;
            ((MainViewModel)this.DataContext).IsSupressed = val;

        }
        /// <summary>
        /// Returns the object StockProduct with passing the values of the Active Filter controls 
        /// </summary>
        /// <returns></returns>
>>>>>>> Stashed changes
        private StockProduct activeFilters()
        {
            return new StockProduct
            {
                Category = cmbCategory.Text.getDefaultEmptyCat()
               ,
                Name = txtSearch.Text
            };
        }
        private string getResultMsgAsync(int total)
        {
            txtTotal.Text = total.ToString();
            return $"📊 Show {total} products found";
        }
        private bool isvalidPaging()
        {
            var total = int.Parse(txtTotal.Text);
            var ofsset = int.Parse(txtoffset.Text);
            return (ofsset > 0 && total > ofsset); 
        }
        private void pagingLabels(int total) {
            if (isvalidPaging())
            {
                txtTotal.Text = total.ToString();
                txtPaging.Text = $"Page {_page.Pagenumber.ToString()} de {_page.TotalPage.ToString()}";
            }
        }
        private void NavigationPage(DIRECTION direction )
        {
       
            if (isvalidPaging())
            {
                switch (direction)
                {
                    case DIRECTION.previous:
                        _page.Pagenumber -= 1;
                        _page.Offset -= _page.DefaulOffset;
                        break;
                    case DIRECTION.next:
                        _page.Pagenumber += 1;
                        _page.Offset += _page.DefaulOffset;
                        break;
                    default:
                        _page.Pagenumber = 1;
                        _page.Offset = _page.DefaulOffset;
                        break;
                }
                txtoffset.Text = _page.Offset.ToString();
                FilterStock(new StockProduct
                {
                    Category = cmbCategory.Text.getDefaultEmptyCat()
                   ,Name = txtSearch.Text
                });

            }


        }
        private void ClearFilters()
        {
            Fillcat();
            txtSearch.Clear();
        }
        #endregion

        #region Filling Methods
        private async void FillLimitPageVal() {
            IEnumerable<string> lim = await _page.GetLimitPaging();
            cmbLimitPage.ItemsSource = lim;
        }
        private async void Fillcat()
        {

            cmbCategory.SelectedIndex = 0;
            using (Task<IEnumerable<Category>> cat = _categoryservices.GetAll(_page))
            {
                foreach (var ct in await cat)
                {
                    cmbCategory.Items.Add(ct.Name);
                }
            }
            _isloaded = false;
        }
<<<<<<< Updated upstream
        private async void FillStock() {
            IEnumerable<StockProduct> prod = await _stockservices.GetAll(_page);
=======
        /// <summary>
        /// Filter the GridView given the activeFilters() : function
        /// </summary>
        private async void FilterStock()
        {
           //IEnumerable<StockProduct> prod = await _stockservices.GetAll(_page);
            IEnumerable<StockProductView> prod = await _stockservices.GetAlltest(_page);
>>>>>>> Stashed changes
            int total = await _stockservices.GetTotalFound(activeFilters());
            gridProducts.ItemsSource = prod;
            txtResults.Text = getResultMsgAsync(total);
            _page.parsePageData(total);
            pagingLabels(total);
        }
        private async void FilterStock(StockProduct param)
        {
<<<<<<< Updated upstream
            IEnumerable<StockProduct> prod = await _stockservices.GetByFilter(param, _page);
            int total = await _stockservices.GetTotalFound(activeFilters());
=======
           // IEnumerable<StockProduct> prod = await _stockservices.GetByFilter(param, _page);
            IEnumerable<StockProductView> prod = await _stockservices.GetAlltest(_page);
            int total = await _stockservices.GetTotalFound(param);
>>>>>>> Stashed changes
            gridProducts.ItemsSource = prod;
            txtResults.Text = getResultMsgAsync(total);
            _page.parsePageData(total);
            pagingLabels(total);
        }
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
            if (!_isloaded)
            {
                _page.resetPage();
                var param = new StockProduct { 
                    Category = e.AddedItems[0].ToString().getDefaultEmptyCat()
                   ,Name     = txtSearch.Text  
                };
                FilterStock(param);

            }
        }
        private void cmbLimitPage_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!_isloaded)
            {
                _page.Limit = int.Parse(e.AddedItems[0].ToString());
                StockProduct param = activeFilters();
                FilterStock(param);
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
            _windowStock.Activate();
            _windowStock.Show();

        }
        private void btnNuevoProducto_Click(object sender, RoutedEventArgs e)
        {
            // Programmatically change what the frame is showing

             if (_wproduct == null)
             {
                 _wproduct = new Wproduct();
             }
             _wproduct.Owner = this;
             _wproduct.Activate();
             _wproduct.Show();
            
        }
        private void btnPrevious_Click(object sender, RoutedEventArgs e)
        {
            NavigationPage(DIRECTION.previous);
        }
        private void btnNext_Click(object sender, RoutedEventArgs e)
        {
            NavigationPage(DIRECTION.next);

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
                    FillStock();
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
        #endregion


    }
}
