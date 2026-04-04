using Microsoft.Extensions.Configuration;
using SIMA.Infrastructure.Repositories;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using SIMA.Templates;
using SIMA.Infrastructure.Repositories.Interfaces;
using SIMA.Domain.Models.Objects;
using SIMA.Domain.Models.Views;
using SIMA.Domain.Models.Params;

namespace SIMA.Presentation.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        private IContextservices<StockProduct, StockProductView, StockProductParam> _stockservices;
        private IContextservices<Category, Category, CategoryParam> _categoryservices;
        private ObservableCollection<StockProductView> _stockproduct;
        private ObservableCollection<LovObject> _category;

        #region Observable Collection Property
        public ObservableCollection<StockProductView> StockProducts
        {
            get => _stockproduct;
            set { _stockproduct = value; OnPropertyChanged(nameof(StockProducts)); }
        }
        public ObservableCollection<LovObject> Category
        {
            get => _category;
            set { _category = value; OnPropertyChanged(nameof(Category)); }
        }
        #endregion

        public MainViewModel() : base()
        {

            InitializeModel(async () => {
                _stockservices = new StockProductServices(Config);
                _categoryservices = new CategoryServices(Config);
                await FillLovCat();
                await FillGridStock();
            });
        }
        public MainViewModel(IPaging page) : base(page)
        {


            InitializeModel(async () => {
                _stockservices = new StockProductServices(Config);
                _categoryservices = new CategoryServices(Config);
                await FillLovCat();
                await FillGridStock();
            });
        }
        public MainViewModel(IPaging page,IConfiguration config) : base(page, config) 
        {

            InitializeModel(async () => {
                _stockservices = new StockProductServices(Config);
                _categoryservices = new CategoryServices(Config);
                await FillLovCat();
                await FillGridStock();
            });

        }

        #region fill Observable Collection 
        /// <summary>
        /// get the Category data
        /// </summary>
        private async Task FillLovCat()
        {
            try
            {
                IEnumerable<Category> cat = await _categoryservices.GetAll(Page);
                Category = new ObservableCollection<LovObject>(cat.Select((p)=> new LovObject { Id = p.IdCategory,Value = p.Name }));
            }
            catch (Microsoft.Data.SqlClient.SqlException ex)
            {
                InvokeError("DataBase Error Failed");
            }
            catch (Exception)
            {
                InvokeError("System Error Failed");
            }

        }
        /// <summary>
        /// get the Stock data
        /// </summary>    
        private async Task FillGridStock()
        {
            try
            {
                IEnumerable<StockProductView> prod = await _stockservices.GetViewAll(Page);
                StockProducts = new ObservableCollection<StockProductView>(prod);
            }
            catch (Microsoft.Data.SqlClient.SqlException ex)
            {
                InvokeError("DataBase Error Failed");
            }
            catch (Exception)
            {
                InvokeError("System Error Failed");
            }

        }
        #endregion
    }
}
