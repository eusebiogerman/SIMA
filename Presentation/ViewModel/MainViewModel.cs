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

        public event PropertyChangedEventHandler? PropertyChanged;

        #region Observable Collection Property
        public ObservableCollection<StockProduct> StockProduct { get; set; }
        public ObservableCollection<Category> Category
        {
            get => _category;
            set { _category = value; OnPropertyChanged(nameof(Category)); }
        }
        #endregion

        public MainViewModel()
        {
            _page = new Paging();
            _stockservices = new StockProductServices();
            _categoryservices = new CategoryServices();
            FillStock();
            FillCat();


        }
        public MainViewModel(Paging page)
        {
            _page = page;
            _stockservices = new StockProductServices();
            _categoryservices = new CategoryServices();
            FillStock();
            FillCat();
        }

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

            IEnumerable<StockProduct> instock = await _stockservices.GetAll(_page);
            StockProduct = new ObservableCollection<StockProduct>(instock);
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
