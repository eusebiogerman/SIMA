using Microsoft.Extensions.Configuration;
using SIMA.Domain.Models;
using SIMA.Infrastructure.Repositories;
using SIMA.Presentation.Views;
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
        private CategoryServices _categoryservices;

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
            InitializeModel(() => {});
        }
        public CategoryViewModel(Paging page, IConfiguration config) : base(page, config) 
        {
            InitializeModel(() => {});
        }


    }
}
