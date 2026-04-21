using Microsoft.Extensions.Configuration;
using SIMA.Helper;
using SIMA.Infrastructure.Repositories;
using SIMA.Presentation.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using SIMA.Infrastructure.Repositories.Interfaces;
using SIMA.Domain.Models.Objects;
using SIMA.Domain.Models.Params;
using SIMA.Presentation.Repository;
using System.ComponentModel;
using Microsoft.Data.SqlClient;
using WpfJEG.net6;

namespace SIMA.Presentation.Views
{
    /// <summary>
    /// Interaction logic for Wcategory.xaml
    /// </summary>
    public partial class Wcategory : Window
    {
        private readonly ICacheService _cache;
        private WindowServices<Category, Category, CategoryParam> _windowservices;
        private CancellationTokenSource _cts;
        private Category? _rowData;
        private CategoryViewModel _vm;
        private const string _editHeight = "40%";

        public WindowServices<Category, Category, CategoryParam> WindowServices { get => _windowservices; }

        public Wcategory()
        {
            InitializeComponent();
        }

        public Wcategory(ICacheService cache)
        {
            _cache = cache;
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
        /// <param name="rowData"></param>
        /// <returns></returns>
        private Action EditControl(CategoryParam? rowData = null)
        {
            return () =>
            {
                //Set the Field Values from the grid
                txtIdCategory.Text = rowData?.IdCategory.ToString();
                txtName.Text = rowData?.Name;
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
                _windowservices.InsertStatus("Saving Category...", BrushesStatus.DBProcess);
                await _windowservices.Status.DelayProgress();
                bool isset = await _windowservices.SetAsync(new Category
                {
                    IdCategory = id,
                    Name = name,
                });

                if (isset)
                {
                    _windowservices.InsertStatus("Category Sucessfully removed", BrushesStatus.DBProcess, false);
                    MessageBox.Show(this, "Category Sucessfully saved", "Save Category", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    await Clear();
                    await _windowservices.FillAsync(_windowservices.activeFilters("Name"));
                    _windowservices?.Status?.StopProgress();
                }
                else
                    _windowservices.CatchExceptionAndMsg(new Exception("Error Saving Category"));
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
            MessageBoxResult result = MessageBox.Show(this, "Confirm removing Category ?", "Remove Category", MessageBoxButton.YesNoCancel, MessageBoxImage.Warning);
            try
            {
                if (result == MessageBoxResult.Yes)
                {
                    _windowservices.InsertStatus("Deleting Category...", BrushesStatus.DBProcess);
                    await _windowservices.Status.DelayProgress();
                    Button btn = sender as Button;
                    Category rowData = (Category)btn.DataContext;
                    bool valid = await _windowservices.DeleteAsync(rowData.IdCategory) > 0;
                    if (valid)
                    {
                        _windowservices.InsertStatus("Category Sucessfully removed", BrushesStatus.DBProcess, false);
                        await _windowservices.FillAsync(_windowservices.activeFilters("Name"));
                        MessageBox.Show(this, "Category Succesfully removed", "Remove Category", MessageBoxButton.OK, MessageBoxImage.Information);
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
                Category rowData = (Category)btn.DataContext;
                _windowservices.EditMode = true;
                await _windowservices.Edit(_editHeight, EditControl(new CategoryParam
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
                  await _windowservices.FillAsync(_windowservices.activeFilters("Name"));
                }
                else
                    _windowservices.UpdatePaging();
            }
        }
        private async void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            _windowservices.Page.Limit = (int)_windowservices.PageControl.GetSelectedItemsPerPage();
             await _windowservices.FillAsync(_windowservices.activeFilters("Name"));
        }
        private async void parentCombobox_SelectionChanged(object sender, RoutedEventArgs e)
        {
          await _windowservices.FillAsync(_windowservices.activeFilters("Name"));
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
                    await _windowservices.FillAsync(_windowservices.activeFilters("Name"));
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
            _vm = new CategoryViewModel(_page, _config, _cache);
            this.DataContext = _vm;
            _vm.ShowErrorFromModel += Vm_ShowErrorFromModel;
            _windowservices = new WindowServices<Category, Category, CategoryParam>(_config, _page, new CategoryServices(_config, _cache));
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
            _windowservices.Status = statusbox;
            _windowservices.ObsrverStatus = new ObservableCollection<StatusItem>();
            _windowservices.Status.ItemsSource = _windowservices.ObsrverStatus;

            _windowservices.InsertStatus("Initializing Category Window.......", BrushesStatus.Progress);
            await _windowservices.Status.DelayProgress();
            _windowservices.Page.Offset = (int?)_windowservices.PageControl.GetSelectedItemsPerPage() ?? _windowservices.Page.DefaultOffset;
            _windowservices.InsertStatus("Retriving Category.......", BrushesStatus.Progress, false);
            await _windowservices.Page.FillLimitPageVal(_windowservices.PageControl);
            _windowservices.UpdatePaging();
            _windowservices.SupressEventComboBox(false);
            _windowservices.InsertStatus("Window ready.......", BrushesStatus.Progress);
            _windowservices.Status.StopProgress();
        }
        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            _windowservices.CurrentWindow = this;
            _windowservices.GridListView = _windowservices.GridListView ?? gridCategory;
            _windowservices.GridView = _windowservices.GridView ?? gridCellCategory;
            _windowservices.ColumnsWidth = new Dictionary<int, string>
            {
                { 0, "4%" },
                { 1, "84%" },
                { 2, "7%" },
            };
            _windowservices.StandarHeight = "51%";
            _windowservices.ResizeGrid();
        }
        private void Vm_ShowErrorFromModel(string mensaje)
        {
            _windowservices.CatchExceptionAndMsg(new Exception(mensaje), "Model Error");
        }
        private async void Window_ContentRendered(object sender, EventArgs e)
        {
            _windowservices.InsertStatus("Category Window Ready.......", BrushesStatus.Progress);
            if (_rowData != null)
            {
                _windowservices.SupressEventComboBox(false);
                await _windowservices.Edit(_editHeight, EditControl(new CategoryParam
                {
                    IdCategory = _rowData.IdCategory,
                    Name = _rowData.Name
                }));
                _windowservices.FromMain = false;
                await _windowservices.FillAsync(new CategoryParam { IdCategory = _rowData.IdCategory });
            }
            _windowservices.Status.StopProgress();
        }
        #endregion



    }
}


