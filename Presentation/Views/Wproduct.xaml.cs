using SIMA.Domain.Models;
using SIMA.ExtensionsHelper;
using SIMA.Helper;
using SIMA.Infrastructure.Repositories;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
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
using System.Windows.Shapes;
using static System.Net.Mime.MediaTypeNames;

namespace SIMA.Presentation.Views
{
    /// <summary>
    /// Interaction logic for Wproduct.xaml
    /// </summary>
    public partial class Wproduct : Window
    {

        private ProductServices _Productervices;
        private CategoryServices _Categoryervices;
        private StockProductServices _StockProductservices;
        private Product _currentProduct;
        private Paging _page;
        private Util _util;
        private bool _isloadedCat;
        private bool _isloadedProd;
        private bool _isNewStock = false;
        


        public Wproduct()
        {
            InitializeComponent();
            this.DataContext = new ProducValidation();
            _util = new Util();
            _util.Loading_spimmer(wloading, true);
            _isloadedCat = true;
            _StockProductservices = new StockProductServices();
            _Productervices = new ProductServices();
            _Categoryervices = new CategoryServices();
            Fillcat();
            _util.Loading_spimmer(wloading, false);
        }

        #region utils
        private void validation() {
            bool valid = false;
            bool priceval = false;
            decimal outvl ;
            priceval = decimal.TryParse(txtPrice.Text, out outvl) ? decimal.Parse(txtPrice.Text) > 0 : false;
            valid = priceval && string.IsNullOrEmpty(txtProductName.Text);
            if (!valid)
                throw new CustomException("Form has Erros, Valid first before save");   

        } 
        private void Clear()
        {
            _isloadedProd = true;
            cmbCategory.SelectedIndex = 0;
            txtProductName.Clear();
            txtPrice.Clear();
            _isloadedProd = false;
        }
        private async void SaveProduct()
        {
            try
            {
                validation();
                string _category = cmbCategory.SelectedItem.ToString();
                var _prod = new Product
                {
                    IdProduct = _currentProduct.IdProduct,
                    Category = _category,
                    Name = txtProductName.Text,
                    Price = decimal.Parse(txtPrice.Text),
                };


                if (await _Productervices.Set(_prod))
                {
                    var prodStock = new StockProduct
                    {
                        IdStock = _Productervices.CurrentIdSave,
                        Category = _prod.Category,
                        Name = _prod.Name,
                        Price = _prod.Price,
                        Stock = 0
                    };

                    if (await _StockProductservices.Set(prodStock))
                    {
                        Clear();
                        MessageBox.Show(this, "Product Sucessfully saved", "Save Product", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    }
                }

            }
            catch (CustomException cx)
            {
                MessageBox.Show(this, cx.Message, "Error Missing Values", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (InvalidOperationException xp)
            {
                MessageBox.Show(this, xp.Message, "Error Product", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "Error while saving the Product ", "Error Product", MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }
        #endregion

        #region Filling Methods
        private async void Fillcat()
        {
            cmbCategory.SelectedIndex = 0;
            Task<IEnumerable<Category>> cat = _Categoryervices.GetAll(_page);
            foreach (var ct in await cat)
            {
                cmbCategory.Items.Add(ct.Name);
            }
            _isloadedCat = false;
        }
        #endregion

        #region Events
        private void btnSaveStock_Click(object sender, RoutedEventArgs e)
        {
            SaveProduct();
        }
        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            Clear();
        }
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            this.Visibility = Visibility.Hidden;
      
        }
        private void cmbCategory_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string Categ = string.Empty;
            _util.Loading_spimmer(wloading, true);
            try
            {
                if (!_isloadedCat)
                {
                    if (cmbCategory.SelectedItem is ComboBoxItem selectedItem)
                        _currentProduct.Category = (string)selectedItem.Content;
                }
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
