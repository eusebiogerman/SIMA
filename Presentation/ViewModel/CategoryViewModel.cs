using Microsoft.Extensions.Configuration;
using SIMA.Domain.Models;
using SIMA.Infrastructure.Repositories;
using SIMA.Presentation.Views;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace SIMA.Presentation.ViewModel
{
    public class CategoryViewModel : INotifyDataErrorInfo, INotifyPropertyChanged
    {
        private ObservableCollection<Category> _category;
        private ObservableCollection<Category> _categoryCombo;
        private CategoryServices _categoryservices;
        private Paging _page;
        private IConfiguration _config;
        private bool _isSupressed;
        private readonly Dictionary<string, List<string>> _errors = new();


        #region Event Validation Properties
        public bool HasErrors => _errors.Any();
        public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged;
        public event PropertyChangedEventHandler? PropertyChanged;
        #endregion

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


        public bool IsSupressed { get => _isSupressed; set => _isSupressed = value; }
        #endregion


        public CategoryViewModel()
        {
            InitializeModel(new Paging(), new ConfigurationManager());
        }

        public CategoryViewModel(Paging page, IConfiguration config)
        {
            InitializeModel(page, config);
        }

        #region Util
        private void InitializeModel(Paging page, IConfiguration config)
        {

            _isSupressed = true;
            _page = page;
            _config = config;
            _categoryservices = new CategoryServices(_config);
            FillCat();
            FillCatCombo();
            _isSupressed = false;

        }

        #endregion



        #region fill Observable Collection 

        /// <summary>
        /// get the Category data
        /// </summary>
        private async void FillCatCombo()
        {
            IEnumerable<Category> cat = await _categoryservices.GetAll(_page);
            CategoryCombo = new ObservableCollection<Category>(cat);

        }

        /// <summary>
        /// get the Category data
        /// </summary>
        private async void FillCat()
        {
            IEnumerable<Category> cat = await _categoryservices.GetAll(_page);
            Categorys = new ObservableCollection<Category>(cat.Where(p => p.IdCategory != null));

        }
        #endregion

        #region Handler Errors Methods 
        private void AddError(string propertyName, string error)
        {
            if (!_errors.ContainsKey(propertyName))
                _errors[propertyName] = new List<string>();

            _errors[propertyName].Add(error);
            OnErrorsChanged(propertyName);
        }
        private void ClearErrors(string propertyName)
        {
            if (_errors.Remove(propertyName))
                OnErrorsChanged(propertyName);
        }
        public IEnumerable GetErrors(string? propertyName)
        {
            return _errors.GetValueOrDefault(propertyName ?? string.Empty, new List<string>());
        }
        #endregion

        #region View Model Events
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        protected virtual void OnErrorsChanged([CallerMemberName] string? propertyName = null)
        {
            ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
        }
        #endregion


    }
}
