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
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media.Animation;
using static System.Net.Mime.MediaTypeNames;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace SIMA.Presentation.Views
{
    /// <summary>
    /// Interaction logic for Wstocks.xaml
    /// </summary>
    public partial class Wstocks : Window
    {
        private ProductServices _Productervices;
        private CategoryServices _Categoryervices;
        private StockProductServices _StockProductservices;
        private Paging _page;
        private StockProduct _currentStockProduct;
        private Util _util;
        private bool _isloadedCat;
        private bool _isloadedProd;
        private bool _isNewStock = false;

        public Wstocks()
        {
            InitializeAll();

        }
        public Wstocks(StockProduct paramStockProduct)
        {

            _currentStockProduct = paramStockProduct;
            InitializeAll();
            setValuesForm();
        }

        #region utils
        private async void setValuesForm() {

            int indexCat = await _Categoryervices.GetIdIndex(_currentStockProduct.Category);
            int indexProd = await _Productervices.GetIdIndex(_currentStockProduct.Category, _currentStockProduct.Name);

            cmbCategory.SelectedIndex = indexCat;
            cmbProduct.SelectedIndex = indexProd;
            txtStockActual.Text = _currentStockProduct.Stock.ToString();
        }
        private void InitializeAll()
        {
            InitializeComponent();
            _util = new Util();
            this.DataContext = new StockValidation();
            _util.Loading_spimmer(wloading, true);
            _isloadedCat = true;
            _StockProductservices = new StockProductServices();
            _Productervices = new ProductServices();
            _Categoryervices = new CategoryServices();
            Fillcat();
            _util.Loading_spimmer(wloading, false);
        }
        private void validation(StockProduct prodparam)
        {
            int outvl;
            bool validStock = (prodparam.Stock  > 0);
            bool validCombobox = !string.IsNullOrEmpty(prodparam.Category) && !string.IsNullOrEmpty(prodparam.Name);
            bool valid = (validStock && validCombobox);
            if (!valid)
                throw new CustomException("Form has Errors, Valid first before save");

        }
        private void Clear()
        {
            _isloadedProd = true;
            cmbCategory.SelectedIndex = 0;
            cmbProduct.Items.Clear();
            cmbProduct.Items.Add(new ComboBoxItem { Content = "Select", Tag = "0" });
            cmbProduct.SelectedIndex = 0;
            txtStockActual.Text = "0";
            _isloadedProd = false;
        }
        private async void SaveStock()
        {

            try
            {
                ComboBoxItem itemProd = ((ComboBoxItem)cmbProduct.SelectedItem);
                ComboBoxItem itemCat = ((ComboBoxItem)cmbCategory.SelectedItem);

                string _category = ((string)itemCat.Content).getDefaultEmptyCat();
                int? _sequence = _currentStockProduct.IdStock;
                IEnumerable<Product> prodc = await _Productervices.GetbyId((int)itemProd.Tag);
                decimal _price = prodc.Single().Price;

                var prod = new StockProduct
                {
                    IdStock = _sequence,
                    Category = _category,
                    Name = (string)itemProd.Content,
                    Price = _price,
                    Stock = int.Parse(txtStockActual.Text)
                };

                validation(prod);
                bool isSaved = await _StockProductservices.Set(prod);
                if (isSaved)
                {
                    Clear();
                    MessageBox.Show(this, "Stock Sucessfully saved", "Save Stock", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                }
            }
            catch (CustomException cx)
            {
                MessageBox.Show(this, cx.Message, "Error Missing Values", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (InvalidOperationException xp)
            {
                MessageBox.Show(this, xp.Message, "Error Stock", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "Error while saving the Stock, Invalid Valiues ", "Error Stock", MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }
        #endregion

        #region Filling Methods
        private async void Fillcat()
        {
            int id = 0;
            cmbCategory.SelectedIndex = 0;
            Task<IEnumerable<Category>> cat = _Categoryervices.GetAll(_page);
            foreach (var ct in await cat)
            {
                cmbCategory.Items.Add(new ComboBoxItem { Content = ct.Name, Tag = id++ });
            }
            _isloadedCat = false;
        }
        private async void FillProd(Product param)
        {
            _isloadedProd = true;
            cmbProduct.Items.Clear();
            cmbProduct.SelectedIndex = 0;
            IEnumerable<Product> prod = await _Productervices.GetByFilter(param);
            cmbProduct.Items.Add(new ComboBoxItem { Content = "Select", Tag = 0 });

            foreach (var ct in prod)
            {
                cmbProduct.Items.Add(new ComboBoxItem { Content = ct.Name, Tag = ct.IdProduct });
            }
            _isloadedProd = false;

        }
     
        #endregion

        #region Events
        private void cmbCategory_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string Categ = string.Empty;

            try
            {
                if (!_isloadedCat)
                {

                    if (cmbCategory.SelectedItem is ComboBoxItem selectedItem)
                    {
                        Categ = (string)selectedItem.Content;
                    }

                    if (Categ.getDefaultEmptyCat() == string.Empty)
                    {
                        txtStockActual.Clear();
                    }

                    _util.Loading_spimmer(wloading, true);
                    FillProd(new Product { Name = string.Empty, Category = Categ });
                    _util.Loading_spimmer(wloading, false);
                }
            }
            catch (Exception ex)
            {
                _util.Loading_spimmer(wloading, false);
            }



        }
        private async void cmbProduct_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int Idprod = 0;
            try
            {
                if (!_isloadedProd)
                {

                    if (cmbProduct.SelectedItem is ComboBoxItem selectedItem)
                        Idprod = (int)selectedItem.Tag;

                    IEnumerable<StockProduct> prodc = await _StockProductservices.GetbyId(Idprod);
                    _currentStockProduct = prodc.Single();
                    txtStockActual.Text = prodc.Any() ? _currentStockProduct.Stock.ToString() : "0";
                }
                wloading.Visibility = Visibility.Hidden;

            }
            catch (Exception ex)
            {
                wloading.Visibility = Visibility.Hidden;
            }
      
        }
        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            Clear();

        }
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        private void btnSaveStock_Click(object sender, RoutedEventArgs e)
        {
            SaveStock();
        }
        private void btnNewStock_Click(object sender, RoutedEventArgs e)
        {
            Clear();
        }
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            this.Visibility = Visibility.Hidden;
        }
        #endregion


    }
}
