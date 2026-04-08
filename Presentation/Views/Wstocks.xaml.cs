using Azure;
using Microsoft.Extensions.Configuration;
using SIMA.Domain.Models.Intefaces;
using SIMA.Domain.Models.Objects;
using SIMA.Domain.Models.Params;
using SIMA.Domain.Models.Views;
using SIMA.ExtensionsHelper;
using SIMA.Helper;
using SIMA.Helper.Interfaces;
using SIMA.Infrastructure.Repositories;
using SIMA.Infrastructure.Repositories.Interfaces;
using SIMA.Presentation.Repository;
using SIMA.Presentation.ViewModel;
using SIMA.Templates;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO.Pipelines;
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
using System.Xml.Linq;
using static System.Net.Mime.MediaTypeNames;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace SIMA.Presentation.Views
{
    /// <summary>
    /// Interaction logic for Wbrand.xaml
    /// </summary>
    public partial class Wstocks : Window
    {

        private IContextservices<StockProduct, StockProductView, StockProductParam> _service;
        private IContextservices<Brand, BrandView, BrandParam> _brandsrervices;
        private WindowServices<StockProduct, StockProductView, StockProductParam> _windowservices;

        private CancellationTokenSource _cts;
        private StockProductView? _rowData;
        private StockViewModel _vm;
        private Util _util;
        public WindowServices<StockProduct, StockProductView, StockProductParam> WindowServices { get => _windowservices; }

        public Wstocks()
        {
            InitializeComponent();
            InitializeWindow();

        }
        public Wstocks(StockProductView rowData)
        {
            InitializeComponent();
            InitializeWindow(rowData);
        }

        #region Util
        /// <summary>
        /// Initialize Window
        /// </summary>
        /// <param name="rowData"></param>
        private void InitializeWindow(StockProductView? rowData = null)
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
        private async Task FillGrid(StockProductParam? param = null)
        {
            var prod = await _service.GetByFilter(param ?? _windowservices.activeFilters("Name"));
            _windowservices.Fill(prod);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="rowData"></param>
        /// <returns></returns>
        private Action EditControl(StockProductParam? rowData = null)
        {
            return () =>
            {
                if (rowData.idProduct == null) {
                    _windowservices.ChildLovtextbox.Clear();
                }


                //Set the Field Values from the grid
                txtIdStock.Text = rowData.idStock.ToString();
                txtStock.Text = rowData.stock.ToString();

                //Set the Field Values for Product
                _windowservices.ParentLovtextbox.Text = rowData.products ?? " "; //dumny select
                var itemProd = _windowservices.ParentLovtextbox.OriginalSource.FirstOrDefault(p => p.Id == rowData.idProduct);
                _windowservices.ParentLovtextbox.SelectedItem = itemProd;
                _windowservices.ParentLovtextbox.Close();

                //Set the Field Values for Brand
                _windowservices.ChildLovtextbox.Text = rowData.brands ?? " "; //dumny select
                var itemBrand = ((IEnumerable<LovObject>)_windowservices.ChildLovtextbox.ItemsSource)?.FirstOrDefault(p => p.Id == rowData.idBrand);
                _windowservices.ChildLovtextbox.SelectedItem = itemBrand;
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
                txtIdStock.Text = string.Empty;
                txtStock.Text = string.Empty;
            }, true);

        }
        #endregion
        
        #region Events
        private async void btnsSave_Click(object sender, RoutedEventArgs e)
        {
            int? id = string.IsNullOrEmpty(txtIdStock.Text) ? null : int.Parse(txtIdStock.Text.ToString());
            int? idbrand = _windowservices.ChildLovtextbox?.getSelectedItem().Id;
            int stock = int.Parse(txtStock.Text.ToString());
            try
            {
                progress.RunProgress(true, "Saving Stock...");
                await progress.DelayProgress();
                bool isset = await _service.Set(new StockProduct
                {
                    IdStock = id,
                    IdBrand = idbrand,
                    Stock = stock
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
            var sendobj = _windowservices.ParentLovtextbox.getSelectedItem(sender);
            int? dummy = (sendobj.Id == 0 || !sendobj.Id.HasValue) ? -1 : null;
            var cat = await _brandsrervices.GetByFilter(new BrandParam { idProduct = sendobj.Id, idBrand = dummy });
            var sel = cat.Select(p => new LovObject { Id = p.IdBrand, Value = p.Name });
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
                    StockProductView rowData = (StockProductView)btn.DataContext;
                    bool valid = await _service.Delete(rowData.IdStock) > 0;
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
                StockProductView rowData = (StockProductView)btn.DataContext;
                _windowservices.EditMode = true;
                await _windowservices.Edit("40%", EditControl(new StockProductParam
                {
                    idStock = rowData.IdStock,
                    idProduct = rowData.IdProduct,
                    idBrand = rowData.IdBrand,
                    stock = rowData.Stock,
                    products = rowData.Products,
                    brands = rowData.Brands
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
            _vm = new StockViewModel(_page, _config);
            this.DataContext = _vm;
            _vm.ShowErrorFromModel += Vm_ShowErrorFromModel;
            _service = new StockProductServices(_config);
            _brandsrervices = new BrandServices(_config);
            _windowservices = new WindowServices<StockProduct, StockProductView, StockProductParam>(_config, _page, _service);
            _windowservices.SupressEventComboBox();
        }
        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {

            _windowservices.DataContext = _vm;
            _windowservices.PageControl = pageControl;
            _windowservices.ParentLovtextbox = cmbPropduct;
            _windowservices.ChildLovtextbox = cmbBrand;
            _windowservices.TxtSearch = txtSearch;
            _windowservices.TxtResults = txtResults;
            _windowservices.IngorePredicate = (x) => x.IdProduct != null;
            _windowservices.Form = FormStock;
            _windowservices.FormIsOpen(false);

            progress.RunProgress(true, "Init Window....");
            _windowservices.Page.Offset = (int?)_windowservices.PageControl.GetSelectedItemsPerPage() ?? _windowservices.Page.DefaultOffset;
            await _windowservices.Page.FillLimitPageVal(_windowservices.PageControl);
            _windowservices.UpdatePaging();
            _windowservices.SupressEventComboBox(false);
        }
        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            _windowservices.CurrentWindow = this;
            _windowservices.GridListView = _windowservices.GridListView ?? gridStocks;
            _windowservices.GridView = _windowservices.GridView ?? gridCellStocks;
            _windowservices.ResizeGrid("60%");
        }
        private void Vm_ShowErrorFromModel(string mensaje)
        {
            MessageBox.Show(this, mensaje, "Model Error", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
        private async void Window_ContentRendered(object sender, EventArgs e)
        {
      
            progress.StopProgress();
            if (_rowData != null)
            {
                _windowservices.SupressEventComboBox(false);
                await _windowservices.Edit("40%", EditControl(new StockProductParam
                {
                    idStock = _rowData.IdStock,
                    idProduct = _rowData.IdProduct,
                    idBrand = _rowData.IdBrand,
                    stock = _rowData.Stock,
                    products = _rowData.Products,
                    brands = _rowData.Brands
                }));
                _windowservices.FromMain = false;
                await FillGrid(new StockProductParam { idBrand = _rowData.IdBrand });
            }

        }
        #endregion
    }
}

