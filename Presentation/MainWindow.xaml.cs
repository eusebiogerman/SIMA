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
            _util = new Util();
            _util.Loading_spimmer(wloading, true);
            _isloaded = true;
            _stockservices = new StockProductServices();
            _categoryservices = new CategoryServices();
             _page = new Paging();
            txtoffset.Text = _page.Offset.ToString();
            FillLimitPageVal();
            Fillcat();
            FillStock();
            _util.Loading_spimmer(wloading, false);

        }

        #region Utils
        private enum DIRECTION { 
            previous = 1,
            next = 2 
        }
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
        private async void FillStock() {
            IEnumerable<StockProduct> prod = await _stockservices.GetAll(_page);
            int total = await _stockservices.GetTotalFound(activeFilters());
            gridProducts.ItemsSource = prod;
            txtResults.Text = getResultMsgAsync(total);
            _page.parsePageData(total);
            pagingLabels(total);
        }
        private async void FilterStock(StockProduct param)
        {
            IEnumerable<StockProduct> prod = await _stockservices.GetByFilter(param, _page);
            int total = await _stockservices.GetTotalFound(activeFilters());
            gridProducts.ItemsSource = prod;
            txtResults.Text = getResultMsgAsync(total);
            _page.parsePageData(total);
            pagingLabels(total);
        }
        #endregion

        #region Events
        private void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
        private void txtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!_isloaded)
            {
                _page.resetPage();
                StockProduct param = activeFilters();
                FilterStock(param);
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
            Fillcat();
            txtSearch.Clear();
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
        #endregion


    }
}
