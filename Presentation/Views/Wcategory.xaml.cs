using Microsoft.Extensions.Configuration;
using SIMA.Domain.Models;
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

namespace SIMA.Presentation.Views
{
    /// <summary>
    /// Interaction logic for Wcategory.xaml
    /// </summary>
    public partial class Wcategory : Window, IUtilServices<Category, CategoryParam>, IUtil
    {
        private CategoryServices _categoryservices;
        private Paging _page;
        private Util _util;
        private CancellationTokenSource _cts;
        private readonly IConfiguration _config;
        private bool _isloaded;

        public Wcategory()
        {
            InitializeComponent();
            FormCategory.Visibility = Visibility.Hidden;
            FormCategory.Height = 0;

            _page = new Paging();
            _util = new Util();
            _config = _util.CustomConfiguration();
            var Vm = new CategoryViewModel(_page, _config);
            this.DataContext = Vm;
            Vm.ShowErrorFromModel += Vm_ShowErrorFromModel;
           _categoryservices = new CategoryServices(_config);

        }

        #region Util
        /// <summary>
        /// 
        /// </summary>
        /// <param name="val"></param>
        public void SupressEventComboBox(bool val = true)
        {
            _isloaded = val;
            if (this.DataContext != null)
                ((CategoryViewModel)this.DataContext).IsSupressed = val;
        }
        /// <summary>
        /// Return and Update the message of the render Category result
        /// <param name="total"></param>
        /// <returns></returns>
        public string getResultMsgAsync(int total)
        {
            pageControl.TotalFound = total;
            return $"📊 Show {total} products found";
        }
        /// <summary>
        /// Update the Paging Labels given the cuurent Offset and Limit Values
        /// </summary>
        /// <param name="total"></param>
        public void pagingLabels(int total)
        {
            if (_page.isvalidPaging())
            {
                pageControl.TotalFound = total;
                pageControl.PageNumber = _page.Pagenumber;
            }
        }
        /// <summary>
        /// Control the Paging Previous and Next Page Number,Offset and Limit 
        /// </summary>
        /// <param name="direction"></param>
        public void NavigationGrid(Paging.DIRECTION direction)
        {
            throw new NotImplementedException();
        }
        /// <summary>
        ///  Restore Initial set of Category and Stock  
        /// </summary>
        public void ClearFilters()
        {
            txtIdCategory.Text = string.Empty;
            txtName.Text = string.Empty;
            txtSearch.Text = string.Empty;
            FormCategory.Visibility = Visibility.Hidden;
            FormCategory.Height = 0;
            ResizeGrid("60%");
        }
        /// <summary>
        /// Update the Total result of the rows and update the labels on the grid 
        /// </summary>
        /// <param name="total"></param>
        public async void UpdatePaging(int total = 0)
        {
            int intotal = (total == 0) ? await _categoryservices.GetTotalFound(activeFilters()) : total;
            _page.parsePageData(intotal);
            txtResults.Text = getResultMsgAsync(intotal);
            pagingLabels(intotal);
        }
        /// <summary>
        /// Managment of Responsive Windows
        /// </summary>
        /// <param name="gridheight">Size Height = Only Numeric string percent {Size}% or Size}</param>
        public void ResizeGrid(string gridheight)
        {
            Dictionary<int, string> columns_width = new Dictionary<int, string>();
            columns_width.Add(0, "4%");
            columns_width.Add(1, "86%");
            columns_width.Add(2, "7%");
            _util.ResponsiveListViewHeight(gridCategory, this.ActualHeight, gridheight);
            _util.ResponsiveGridWidth(gridCellCategory, this.ActualWidth, columns_width);
        }
        #endregion

