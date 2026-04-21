using Microsoft.Extensions.Configuration;
using SIMA.Domain.Models.Objects;
using SIMA.Domain.Models.Params;
using SIMA.Domain.Models.Views;
using SIMA.Infrastructure.Repositories;
using SIMA.Infrastructure.Repositories.Interfaces;
using SIMA.Presentation.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media;
using WpfJEG.net6;

namespace SIMA.Presentation.Views
{
    public class BrandViewModel : ViewModelBase
    {

        private IContextservices<Category, Category, CategoryParam> _categoryservices;
        private IContextservices<Brand, BrandView, BrandParam> _brandservices;

        private ObservableCollection<LovObject> _category;
        private ObservableCollection<LovObject> _product;
        private ObservableCollection<BrandView> _brand;
        private string _name;
        private decimal _price;

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

        public BrandViewModel(ICacheService cache) :base(cache)
        {

            InitializeModel(async () => {
                _categoryservices = new CategoryServices(Config, cache);
                _brandservices = new BrandServices(Config,cache);
                await FillLovCat();
                await FillBrand();
            });
        }
        public BrandViewModel(IPaging page, IConfiguration config, ICacheService cache):base(page,config,cache)
        {

            InitializeModel(async () => {
                _categoryservices = new CategoryServices(Config, cache);
                _brandservices = new BrandServices(Config, cache);
                await FillLovCat();
                await FillBrand();
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
                AddStatusLog("Getting Category data......\n", Brushes.AliceBlue);
                IEnumerable<Category> cat = await _categoryservices.GetAll(new Paging { Offset = 0, Limit = 2000 });
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
        private async Task FillBrand()
        {
            try
            {
                AddStatusLog( "Getting Brand data......\n",Brushes.AliceBlue);
                IEnumerable<BrandView> prod = await _brandservices.GetViewAll(new Paging { Offset = 0, Limit = 2000 });
                Brand = new ObservableCollection<BrandView>(prod.Where(p => p.IdBrand != null));
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
