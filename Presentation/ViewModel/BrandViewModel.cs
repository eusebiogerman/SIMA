using Microsoft.Extensions.Configuration;
using SIMA.Domain.Models;
using SIMA.Helper;
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
  public class BrandViewModel : ViewModelBase
    {
        private string _name;
        private decimal _price;
        private ObservableCollection<LovObject> _category;
        private ObservableCollection<LovObject> _product;
        private ObservableCollection<BrandView> _brand;
        private CategoryServices _categoryservices;
        private BrandServices _brandservices;

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
        #endregion

        public BrandViewModel() :base()
        {

            InitializeModel(() => {
                _categoryservices = new CategoryServices(Config);
                _brandservices = new BrandServices(Config);
                FillLovCat();
                FillBrand();
            });
        }
        public BrandViewModel(Paging page, IConfiguration config):base()
        {

            InitializeModel(() => {
                _categoryservices = new CategoryServices(Config);
                _brandservices = new BrandServices(Config);
                FillLovCat();
                FillBrand();
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
                InvokeError("PopupLov Category DataBase Error Failed");
            }
            catch (Exception)
            {
                InvokeError("PopupLov Category System Error Failed");
            }
        }
        /// <summary>
        /// get the Brand data
        /// </summary>
        private async void FillBrand()
        {
            try
            {
                IEnumerable<BrandView> br = await _brandservices.GetViewAll(Page);
                Brand = new ObservableCollection<BrandView>(br);
            }
            catch (Microsoft.Data.SqlClient.SqlException ex)
            {
                InvokeError("List Brand DataBase Error Failed");
            }
            catch (Exception)
            {
                InvokeError("List Brand System Error Failed");
            }
        }
        #endregion




    }
}
