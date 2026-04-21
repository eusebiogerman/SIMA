using Microsoft.Extensions.Configuration;
using SIMA.Domain.Models.Views;
using SIMA.Infrastructure.Repositories;
using SIMA.Infrastructure.Repositories.Interfaces;
using SIMA.Presentation.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using WpfJEG.net6;

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
            set { _stockproduct = value; OnPropertyChanged(nameof(StockProducts)); }
        }
        #endregion

        public StockViewModel(ICacheService cache,bool fillstock = true) : base(cache)
        {
            InitializeModel(async () =>
            {
                _stockservices = new StockProductServices(Config, cache);
                _productservices = new ProductServices(Config,cache);
                await FillLovProd();
                if (fillstock)
                    await FillStockProd();
            });
        }
        public StockViewModel(IPaging page, IConfiguration config, ICacheService cache, bool fillstock = true) :base(page, config, cache) 
        {
            InitializeModel(async () =>
            {
                _stockservices = new StockProductServices(Config, cache);
                _productservices = new ProductServices(Config, cache);
                await FillLovProd();
                if (fillstock)
                    await FillStockProd();
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
        private async Task FillLovProd()
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

        /// <summary>
        /// get the Stock data
        /// </summary>
        private async Task FillStockProd()
        {
            try
            {
                IEnumerable<StockProductView> prod = await _stockservices.GetViewAll(new Paging { Offset = 0, Limit = 2000 });
                StockProducts = new ObservableCollection<StockProductView>(prod.Where(p => p.IdStock != null));
            }
            catch (Microsoft.Data.SqlClient.SqlException ex)
            {
                InvokeError("PopupLov DataBase Error Failed");
            }
            catch (Exception)
            {
                InvokeError("PopupLov System Error Failed");
            }
        }
        #endregion

    }
}
