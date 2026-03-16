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
        private bool _isSupressModel = false;
        private bool _isNewStock = false;
        private bool _editMode = false;

        public bool IsSupressModel { get => _isSupressModel; set => _isSupressModel = value; }
        public bool EditMode { get => _editMode; set => _editMode = value; }

        public Wstocks()
        {
            InitializeComponent();
            this.DataContext = new StockViewModel();
            InitializeAll();
        }
        public Wstocks(StockProduct paramStockProduct)
        {
            InitializeComponent();
            this.DataContext = new StockViewModel();
            _currentStockProduct = paramStockProduct;
            InitializeAll();
            setValuesForm();
        }

        #region utils
        public void SupressEventComboBox(bool val = true)
        {
            _isloadedCat = val;
            _isloadedProd = val;
            _isSupressModel = val;
            ((StockViewModel)this.DataContext).IsSupressed = val;

        }
        private async void setValuesForm()
        {

            int indexCat = await _Categoryervices.GetIdIndex(_currentStockProduct.Category);
            int indexProd = await _Productervices.GetIdIndex(_currentStockProduct.Category, _currentStockProduct.Name);

            cmbCategoryStock.SelectedIndex = indexCat;
            cmbProduct.SelectedIndex = indexProd-1;
            txtStockActual.Text = _currentStockProduct.Stock.ToString();

        }
        private void InitializeAll()
        {
            _util = new Util();
            _util.Loading_spimmer(wloading, true);
            _StockProductservices = new StockProductServices();
            _Productervices = new ProductServices();
            _Categoryervices = new CategoryServices();
            _util.Loading_spimmer(wloading, false);


        }
        private void validation(StockProduct prodparam)
        {
            int outvl;
            bool validStock = (prodparam.Stock > 0);
            bool validCombobox = !string.IsNullOrEmpty(prodparam.Category) && !string.IsNullOrEmpty(prodparam.Name);
            bool valid = (validStock && validCombobox);
            if (!valid)
                throw new CustomException("Form has Errors, Valid first before save");

        }
        private void Clear()
        {
            cmbProduct.ItemsSource = _Productervices.DefeaultProductList();
            cmbCategoryStock.SelectedIndex = 0;
            cmbProduct.SelectedIndex = 0;
            txtStockActual.Text = "0";
        }



        private async void SaveStock()
        {

            try
            {
                string _category = ((Category)cmbCategoryStock.SelectedItem).Name;
                Product prodc = ((Product)cmbProduct.SelectedItem);
                var prod = new StockProduct
                    {
                        IdStock = _editMode ? _currentStockProduct.IdStock : null,
                        Category = _category,
                        Name = prodc.Name,
                        Price = prodc.Price,
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
        private async void FillProd(Product param)
        {
            IEnumerable<Product> prod = await _Productervices.GetByFilter(param);
            cmbProduct.ItemsSource = prod;
            if(!_editMode)
                cmbProduct.SelectedIndex = 0;
        }

        #endregion

        #region Events
        private void cmbCategoryStock_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string Categ = string.Empty;

            try
            {
                if (!_isloadedCat && !_isSupressModel)
                {
                    Categ = ((Category)(cmbCategoryStock.SelectedItem)).Name.getDefaultEmptyCat().isNull("1-1");
                    FillProd(new Product { Name = string.Empty, Category = Categ });

                }

            }
            catch (Exception ex)
            {
                _util.Loading_spimmer(wloading, false);
            }



        }
        private async void cmbProduct_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int? Idprod = 0;
            try
            {
                if (!_isloadedProd && !_isSupressModel && !_editMode)
                {
                    if (cmbProduct.SelectedItem != null)
                    {
                        Idprod = ((Product)(cmbProduct.SelectedItem)).IdProduct;
                        IEnumerable<StockProduct> prodc = await _StockProductservices.GetbyId(Idprod);
                        var hasElement = !_editMode && prodc.Any();
                        _currentStockProduct = _editMode ? _currentStockProduct : (hasElement ? prodc.Single() : new StockProduct() );
                        txtStockActual.Text = hasElement ? _currentStockProduct.Stock.ToString() : "0";

                    }
                    else
                        txtStockActual.Text = "0";

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
            Clear();
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
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (!_editMode)
            {
                cmbCategoryStock.SelectedIndex = 0;
                cmbProduct.SelectedIndex = 0;
            }   
        }
        private void Window_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {

        }
        #endregion


    }
}
