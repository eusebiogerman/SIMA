using Microsoft.Extensions.Configuration;
using SIMA.Domain.Models.Objects;
using SIMA.Domain.Models.Params;
using SIMA.Domain.Models.Views;
using SIMA.Infrastructure.Repositories;
using SIMA.Infrastructure.Repositories.Interfaces;
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
    public class ProductViewModel : ViewModelBase
    {
        private string _name;
        private decimal _price;
        private ObservableCollection<LovObject> _category;
        private ObservableCollection<ProductView> _product;
        private IContextservices<Category, Category, CategoryParam> _categoryservices;
        private IContextservices<Product, ProductView, ProductParam> _productservices;


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

        #region Observable Collection Properties
        public ObservableCollection<LovObject> Category
        {
            get => _category;
            set { _category = value; OnPropertyChanged(nameof(Category)); }
        }
        public ObservableCollection<ProductView> Product
        {
            get => _product;
            set { _product = value; OnPropertyChanged(nameof(Product)); }
        }
        #endregion

        public ProductViewModel(ICacheService cache):base(cache)
        {
            InitializeModel(async () =>
            {
                _categoryservices = new CategoryServices(Config, cache);
                _productservices = new ProductServices(Config, cache);
                await FillLovCat();
                await FillProd();
            });
        }
        public ProductViewModel(IPaging page, IConfiguration config, ICacheService cache) : base(page, config, cache) 
        {
            InitializeModel(async () =>
            {
                _categoryservices = new CategoryServices(Config, cache);
                _productservices = new ProductServices(Config, cache);
                await FillLovCat();
                await FillProd();
            });
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
        private async Task FillLovCat()
        {
            try
            {
                IEnumerable<Category> cat = await _categoryservices.GetAll(new Paging { Offset = 0,Limit = 2000});
                Category = new ObservableCollection<LovObject>(cat.Select((p) => new LovObject { Id = p.IdCategory, Value = p.Name }));
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
        /// <summary>
        /// get the Category data
        /// </summary>
        private async Task FillProd()
        {
            try
            {
                IEnumerable<ProductView> prod = await _productservices.GetViewAll(new Paging { Offset = 0, Limit = 2000 });
                Product = new ObservableCollection<ProductView>(prod.Where(p => p.IdProduct != null));
            }
            catch (Microsoft.Data.SqlClient.SqlException ex)
            {
                InvokeError("Grid DataBase Error Failed");
            }
            catch (Exception)
            {
                InvokeError("Grid System Error Failed");
            }

        }
        #endregion

    }
}
