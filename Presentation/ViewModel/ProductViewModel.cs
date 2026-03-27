using Microsoft.Extensions.Configuration;
using SIMA.Domain.Models;
using SIMA.Infrastructure.Repositories;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace SIMA.Presentation.Views
{
    internal class ProductViewModel : INotifyDataErrorInfo, INotifyPropertyChanged
    {
        private string _name;
        private decimal _price;
        private ObservableCollection<Category> _category;
        private ObservableCollection<ProductView> _product;
        private CategoryServices _categoryservices;
        private ProductServices _productservices;
        private IConfiguration _config;
        private Paging _page;
        private readonly Dictionary<string, List<string>> _errors = new();
        private bool _isSupressed;


        public string Name
        {
            get => _name; set
            {
                _name = value;
                ValidateProductName();
                OnPropertyChanged(nameof(Name));
            }
        }
        public decimal Price
        {
            get => _price; set
            {
                _price = value;
                ValidatePrice();
                OnPropertyChanged(nameof(Price));
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

        public ObservableCollection<ProductView> Product
        {
            get => _product;
            set { _product = value; OnPropertyChanged(nameof(Product)); }
        }

        public bool IsSupressed { get => _isSupressed; set => _isSupressed = value; }
        #endregion


        public ProductViewModel()
        {
            _page = new Paging();
            _categoryservices = new CategoryServices();
            FillCat();
        }


        public ProductViewModel(Paging page, IConfiguration config)
        {
            _config = config;
            _page = new Paging();
            _categoryservices = new CategoryServices(_config);
            _productservices = new ProductServices(_config);
            FillCat();
            FillProduct();
        }


        #region Validation Errors Methods
        private void ValidateProductName()
        {
            ClearErrors(nameof(Name));
            if (string.IsNullOrEmpty(Name))
                AddError(nameof(Name), "Product Name cannot be empty.");
        }
        private void ValidatePrice()
        {
            decimal dout;
            ClearErrors(nameof(Price));
            if (!decimal.TryParse(Price.ToString(), out dout))
                AddError(nameof(Price), "Invalid Price!!, Only Numbers accept .");
            if (Price == null)
                AddError(nameof(Price), "Price cannot be empty.");
            if (Price < 0)
                AddError(nameof(Price), "Invalid Price value.");


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
        /// get the Product data
        /// </summary>
        private async void FillProduct()
        {

            IEnumerable<ProductView> cat = await _productservices.GetViewAll(_page);
            Product = new ObservableCollection<ProductView>(cat);
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
