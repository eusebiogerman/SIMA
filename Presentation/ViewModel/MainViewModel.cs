using Microsoft.Extensions.Configuration;
using SIMA.Infrastructure.Repositories;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using SIMA.Infrastructure.Repositories.Interfaces;
using SIMA.Domain.Models.Objects;
using SIMA.Domain.Models.Views;
using SIMA.Domain.Models.Params;
using WpfJEG.net6;

namespace SIMA.Presentation.ViewModel
{
    public class MenuItems
    {
        public MenuSima ItemMenu { get; set; }
        public ObservableCollection<MenuItems> Items { get; } = new();
    }

    public class MainViewModel : ViewModelBase
    {
        private IContextservices<StockProduct, StockProductView, StockProductParam> _stockservices;
        private IContextservices<Category, Category, CategoryParam> _categoryservices;
        private IContextservicesMenu<MenuSima> _menuservices;

        private ObservableCollection<StockProductView> _stockproduct;
        private ObservableCollection<LovObject> _category;
        private ObservableCollection<MenuItems> _menutree;

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
        public ObservableCollection<MenuItems> MenuTree
        {
            get => _menutree;
            set { _menutree = value; OnPropertyChanged(nameof(MenuTree)); }
        }
        #endregion

        public MainViewModel(ICacheService cache) : base(cache)
        {
               
               InitializeModel(async () => {
                _stockservices = new StockProductServices(Config, cache);
                _categoryservices = new CategoryServices(Config, cache);
                _menuservices = new MenuServices(Config, cache);
                await FillMenu();
                await FillLovCat();
                await FillGridStock();
            });
        }
        public MainViewModel(IPaging page, ICacheService cache) : base(page, cache)
        {

            InitializeModel(async () =>
            {
                _stockservices = new StockProductServices(Config, cache);
                _categoryservices = new CategoryServices(Config, cache);
                _menuservices = new MenuServices(Config, cache);
                await FillMenu();
                await FillLovCat();
                await FillGridStock();
            });
        }
        public MainViewModel(IPaging page,IConfiguration config, ICacheService cache) : base(page, config, cache) 
        {
            

            InitializeModel(async () => {
                _stockservices = new StockProductServices(Config, cache);
                _categoryservices = new CategoryServices(Config, cache);
                _menuservices = new MenuServices(Config, cache);
                await FillMenu();
                await FillLovCat();
                await FillGridStock();
            });

        }

        #region fill Observable Collection
        /// <summary>
        /// Fill the Menu Tree
        /// </summary>
        private async Task FillMenu()
        {
            try
            {
                IEnumerable<MenuSima> menusima = await _menuservices.GetMenu(0);
                var root = new MenuItems { ItemMenu = new MenuSima { AppName = "Menu", IdParent = 0 } };
                MenuItems? child = null;
                foreach (var item in menusima)
                {
                    if (item.IdChild == null)
                    {
                        if(child != null)
                        {
                            root.Items.Add(child);
                        }
                        child = null;
                        child = new MenuItems { ItemMenu = item };
                        child.ItemMenu.AppName = child.ItemMenu.ParentName;
                    }
                    else
                        child?.Items?.Add(new MenuItems { ItemMenu = item });
                }
                root.Items.Add(child);
                MenuTree = new ObservableCollection<MenuItems>();
                MenuTree.Add(root);
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