        #region Filling Methods
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public Category Result()
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// Returns the object CategoryParam with passing values of the Active Filter controls 
        /// </summary>
        /// <returns></returns>
        public CategoryParam activeFilters()
        {
            return new CategoryParam
            {
                Name = txtSearch.Text,
                offset = _page.Offset,
                limit = _page.Limit
            };

        }
        /// <summary>
        /// Fill the Category ComboBox
        /// </summary>
        public async void FillCombobox(int? id = null)
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// Fill the Gridview
        /// </summary>
        /// <exception cref="NotImplementedException"></exception>
        public void Fill()
        {
            try
            {
                progress.SetLoadingStateDataBase(async () =>
                {
                    IEnumerable<Category> cat = await _categoryservices.GetAll(_page);
                    gridCategory.ItemsSource = cat.Where(p => p.IdCategory != null);
                    UpdatePaging();
                }, _config, true, "Loading Category...");
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
        /// <summary>
        /// Filter the GridView given the activeFilters() : function
        /// </summary>
        public async void Filter()
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// Filter the GridView given the Stock Category param
        /// </summary>
        /// <param name="param"></param>
        public async void Filter(CategoryParam param)
        {
            try
            {
                progress.SetLoadingStateDataBase(async () =>
                {
                    IEnumerable<Category> cat = await _categoryservices.GetByFilter(param);
                    gridCategory.ItemsSource = cat.Where(p => p.IdCategory != null);
                    UpdatePaging();
                }, _config, true, "Loading Category...");
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
        /// <summary>
        /// Filter the GridView given the Search text description
        /// </summary>
        /// <param name="param"></param>
        public async void FilterbyText(CategoryParam param)
        {
            try
            {
                
                IEnumerable<Category> cat = await _categoryservices.GetByFilter(param);
                gridCategory.ItemsSource = cat.Where(p => p.IdCategory != null);
                UpdatePaging();
               

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
        /// <summary>
        /// Fill the Page Limit Values
        /// </summary>
        public async void FillLimitPageVal()
        {
            IEnumerable<string> lim = await _page.GetLimitPaging();
            pageControl.SetItemsPerPageSource(lim);
            var selected = pageControl.GetSelectedItemsPerPage();
            var cmblimit = selected != null
                ? int.Parse(selected.ToString())
                : _page.DefaulLimit;
            _page.Limit = cmblimit;
        }
        /// <summary>
        /// Open the Edit Form for the Stock select in th gridview
        /// </summary>
        /// <param name="param"></param>
        public void Edit(CategoryParam param)
        {
            FormCategory.Visibility = Visibility.Visible;
            FormCategory.Height = Double.NaN;
            ResizeGrid("40%");

            //Set the Field Values from the grid
            txtIdCategory.Text = param.IdCategory.ToString();
            txtName.Text = param.Name;
        }
        #endregion

        #region Events
        private  void btnsSaveCategory_Click(object sender, RoutedEventArgs e)
        {
            
            int? id = string.IsNullOrEmpty(txtIdCategory.Text) ? null : int.Parse(txtIdCategory.Text);
            bool isset = false;
            progress.SetLoadingStateDataBase(async () =>
            {
                isset = await _categoryservices.Set(new Category
                {
                    IdCategory = id,
                    Name = txtName.Text
                });
            }, _config, true, "Saving Category...");

            try
            {
                if (isset)
                {
                    MessageBox.Show(this, "Category Sucessfully saved", "Save Category", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    ClearFilters();
                    Filter(activeFilters());
                }
                else
                {
                    MessageBox.Show(this, "Error Saving Category", "Save Category", MessageBoxButton.OK, MessageBoxImage.Exclamation);
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
        private void btnsClear_Click(object sender, RoutedEventArgs e)
        {
            ClearFilters();
        }
        private void btnsClose_Click(object sender, RoutedEventArgs e)
        {
            ClearFilters();
            this.Close();
        }
        private void btnUpdate_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Button btn = sender as Button;
                Category rowData = (Category)btn.DataContext;
                Edit(new CategoryParam { IdCategory = rowData.IdCategory, Name = rowData.Name });
            }
            catch (Exception)
            {
               
            }
        }
        private void btnRemove_Click(object sender, RoutedEventArgs e)
        {

        }
        private void btnNewStock_Click(object sender, RoutedEventArgs e)
        {
            Edit(new CategoryParam { IdCategory = null, Name = null });
        }
        private async void txtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            _cts?.Cancel();
            _cts = new CancellationTokenSource();

            try
            {
                await Task.Delay(300, _cts.Token);
                FilterbyText(activeFilters());
            }
            catch (TaskCanceledException)
            {
                // Ignorar: se canceló porque el usuario siguió escribiendo
            }
        }
        private void PagePrevious_Click(object sender, RoutedEventArgs e)
        {
            NavigationGrid(Paging.DIRECTION.previous);
        }
        private void PageNext_Click(object sender, RoutedEventArgs e)
        {
            NavigationGrid(Paging.DIRECTION.next);
        }
        private void PageNavigation_SelectionChanged(object sender, RoutedEventArgs e)
        {
            if (!_isloaded)
            {
                _page.Limit = (int)pageControl.GetSelectedItemsPerPage();
                Filter(activeFilters());
                UpdatePaging();
            }
        }
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            this.Visibility = Visibility.Hidden;

        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            
            _page.Offset = (int?)pageControl.GetSelectedItemsPerPage() ?? _page.DefaulOffset;
            this.SupressEventComboBox();
            FillLimitPageVal();
            UpdatePaging();
            Fill();
            this.SupressEventComboBox(false);
        }
        private void Window_Initialized(object sender, EventArgs e)
        {
            this.SupressEventComboBox();
        }
        private void Vm_ShowErrorFromModel(string mensaje)
        {
           
            MessageBox.Show(this, mensaje, "Model Error", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
           ResizeGrid("60%");
        }
        #endregion


    }
}


