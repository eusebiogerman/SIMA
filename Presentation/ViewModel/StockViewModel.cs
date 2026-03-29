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
        internal class StockViewModel : INotifyDataErrorInfo, INotifyPropertyChanged
        {
        private int? _stockcount;
        private ObservableCollection<StockProductView> _stockproduct;
        private ObservableCollection<LovObject> _product;
        private ObservableCollection<LovObject> _brand;
        private StockProductServices _stockservices;
        private ProductServices _productservices;
        private IConfiguration _config;
        private Paging _page;
        private readonly Dictionary<string, List<string>> _errors = new();
        private bool _isSupressed;

        public int? StockCount
        {
            get => _stockcount; set
            {
                _stockcount = value;
                ValidateStock();
                OnPropertyChanged(nameof(StockCount));
            }
        }

        #region Event Validation Properties
        public bool HasErrors => _errors.Any();
        public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged;
        public event PropertyChangedEventHandler? PropertyChanged;
        public event Action<string> ShowErrorFromModel;
        #endregion

        #region Observable Collection Properties
        public ObservableCollection<LovObject> Product
        {
            get => _product;
            set { _product = value; OnPropertyChanged(nameof(Product)); }
        }
        public ObservableCollection<LovObject> Brand
        {
            get => _brand;
            set { _brand = value; OnPropertyChanged(nameof(Brand)); }
        }
        public ObservableCollection<StockProductView> StockProducts
        {
            get => _stockproduct;
            set { _stockproduct = value; OnPropertyChanged(nameof(Brand)); }
        }
        public bool IsSupressed { get => _isSupressed; set => _isSupressed = value; }
        #endregion

        public StockViewModel()
        {
           _page = new Paging();
           _stockservices = new StockProductServices();
           _productservices = new ProductServices();
            FillLovProd();
            FillStock();
        }
        public StockViewModel(Paging page, IConfiguration config)
        {
            _config = config;
            _page = page ?? new Paging();
            _stockservices = new StockProductServices(_config);
            _productservices = new ProductServices(_config);
            FillLovProd();
            FillStock();
        }

        #region Validation Errors Methods
        private void ValidateStock()
        {
            int dout;
            ClearErrors(nameof(StockCount));
            if (!int.TryParse(StockCount.ToString(), out dout))
                AddError(nameof(StockCount), "Invalid Price!!, Only Numbers accept .");
            if (StockCount == null)
                AddError(nameof(StockCount), "Price cannot be empty.");
            if (StockCount < 0)
                AddError(nameof(StockCount), "Invalid Price value.");


        }
        #endregion

        #region fill Observable Collection 
        /// <summary>
        /// get the Category data
        /// </summary>
        private async void FillLovProd()
        {
            try
            {
                IEnumerable<ProductView> prod = await _productservices.GetViewAll(_page);
                Product = new ObservableCollection<LovObject>(prod.Select((p) => new LovObject { Id = p.IdProduct, Value = p.Name }));
            }
            catch (Microsoft.Data.SqlClient.SqlException ex)
            {
                ShowErrorFromModel?.Invoke("PopupLov Product DataBase Error Failed");
            }
            catch (Exception)
            {
                ShowErrorFromModel?.Invoke("PopupLov Product System Error Failed");
            }
        }

        /// <summary>
        /// get the Brand data
        /// </summary>
        private async void FillStock()
        {

            IEnumerable<StockProductView> st = await _stockservices.GetViewAll(_page);
            StockProducts = new ObservableCollection<StockProductView>(st);
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
