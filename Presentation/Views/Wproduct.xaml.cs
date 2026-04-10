using Microsoft.Extensions.Configuration;
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
using SIMA.Infrastructure.Repositories.Interfaces;
using SIMA.Domain.Models.Objects;
using SIMA.Domain.Models.Views;
using SIMA.Domain.Models.Params;
using SIMA.Domain.Models.Intefaces;
using SIMA.Presentation.Repository;
using Azure;
using System.Collections.ObjectModel;
using Microsoft.Data.SqlClient;

namespace SIMA.Presentation.Views
{
    /// <summary>
    /// Interaction logic for Wproduct.xaml
    /// </summary>
    public partial class Wproduct : Window
    {
        private readonly ICacheService _cache;
        private WindowServices<Product, ProductView, ProductParam> _windowservices;
        private CancellationTokenSource _cts;
        private ProductView? _rowData;
        private ProductViewModel _vm;
        private const string _editHeight = "40%";

        public WindowServices<Product, ProductView, ProductParam> WindowServices { get => _windowservices; }

        public Wproduct(ICacheService cache)
        {
            _cache = cache;
            InitializeComponent();
            InitializeWindow();
        }

        #region Util
        /// <summary>
        /// Initialize Window
        /// </summary>
        /// <param name="rowData"></param>
        private void InitializeWindow(ProductView? rowData = null)
        {
            _rowData = rowData;
            _windowservices.EditMode = _rowData != null;
            _windowservices.FromMain = _windowservices.EditMode;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="rowData"></param>
        /// <returns></returns>
        private Action EditControl(ProductParam? rowData = null)
        {
            return () =>
            {

                //Set the Field Values from the grid
                txtIdProduct.Text = rowData?.idProduct.ToString();
                txtName.Text = rowData?.name;
                txtPrice.Text = rowData?.price.ToString();

                //Set the Field Values for Category
                _windowservices.SetLoveValueItem(_windowservices?.ParentLovtextbox, rowData?.categorys, rowData?.idCategory);

            };
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private async Task Clear(string windowheight = _editHeight) {
            await _windowservices.ClearFilters(windowheight, () =>
            {
                txtIdProduct.Text = string.Empty;
                txtName.Text = string.Empty;
                txtPrice.Text = string.Empty;
            });

        }
        #endregion

        #region Events
        private async void btnsSave_Click(object sender, RoutedEventArgs e)
        {
            int? id = string.IsNullOrEmpty(txtIdProduct.Text) ? null : int.Parse(txtIdProduct.Text.ToString());
            int? idcat = _windowservices.ParentLovtextbox?.getSelectedItem().Id;
            string name = txtName.Text.ToString();
            decimal price = decimal.Parse(txtPrice.Text.isNull("0"));
            try
            {
                _windowservices.InsertStatus("Saving Product...", BrushesStatus.DBProcess);
                await _windowservices.Status.DelayProgress();
                bool isset = await _windowservices.SetAsync(new Product
                {
                    IdProduct = id,
                    IdCategory = idcat,
                    Name = name,
                    Price = price
                });

                if (isset)
                {
                    _windowservices.InsertStatus("Product Sucessfully removed", BrushesStatus.DBProcess, false);
                    MessageBox.Show(this, "Product Sucessfully saved", "Save Product", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    await Clear();
                    await _windowservices.FillAsync(_windowservices.activeFilters("name"));
                    _windowservices?.Status?.StopProgress();
                }
                else
                    _windowservices.CatchExceptionAndMsg(new Exception("Error Saving Product"));
            }
            catch (Microsoft.Data.SqlClient.SqlException ex)
            {
                _windowservices.CatchExceptionAndMsg(ex);
            }
            catch (Exception se)
            {
                _windowservices.CatchExceptionAndMsg(se);
            }
        }
        private async void btnRemove_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show(this, "Confirm removing Product ?", "Remove Product", MessageBoxButton.YesNoCancel, MessageBoxImage.Warning);
            try
            {
                if (result == MessageBoxResult.Yes)
                {
                    _windowservices.InsertStatus("Deleting Product...", BrushesStatus.DBProcess);
                    await _windowservices.Status.DelayProgress();
                    Button btn = sender as Button;
                    ProductView rowData = (ProductView)btn.DataContext;
                    bool valid = await _windowservices.DeleteAsync(rowData.IdProduct) > 0;
                    if (valid)
                    {
                        _windowservices.InsertStatus("Product Sucessfully removed", BrushesStatus.DBProcess, false);
                        await _windowservices.FillAsync(_windowservices.activeFilters("name"));
                        MessageBox.Show(this, "Product Succesfully removed", "Remove Product", MessageBoxButton.OK, MessageBoxImage.Information);
                        _windowservices.Status.StopProgress();
                    }
                    else
                        _windowservices.CatchExceptionAndMsg(new Exception("Error removing the Product"));
                }
            }
            catch (SqlException ex)
            {
                _windowservices.CatchExceptionAndMsg(ex);
            }
            catch (Exception se)
            {
                _windowservices.CatchExceptionAndMsg(se);
            }
        }
        private async void btnUpdate_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Button btn = sender as Button;
                ProductView rowData = (ProductView)btn.DataContext;
                _windowservices.EditMode = true;
                await _windowservices.Edit(_editHeight, EditControl(new ProductParam
                {
                    idProduct = rowData.IdProduct,
                    idCategory = rowData.IdCategory,
                    categorys = rowData.Categorys,
                    name = rowData.Name,
                    price = rowData.Price
                }));
                _windowservices.EditMode = false;
            }
            catch (Exception)
            {
                _windowservices.EditMode = false;
            }

        }
        private async void btnNew_Click(object sender, RoutedEventArgs e)
        {
            await Clear();
        }
        private async void btnsClear_Click(object sender, RoutedEventArgs e)
        {
            await Clear(_windowservices.FormState ? _editHeight : _windowservices.StandarHeight);
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
                    await _windowservices.FillAsync(_windowservices.activeFilters("name"));
                }
                _windowservices.UpdatePaging();
            }
        }
        private async void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            _windowservices.Page.Limit = (int)_windowservices.PageControl.GetSelectedItemsPerPage();
            await _windowservices.FillAsync(_windowservices.activeFilters("name"));
        }
        private async void parentCombobox_SelectionChanged(object sender, RoutedEventArgs e)
        {
            await _windowservices.FillAsync(_windowservices.activeFilters("name"));
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
                    await _windowservices.FillAsync(_windowservices.activeFilters("name"));
                }
            }
            catch (TaskCanceledException)
            {
                //Cancel
            }

        }
        private void Window_Closing(object sender, CancelEventArgs e)
        {
           // _windowservices = null;
            e.Cancel = true;
            this.Visibility = Visibility.Hidden;
        }
        private void Window_Initialized(object sender, EventArgs e)
        {
             IPaging _page = new Paging();
             IConfiguration _config = new Util().CustomConfiguration();
             _vm = new ProductViewModel(_page, _config,_cache);
             this.DataContext = _vm;
             _vm.ShowErrorFromModel += Vm_ShowErrorFromModel;
             _windowservices = new WindowServices<Product,ProductView,ProductParam>(_config, _page, new ProductServices(_config,_cache));
             _windowservices.SupressEventComboBox();

        }
        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {

            _windowservices.DataContext = _vm;
            _windowservices.PageControl = pageControl;
            _windowservices.ParentLovtextbox = cmbCategory;
            _windowservices.ChildLovtextbox = null;
            _windowservices.TxtSearch = txtSearch;
            _windowservices.TxtResults = txtResults;
            _windowservices.Form = FormProduct;
            _windowservices.FormIsOpen(false);
            _windowservices.Status = statusbox;
            _windowservices.ObsrverStatus = new ObservableCollection<StatusItem>();
            _windowservices.Status.ItemsSource = _windowservices.ObsrverStatus;

            _windowservices.InsertStatus("Initializing Product Window.......", BrushesStatus.Progress);
            await _windowservices.Status.DelayProgress();
            _windowservices.Page.Offset = (int?)_windowservices.PageControl.GetSelectedItemsPerPage() ?? _windowservices.Page.DefaultOffset;
            _windowservices.InsertStatus("Retriving Products.......", BrushesStatus.Progress, false);
            await _windowservices.Page.FillLimitPageVal(_windowservices.PageControl);
            _windowservices.UpdatePaging();
            _windowservices.SupressEventComboBox(false);
            _windowservices.InsertStatus("Window ready.......", BrushesStatus.Progress);
            _windowservices.Status.StopProgress();
        }
        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            _windowservices.CurrentWindow = this;
            _windowservices.GridListView = _windowservices.GridListView ?? gridProducts;
            _windowservices.GridView = _windowservices.GridView ?? gridCellProducts;
            _windowservices.StandarHeight = "50%";
            _windowservices.ResizeGrid();
        }
        private void Vm_ShowErrorFromModel(string mensaje)
        {
            _windowservices.CatchExceptionAndMsg(new Exception(mensaje), "Model Error");
        }
        private async void Window_ContentRendered(object sender, EventArgs e)
        {
            _windowservices.InsertStatus("Product Window Ready.......", BrushesStatus.Progress);
            if (_rowData != null)
            {
                _windowservices.SupressEventComboBox(false);
                await _windowservices.Edit(_editHeight, EditControl(new ProductParam
                {
                    idProduct = _rowData.IdProduct,
                    idCategory = _rowData.IdCategory,
                    categorys = _rowData.Categorys,
                    name = _rowData.Name,
                    price = _rowData.Price
                }));
                _windowservices.FromMain = false;
                await _windowservices.FillAsync(new ProductParam { idProduct = _rowData.IdProduct });
            }
            _windowservices?.Status?.StopProgress();
        }
        #endregion

    }
}
