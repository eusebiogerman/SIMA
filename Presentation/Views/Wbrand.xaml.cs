using Microsoft.Extensions.Configuration;
using SIMA.Domain.Models.Objects;
using SIMA.Domain.Models.Params;
using SIMA.Domain.Models.Views;
using SIMA.ExtensionsHelper;
using SIMA.Helper;
using SIMA.Infrastructure.Repositories;
using SIMA.Infrastructure.Repositories.Interfaces;
using SIMA.Presentation.Repository;
using SIMA.Presentation.ViewModel;
using SIMA.Templates;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
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
    public partial class Wbrand : Window
    {

        private IContextservices<Brand, BrandView, BrandParam> _service;
        private IContextservices<Product, ProductView, ProductParam> _productervices;
        private WindowServices<Brand, BrandView, BrandParam> _windowservices;

        private CancellationTokenSource _cts;
        private BrandView? _rowData;
        private BrandViewModel _vm;
        private Util _util;
        public WindowServices<Brand, BrandView, BrandParam> WindowServices { get => _windowservices; }

        public Wbrand()
        {

            InitializeComponent();

        }



        #region Util
        /// <summary>
        /// Initialize Window
        /// </summary>
        /// <param name="rowData"></param>
        private void InitializeWindow(BrandView? rowData = null)
        {

            _rowData = rowData;
            _windowservices.EditMode = _rowData != null;
            _windowservices.FromMain = _windowservices.EditMode;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="param"></param>
        /// <returns></returns>
        private async Task FillGrid(BrandParam? param = null)
        {
            var prod = await _service.GetByFilter(param ?? _windowservices.activeFilters("Name"));
            _windowservices.Fill(prod);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="rowData"></param>
        /// <returns></returns>
        private Action EditControl(BrandParam? rowData = null)
        {
            return () =>
            {
                if (rowData?.idBrand == null)
                {
                    _windowservices.ChildLovtextbox?.Clear();
                }


                //Set the Field Values from the grid
                txtIdBrand.Text = rowData?.idBrand.ToString();
                txtName.Text = rowData?.name?.ToString();
                txtPrice.Text = rowData?.price.ToString();

                //Set the Field Values for Product
                _windowservices.ParentLovtextbox.Text = rowData?.categorys ?? " "; //dumny select
                var itemCat = _windowservices.ParentLovtextbox.OriginalSource.FirstOrDefault(p => p.Id == rowData?.idCategory);
                _windowservices.ParentLovtextbox.SelectedItem = itemCat;
                _windowservices.ParentLovtextbox.Close();

                //Set the Field Values for Brand
                _windowservices.ChildLovtextbox.Text = rowData?.products ?? " "; //dumny select
                var itemProd = ((IEnumerable<LovObject>)_windowservices.ChildLovtextbox.ItemsSource)?.FirstOrDefault(p => p.Id == rowData?.idProduct);
                _windowservices.ChildLovtextbox.SelectedItem = itemProd;
                _windowservices.ChildLovtextbox.Close();
            };
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private async Task Clear(string windowheight = "40%")
        {
            await _windowservices.ClearFilters(windowheight, () =>
            {
                txtIdBrand.Text = string.Empty;
                txtPrice.Text = string.Empty;
            }, true);
        }
        #endregion

        #region Events
        private async void btnsSave_Click(object sender, RoutedEventArgs e)
        {
            int? id = string.IsNullOrEmpty(txtIdBrand.Text) ? null : int.Parse(txtIdBrand.Text.ToString());
            int? idprod = _windowservices.ChildLovtextbox?.getSelectedItem().Id;
            decimal price = decimal.Parse(txtPrice.Text.ToString());
            try
            {
                progress.RunProgress(true, "Saving Stock...");
                await progress.DelayProgress();
                bool isset = await _service.Set(new Brand
                {
                    IdBrand = id,
                    IdProduct = idprod,
                    Price = price
                });
                progress.StopProgress();

                if (isset)
                {
                    MessageBox.Show(this, "Stock Sucessfully saved", "Save Stock", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    await Clear();
                    await FillGrid();
                }
                else
                {
                    MessageBox.Show(this, "Error Saving Stock", "Save Stock", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                }
            }
            catch (Microsoft.Data.SqlClient.SqlException ex)
            {
                progress.StopProgress();
                MessageBox.Show(this, "DataBase Error Failed", "Fatal Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            catch (Exception)
            {
                progress.StopProgress();
                MessageBox.Show(this, "System Error Failed", "Fatal Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
        private async void btnNew_Click(object sender, RoutedEventArgs e)
        {
            await Clear();
        }
        private async void btnsClear_Click(object sender, RoutedEventArgs e)
        {
            await Clear(_windowservices.FormState ? "40%" : "60%");
        }
        private async void btnsClose_Click(object sender, RoutedEventArgs e)
        {
            await Clear();
            this.Close();
        }
        private void PagePrevious_Click(object sender, RoutedEventArgs e)
        {
            _windowservices.NavigationGrid(DIRECTION.previous);
        }
        private void PageNext_Click(object sender, RoutedEventArgs e)
        {
            _windowservices.NavigationGrid(DIRECTION.next);
        }
        private async void PageNavigation_SelectionChanged(object sender, RoutedEventArgs e)
        {
            if (!_windowservices.Isloaded)
            {
                _windowservices.Page.Limit = (int)_windowservices.PageControl.GetSelectedItemsPerPage();
                if (!_windowservices.FromMain)
                {
                    await FillGrid();
                }
                _windowservices.UpdatePaging();
            }
        }
        private async void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            _windowservices.Page.Limit = (int)_windowservices.PageControl.GetSelectedItemsPerPage();
            await FillGrid();
            _windowservices.UpdatePaging();
        }
        private async void parentCombobox_SelectionChanged(object sender, RoutedEventArgs e)
        {
            LovObject? sendobj = _windowservices?.ParentLovtextbox?.getSelectedItem(sender);
            int? dummy = (sendobj.Id == 0 ) ? -1 : null;
            var cat = await _productervices.GetByFilter(new ProductParam { idCategory = sendobj.Id, idProduct = dummy });
            var sel = cat.Select(p => new LovObject { Id = p.IdProduct, Value = p.Name });
            _windowservices.FillChildCombobox(sel);
        }
        private async void txtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {

            _cts?.Cancel();
            _cts = new CancellationTokenSource();

            try
            {
                if (!_windowservices.Isloaded && !_windowservices.FromMain)
                {
                    await Task.Delay(300, _cts.Token);
                    await FillGrid();
                }
            }
            catch (TaskCanceledException)
            {
                //Cancel
            }

        }
        private async void btnRemove_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show(this, "Confirm removing Stock ?", "Remove Stock", MessageBoxButton.YesNoCancel, MessageBoxImage.Warning);
            try
            {
                if (result == MessageBoxResult.Yes)
                {

                    Button btn = sender as Button;
                    BrandView rowData = (BrandView)btn.DataContext;
                    bool valid = await _service.Delete(rowData.IdBrand) > 0;
                    if (valid)
                    {
                        await FillGrid();
                        MessageBox.Show(this, "Stock Succesfully removed", "Remove Stock", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show(this, "Error removing the Brand", "Remove Stock", MessageBoxButton.OK, MessageBoxImage.Information);
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
                _windowservices.EditMode = true;
                await _windowservices.Edit("40%", EditControl(new BrandParam
                {
                    idBrand = rowData.IdBrand,
                    name = rowData.Name,
                    price = rowData.Price,
                    idProduct = rowData.IdProduct,
                    products = rowData.Products,
                    idCategory = rowData.IdCategory,
                    categorys = rowData.Categorys
                }));
                _windowservices.EditMode = false;
            }
            catch (Exception)
            {
                _windowservices.EditMode = false;
            }

        }
        private void Window_Closing(object sender, CancelEventArgs e)
        {
            e.Cancel = true;
            this.Visibility = Visibility.Hidden;
        }
        private void Window_Initialized(object sender, EventArgs e)
        {
            _util = new Util();
            IPaging _page = new Paging();
            IConfiguration _config = _util.CustomConfiguration();
            _vm = new BrandViewModel(_page, _config);
            this.DataContext = _vm;
            _vm.ShowErrorFromModel += Vm_ShowErrorFromModel;
            _service = new BrandServices(_config);
            _productervices = new ProductServices(_config);
            _windowservices = new WindowServices<Brand, BrandView, BrandParam>(_config, _page, _service);
            _windowservices.SupressEventComboBox();

        }
        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {

            _windowservices.DataContext = _vm;
            _windowservices.PageControl = pageControl;
            _windowservices.ParentLovtextbox = cmbCategory;
            _windowservices.ChildLovtextbox = cmbPropduct;
            _windowservices.TxtSearch = txtSearch;
            _windowservices.TxtResults = txtResults;
            _windowservices.IngorePredicate = (x) => x.IdProduct != null;
            _windowservices.Form = FormBrand;
            _windowservices.Status = statusbox;
            _windowservices.Status.ItemsSource = _windowservices.ObsrverStatus;
            _windowservices.FormIsOpen(false);

            progress.RunProgress(true, "Init Window....");
            _windowservices.InsertStatus("Initializing Window.......", Brushes.Blue);
            _windowservices.Page.Offset = (int?)_windowservices.PageControl.GetSelectedItemsPerPage() ?? _windowservices.Page.DefaultOffset;
            _windowservices.InsertStatus("Retriving Brands.......", Brushes.Blue);
            await _windowservices.Page.FillLimitPageVal(_windowservices.PageControl);
            _windowservices.UpdatePaging();
            _windowservices.SupressEventComboBox(false);
        }
        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            _windowservices.CurrentWindow = this;
            _windowservices.GridListView = _windowservices.GridListView ?? gridBrands;
            _windowservices.GridView = _windowservices.GridView ?? gridCellBrands;
            _windowservices.ResizeGrid("50%");
        }
        private void Vm_ShowErrorFromModel(string mensaje)
        {
            MessageBox.Show(this, mensaje, "Model Error", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
        private async void Window_ContentRendered(object sender, EventArgs e)
        {

            progress.StopProgress();
            _windowservices.InsertStatus("Windows Ready.......", Brushes.Green);
            if (_rowData != null)
            {
                _windowservices.SupressEventComboBox(false);
                await _windowservices.Edit("40%", EditControl(new BrandParam
                {
                    idBrand = _rowData.IdBrand,
                    name = _rowData.Name,
                    price = _rowData.Price,
                    idProduct = _rowData.IdProduct,
                    products = _rowData.Products,
                    idCategory = _rowData.IdCategory,
                    categorys = _rowData.Categorys
                }));
                _windowservices.FromMain = false;
                await FillGrid(new BrandParam { idBrand = _rowData.IdBrand });
            }

        }
        #endregion

    }
}

