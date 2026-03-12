using SIMA.Domain.Models;
using SIMA.Infrastructure.Repositories;
using SIMA.ExtensionsHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Diagnostics.SymbolStore;
using static System.Net.Mime.MediaTypeNames;

namespace SIMA.Presentation
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private StockProductServices _stockservices;
        private bool _isloaded;
        private Paging _page;

        public MainWindow()
        {
            InitializeComponent();
            _isloaded = true;
            _stockservices = new StockProductServices();
             _page = new Paging();
            txtoffset.Text = _page.Offset.ToString();
            FillLimitPageVal();
            Fillcat();
            FillStock();
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
            using (Task<IEnumerable<Category>> cat = new CategoryServices().GetAll(_page))
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

        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            Fillcat();
            txtSearch.Clear();
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

        private void btnPrevious_Click(object sender, RoutedEventArgs e)
        {
            NavigationPage(DIRECTION.previous);
        }

        private void btnNext_Click(object sender, RoutedEventArgs e)
        {
            NavigationPage(DIRECTION.next);

        }


        #endregion

    }
}
