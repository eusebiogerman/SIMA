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

namespace SIMA.Presentation.ViewModel
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private StockProductServices _stockservices;
        private CategoryServices _categoryservices;
        private ObservableCollection<Category> _category;
        private Paging _page;
        private IConfiguration _config;
        private bool _isSupressed;

        public event PropertyChangedEventHandler? PropertyChanged;

        #region Observable Collection Property
        public ObservableCollection<StockProductView> StockProducts { get; set; }
        public ObservableCollection<Category> Category
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
            _categoryservices = new CategoryServices();
            FillStock();
            FillCat();
            _isSupressed = false;

        }

        #endregion

        #region fill Observable Collection 
        /// <summary>
        /// get the Category data
        /// </summary>
        private async void FillCat()
        {

            IEnumerable<Category> cat = await _categoryservices.GetAll(_page);
            Category = new ObservableCollection<Category>(cat);
        }
        /// <summary>
        /// Get the Stock data given the paging configuration
        /// </summary>
        private async void FillStock()
        {
            IEnumerable<StockProductView> instock = await _stockservices.GetAlltest(_page);
            StockProducts = new ObservableCollection<StockProductView>(instock);
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
