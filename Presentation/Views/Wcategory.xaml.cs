using Microsoft.Extensions.Configuration;
using SIMA.Helper;
using SIMA.Infrastructure.Repositories;
using SIMA.Presentation.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using SIMA.Infrastructure.Repositories.Interfaces;
using SIMA.Domain.Models.Objects;
using SIMA.Domain.Models.Params;
using SIMA.Domain.Models.Intefaces;
using SIMA.Templates;
using SIMA.Domain.Models.Views;
using SIMA.Presentation.Repository;
using System.ComponentModel;

namespace SIMA.Presentation.Views
{
    /// <summary>
    /// Interaction logic for Wcategory.xaml
    /// </summary>
    public partial class Wcategory : Window
    {

        private IContextservices<Category, Category, CategoryParam> _service;
        private WindowServices<Category, Category, CategoryParam> _windowservices;

        private CancellationTokenSource _cts;
        private Category? _rowData;
        private CategoryViewModel _vm;
        private Util _util;
        private readonly Dictionary<int, string> columns_width;
        public WindowServices<Category, Category, CategoryParam> WindowServices { get => _windowservices; }
        public Wcategory()
        {
            InitializeComponent();
        }


 

        #region Util
        /// <summary>
        /// Initialize Window
        /// </summary>
        /// <param name="rowData"></param>
        private void InitializeWindow(Category? rowData = null)
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
        private async Task FillGrid(CategoryParam? param = null)
        {
            var prod = await _service.GetByFilter(param ?? _windowservices.activeFilters("Name"));
            _windowservices.Fill(prod);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="rowData"></param>
        /// <returns></returns>
        private Action EditControl(CategoryParam? rowData = null)
        {
            return () =>
            {

                //Set the Field Values from the grid
                txtIdCategory.Text = rowData?.IdCategory.ToString();
                txtName.Text = rowData?.Name.ToString();
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
                txtIdCategory.Text = string.Empty;
                txtName.Text = string.Empty;
            });

        }
        #endregion

        #region Events
        private async void btnsSave_Click(object sender, RoutedEventArgs e)
        {
            int? id = string.IsNullOrEmpty(txtIdCategory.Text) ? null : int.Parse(txtIdCategory.Text.ToString());
            string name = txtName.Text.ToString();
            try
            {
                progress.RunProgress(true, "Saving Stock...");
                await progress.DelayProgress();
                bool isset = await _service.Set(new Category
                {
                    IdCategory = id,
                    Name = name,
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
            await FillGrid();
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
                    Category rowData = (Category)btn.DataContext;
                    bool valid = await _service.Delete(rowData.IdCategory) > 0;
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
                Category rowData = (Category)btn.DataContext;
                _windowservices.EditMode = true;
                await _windowservices.Edit("40%", EditControl(new CategoryParam
                {
                    IdCategory = rowData.IdCategory,
                    Name = rowData.Name
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
            _vm = new CategoryViewModel(_page, _config);
            this.DataContext = _vm;
            _vm.ShowErrorFromModel += Vm_ShowErrorFromModel;
            _service = new CategoryServices(_config);
            _windowservices = new WindowServices<Category, Category, CategoryParam>(_config, _page, _service);
            _windowservices.SupressEventComboBox();


        }
        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {

            _windowservices.DataContext = _vm;
            _windowservices.PageControl = pageControl;
            _windowservices.ParentLovtextbox = null;
            _windowservices.ChildLovtextbox = null;
            _windowservices.TxtSearch = txtSearch;
            _windowservices.TxtResults = txtResults;
            _windowservices.IngorePredicate = null;
            _windowservices.Form = FormCategory;
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
            _windowservices.GridListView = _windowservices.GridListView ?? gridCategory;
            _windowservices.GridView = _windowservices.GridView ?? gridCellCategory;
            _windowservices.ColumnsWidth = new Dictionary<int, string>
            {
                { 0, "4%" },
                { 1, "86%" },
                { 2, "7%" },
            };
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
                await _windowservices.Edit("40%", EditControl(new CategoryParam
                {
                    IdCategory = _rowData.IdCategory,
                    Name = _rowData.Name
                }));
                _windowservices.FromMain = false;
                await FillGrid(new CategoryParam { IdCategory = _rowData.IdCategory });
            }

        }
        #endregion



    }
}


