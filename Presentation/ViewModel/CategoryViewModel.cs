using Microsoft.Extensions.Configuration;
using SIMA.Domain.Models.Objects;
using SIMA.Domain.Models.Params;
using SIMA.Infrastructure.Repositories;
using SIMA.Infrastructure.Repositories.Interfaces;
using SIMA.Presentation.Views;
using SIMA.Templates;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace SIMA.Presentation.ViewModel
{
    public class CategoryViewModel : ViewModelBase
    {
        private ObservableCollection<Category> _category;
        private ObservableCollection<Category> _categoryCombo;
        private IContextservices<Category, Category, CategoryParam> _categoryservices;


        #region Observable Collection Properties
        public ObservableCollection<Category> Categorys
        {
            get => _category;
            set { _category = value; OnPropertyChanged(nameof(Categorys)); }
        }
        public ObservableCollection<Category> CategoryCombo
        {
            get => _categoryCombo;
            set { _categoryCombo = value; OnPropertyChanged(nameof(CategoryCombo)); }
        }
        #endregion

        public CategoryViewModel() : base()
        {
            InitializeModel(async () => {

                _categoryservices = new CategoryServices(Config);
                await FillCat();
            });
        }
        public CategoryViewModel(IPaging page, IConfiguration config) : base(page, config) 
        {
            InitializeModel(async () => {

                _categoryservices = new CategoryServices(Config);
                await FillCat();
            });
        }

        #region fill Observable Collection 
        /// <summary>
        /// get the Category data
        /// </summary>
        private async Task FillCat()
        {
            try
            {
                IEnumerable<Category> cat = await _categoryservices.GetAll(new Paging { Offset = 0, Limit = 2000 });
                Categorys = new ObservableCollection<Category>(cat.Where(p=>p.IdCategory !=null));
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
