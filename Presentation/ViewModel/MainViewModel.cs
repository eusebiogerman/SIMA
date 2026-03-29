using Microsoft.Extensions.Configuration;
using SIMA.Domain.Models;
using SIMA.Infrastructure.Repositories;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using SIMA.Templates;

namespace SIMA.Presentation.ViewModel
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private StockProductServices _stockservices;
        private CategoryServices _categoryservices;
        private ObservableCollection<LovObject> _category;

        private Paging _page;
        private IConfiguration _config;
        private bool _isSupressed;

        #region Event and Validation Properties
        public event PropertyChangedEventHandler? PropertyChanged;
        public event Action<string> ShowErrorFromModel;
        #endregion

        #region Observable Collection Property
        public ObservableCollection<StockProductView> StockProducts { get; set; }
        public ObservableCollection<LovObject> Category
        {
            get => _category;
            set { _category = value; OnPropertyChanged(nameof(Category)); }
        }
        public bool IsSupressed { get => _isSupressed; set => _isSupressed = value; }
        #endregion

        public MainViewModel()
        {
            InitializeModel(new Paging(),new ConfigurationManager());
        }
        public MainViewModel(Paging page)
        {
            InitializeModel(page,new ConfigurationManager());
        }
        public MainViewModel(Paging page,IConfiguration config)
        {
            InitializeModel(page,config);
        }

        #region Util
        private void InitializeModel(Paging page, IConfiguration config) {

            _isSupressed = true;
            _page = page;
            _config = config;
            _stockservices = new StockProductServices(_config);
            _categoryservices = new CategoryServices(_config);
            FillLovCat();
            FillStock();
            _isSupressed = false;

        }
        #endregion

        #region fill Observable Collection 
        /// <summary>
        /// get the Category data
        /// </summary>
        private async void FillLovCat()
        {
            try
            {
                IEnumerable<Category> cat = await _categoryservices.GetAll(_page);
                Category = new ObservableCollection<LovObject>(cat.Select((p)=> new LovObject { Id = p.IdCategory,Value = p.Name }));
            }
            catch (Microsoft.Data.SqlClient.SqlException ex)
            {
                ShowErrorFromModel?.Invoke("DataBase Error Failed");
            }
            catch (Exception)
            {
                ShowErrorFromModel?.Invoke("System Error Failed");
            }

        }
        /// <summary>
        /// Get the Stock data given the paging configuration
        /// </summary>
        private async void FillStock()
        {
            try
            {
                IEnumerable<StockProductView> instock = await _stockservices.GetViewAll(_page);
                StockProducts = new ObservableCollection<StockProductView>(instock);
            }
            catch (Microsoft.Data.SqlClient.SqlException ex)
            {
                ShowErrorFromModel?.Invoke("DataBase Error Failed");
            }
            catch (Exception)
            {
                ShowErrorFromModel?.Invoke("System Error Failed");
            }
        }
        #endregion

        #region View Model Events
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
        #endregion


    }
}
