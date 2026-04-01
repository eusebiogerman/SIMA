using Microsoft.Extensions.Configuration;
using SIMA.Domain.Models;
using SIMA.Infrastructure.Repositories;
using SIMA.Presentation.ViewModel;
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
 public class StockViewModel : ViewModelBase
    {
        private int? _stockcount;
        private ObservableCollection<StockProductView> _stockproduct;
        private ObservableCollection<LovObject> _product;
        private ObservableCollection<LovObject> _brand;
        private StockProductServices _stockservices;
        private ProductServices _productservices;

        public int? StockCount
        {
            get => _stockcount; set
            {
                _stockcount = value;
                ValidateStock();
                OnPropertyChanged(nameof(StockCount));
            }
        }

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
        #endregion

        public StockViewModel() : base()
        {
            InitializeModel(() =>
            {
                _stockservices = new StockProductServices(Config);
                _productservices = new ProductServices(Config);
                FillLovProd();
            });
        }
        public StockViewModel(Paging page, IConfiguration config) :base(page, config) 
        {
            InitializeModel(() =>
            {
                _stockservices = new StockProductServices(Config);
                _productservices = new ProductServices(Config);
                FillLovProd();
            });
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
                IEnumerable<ProductView> prod = await _productservices.GetViewAll(Page);
                Product = new ObservableCollection<LovObject>(prod.Select((p) => new LovObject { Id = p.IdProduct, Value = p.Name }));
            }
            catch (Microsoft.Data.SqlClient.SqlException ex)
            {
                InvokeError("PopupLov Product DataBase Error Failed");
            }
            catch (Exception)
            {
                InvokeError("PopupLov Product System Error Failed");
            }
        }

        #endregion

    }
}
