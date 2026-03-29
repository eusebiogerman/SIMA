using Microsoft.Extensions.Configuration;
using SIMA.Domain.Models;
using SIMA.Infrastructure.Repositories;
using SIMA.Templates;
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
    internal class BrandViewModel : INotifyDataErrorInfo, INotifyPropertyChanged
    {
        private string _name;
        private decimal _price;
        private ObservableCollection<LovObject> _category;
        private ObservableCollection<LovObject> _product;
        private ObservableCollection<BrandView> _brand;
        private CategoryServices _categoryservices;
        private BrandServices _brandservices;
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
        public event Action<string> ShowErrorFromModel;
        #endregion

        #region Observable Collection Properties
        public ObservableCollection<LovObject> Category
        {
            get => _category;
            set { _category = value; OnPropertyChanged(nameof(Category)); }
        }
        public ObservableCollection<LovObject> Product
        {
            get => _product;
            set { _product = value; OnPropertyChanged(nameof(Product)); }
        }
        public ObservableCollection<BrandView> Brand
        {
            get => _brand;
            set { _brand = value; OnPropertyChanged(nameof(Brand)); }
        }
        public bool IsSupressed { get => _isSupressed; set => _isSupressed = value; }
        #endregion

        public BrandViewModel()
        {
            _page = new Paging();
            _categoryservices = new CategoryServices();
            FillLovCat();
            FillBrand();
        }
        public BrandViewModel(Paging page, IConfiguration config)
        {
            _config = config;
            _page = page ?? new Paging();
            _categoryservices = new CategoryServices(_config);
            _brandservices = new BrandServices(_config);
            FillLovCat();
            FillBrand();
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
        private async void FillLovCat()
        {
            try
            {
                IEnumerable<Category> cat = await _categoryservices.GetAll(_page);
                Category = new ObservableCollection<LovObject>(cat.Select((p) => new LovObject { Id = p.IdCategory, Value = p.Name }));
            }
            catch (Microsoft.Data.SqlClient.SqlException ex)
            {
                ShowErrorFromModel?.Invoke("PopupLov Category DataBase Error Failed");
            }
            catch (Exception)
            {
                ShowErrorFromModel?.Invoke("PopupLov Category System Error Failed");
            }
        }
        /// <summary>
        /// get the Brand data
        /// </summary>
        private async void FillBrand()
        {
            try
            {
                IEnumerable<BrandView> br = await _brandservices.GetViewAll(_page);
                Brand = new ObservableCollection<BrandView>(br);
            }
            catch (Microsoft.Data.SqlClient.SqlException ex)
            {
                ShowErrorFromModel?.Invoke("List Brand DataBase Error Failed");
            }
            catch (Exception)
            {
                ShowErrorFromModel?.Invoke("List Brand System Error Failed");
            }
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
