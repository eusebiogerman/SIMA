using SIMA.Domain.Models;
using SIMA.Infrastructure.Repositories;
using SIMA.Presentation.Views;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace SIMA.Presentation.ViewModel
{
    public class StockViewModel : INotifyDataErrorInfo, INotifyPropertyChanged
    {
        private decimal _stock;
        private ObservableCollection<Category> _category;
        private ObservableCollection<Product> _product;
        private CategoryServices _categoryservices;
        private ProductServices _Productervices;
        private Paging _page;
        private bool _isSupressed;
        private readonly Dictionary<string, List<string>> _errors = new();

        public decimal Stock
        {
            get => _stock; set
            {
                _stock = value;
                ValidateStock();
                OnPropertyChanged(nameof(Stock));
            }
        }

        #region Event Validation Properties
        public bool HasErrors => _errors.Any();
        public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged;
        public event PropertyChangedEventHandler? PropertyChanged;
        #endregion

        #region Observable Collection Properties
        public ObservableCollection<Category> Category
        {
            get => _category;
            set { _category = value; OnPropertyChanged(nameof(Category)); }
        }

        public ObservableCollection<Product> Products
        {
            get => _product;
            set { _product = value; OnPropertyChanged(nameof(Products)); }
        }

        public bool IsSupressed { get => _isSupressed; set => _isSupressed = value; }
        #endregion


        public StockViewModel()
        {
            _isSupressed = true;
            _page = new Paging();
            _categoryservices = new CategoryServices();
            _Productervices = new ProductServices();
            FillCat();
            //  FillProd();
            _isSupressed = false;
        }

        #region Validation Errors Methods
        private void ValidateStock()
        {
            decimal dout;
            ClearErrors(nameof(Stock));
            if (!decimal.TryParse(Stock.ToString(), out dout))
                AddError(nameof(Stock), "Invalid Stock!!, Only Numbers accept .");
            if (Stock == null)
                AddError(nameof(Stock), "Stock cannot be empty.");
            if (Stock < 0)
                AddError(nameof(Stock), "Invalid Stock value.");


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
        /// 
        /// </summary>
        private void FillProd()
        {
            _isSupressed = true;
            IEnumerable<Product> prod = _Productervices.DefeaultProductList();
            Products = new ObservableCollection<Product>(prod);
            _isSupressed = false;
        }
        #endregion

        #region Handler Errors Methods 
        private void AddError(string propertyName, string error)
        {
            if (!_errors.ContainsKey(propertyName))
                _errors[propertyName] = new List<string>();

            _errors[propertyName].Add(error);
            OnErrorsChanged(propertyName);
        }
        private void ClearErrors(string propertyName)
        {
            if (_errors.Remove(propertyName))
                OnErrorsChanged(propertyName);
        }
        public IEnumerable GetErrors(string? propertyName)
        {
            return _errors.GetValueOrDefault(propertyName ?? string.Empty, new List<string>());
        }
        #endregion

        #region View Model Events
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        protected virtual void OnErrorsChanged([CallerMemberName] string? propertyName = null)
        {
            ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
        }
        #endregion

    }
}
