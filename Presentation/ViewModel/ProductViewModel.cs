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
  public class ProductViewModel : ViewModelBase
    {
        private string _name;
        private decimal _price;
        private ObservableCollection<LovObject> _category;
        private ObservableCollection<ProductView> _product;
        private CategoryServices _categoryservices;
        private ProductServices _productservices;

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

        public ProductViewModel():base()
        {
            InitializeModel(() =>
            {
                _categoryservices = new CategoryServices(Config);
                FillLovCat();
            });
        }
        public ProductViewModel(Paging page, IConfiguration config) : base(page, config) 
        {
            InitializeModel(() =>
            {
                _categoryservices = new CategoryServices(Config);
                _productservices = new ProductServices(Config);
                FillLovCat();
                FillProduct();
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
        private async void FillLovCat()
        {
            try
            {
                IEnumerable<Category> cat = await _categoryservices.GetAll(Page);
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
        /// get the Product data
        /// </summary>
        private async void FillProduct()
        {
            try
            {
                IEnumerable<ProductView> cat = await _productservices.GetViewAll(Page);
                Product = new ObservableCollection<ProductView>(cat);
            }
            catch (Microsoft.Data.SqlClient.SqlException ex)
            {
                InvokeError("List Product DataBase Error Failed");
            }
            catch (Exception)
            {
                InvokeError("List Product System Error Failed");
            }

        }
        #endregion




    }
}
