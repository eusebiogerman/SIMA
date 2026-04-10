using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using SIMA.Domain.Models.Objects;
using SIMA.Domain.Models.Params;
using SIMA.Domain.Models.Views;
using SIMA.ExtensionsHelper;
using SIMA.Helper;
using SIMA.Infrastructure.Repositories;
using SIMA.Infrastructure.Repositories.Interfaces;
using SIMA.Presentation.Repository;
using SIMA.Templates;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace SIMA.Presentation.Views
{
    /// <summary>
    /// Interaction logic for Wbrand.xaml
    /// </summary>
    public partial class Wbrand : Window
    {
        private readonly ICacheService _cache;
        private WindowServices<Brand, BrandView, BrandParam> _windowservices;
        private CancellationTokenSource _cts;
        private BrandView? _rowData;
        private BrandViewModel _vm;
        private const string _editHeight = "40%";

        public WindowServices<Brand, BrandView, BrandParam> WindowServices { get => _windowservices; }

        public Wbrand()
        {
            InitializeComponent();
        }

        public Wbrand(ICacheService cache)
        {
            _cache = cache;
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

                //Set the Field Values for Category
                _windowservices?.SetLoveValueItem(_windowservices?.ParentLovtextbox, rowData?.categorys, rowData?.idCategory);

                //Set the Field Values for Product
                _windowservices?.SetLoveValueItem(_windowservices?.ChildLovtextbox, rowData?.products, rowData?.idProduct);

            };
        }
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private async Task Clear(string windowheight = _editHeight)
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
            decimal price = decimal.Parse(txtPrice.Text.isNull("0"));
            try
            {
                _windowservices.InsertStatus("Saving Brand...", BrushesStatus.DBProcess);
                await _windowservices.Status.DelayProgress();
                bool isset = await _windowservices.SetAsync(new Brand
                {
                    IdBrand = id,
                    IdProduct = idprod,
                    Price = price
                });

                if (isset)
                {
                    _windowservices.InsertStatus("Brand Sucessfully saved", BrushesStatus.DBProcess, false);
                    MessageBox.Show(this, "Brand Sucessfully saved", "Save Brand", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    await Clear();
                    await _windowservices.FillAsync(_windowservices.activeFilters("name"));
                    _windowservices.Status.StopProgress();
                }
                else
                    _windowservices.CatchExceptionAndMsg(new Exception("Error Saving Stock"));
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
        private async void btnRemove_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show(this, "Confirm removing Brand ?", "Remove Stock", MessageBoxButton.YesNoCancel, MessageBoxImage.Warning);
            try
            {
                if (result == MessageBoxResult.Yes)
                {
                    _windowservices.InsertStatus("Deleting Brand...", BrushesStatus.DBProcess);
                    await _windowservices.Status.DelayProgress();
                    Button btn = sender as Button;
                    BrandView rowData = (BrandView)btn.DataContext;
                    bool valid = await _windowservices.DeleteAsync(rowData.IdBrand) > 0;
                    if (valid)
                    {
                        _windowservices.InsertStatus("Brand Sucessfully removed", BrushesStatus.DBProcess, false);
                        await _windowservices.FillAsync(_windowservices.activeFilters("name"));
                        MessageBox.Show(this, "Brand Succesfully removed", "Remove Brand", MessageBoxButton.OK, MessageBoxImage.Information);
                        _windowservices.Status.StopProgress();
                    }
                    else
                        _windowservices.CatchExceptionAndMsg(new Exception("Error removing the Brand"));
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
                BrandView rowData = (BrandView)btn.DataContext;
                _windowservices.EditMode = true;
                await _windowservices.Edit(_editHeight, EditControl(new BrandParam
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
                }else
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
            await _windowservices.FillChildComboboxAsync<Brand, BrandView, BrandParam>
                 (sender, typeof(BrandServices), "IdProduct", "Name", "idCategory", "idProduct");

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
            e.Cancel = true;
            this.Visibility = Visibility.Hidden;
        }
        private void Window_Initialized(object sender, EventArgs e)
        {
            IPaging _page = new Paging();
            IConfiguration _config = new Util().CustomConfiguration();
            _vm = new BrandViewModel(_page, _config, _cache);
            this.DataContext = _vm;
            _vm.ShowErrorFromModel += Vm_ShowErrorFromModel;
            _windowservices = new WindowServices<Brand, BrandView, BrandParam>(_config, _page, new BrandServices(_config,_cache));
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
            _windowservices.ObsrverStatus = new ObservableCollection<StatusItem>();
            _windowservices.Status.ItemsSource = _windowservices.ObsrverStatus;
            _windowservices.FormIsOpen(false);

            _windowservices.InsertStatus("Initializing Brand Window.......", BrushesStatus.Progress);
            await _windowservices.Status.DelayProgress();
            _windowservices.Page.Offset = (int?)_windowservices.PageControl.GetSelectedItemsPerPage() ?? _windowservices.Page.DefaultOffset;
            _windowservices.InsertStatus("Retriving Brands.......", BrushesStatus.Progress, false);
            await _windowservices.Page.FillLimitPageVal(_windowservices.PageControl);
            _windowservices.UpdatePaging();
            _windowservices.SupressEventComboBox(false);
            _windowservices.InsertStatus("Window ready.......", BrushesStatus.Progress);
            _windowservices.Status.StopProgress();
        }
        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            _windowservices.CurrentWindow = this;
            _windowservices.GridListView = _windowservices.GridListView ?? gridBrands;
            _windowservices.GridView = _windowservices.GridView ?? gridCellBrands;
            _windowservices.StandarHeight = "50%";
            _windowservices.ResizeGrid();
        }
        private void Vm_ShowErrorFromModel(string mensaje)
        {
            _windowservices.CatchExceptionAndMsg(new Exception(mensaje), "Model Error");
        }
        private async void Window_ContentRendered(object sender, EventArgs e)
        {
            _windowservices.InsertStatus("Brands Window Ready.......", BrushesStatus.Progress);
            if (_rowData != null && _windowservices!= null)
            {
                _windowservices?.SupressEventComboBox(false);
                await _windowservices.Edit(_editHeight, EditControl(new BrandParam
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
                await _windowservices.FillAsync(new BrandParam { idBrand = _rowData.IdBrand });
            }
            _windowservices.Status.StopProgress();
        }
        #endregion

    }
}

