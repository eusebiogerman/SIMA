using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace SIMA.Presentation.Views
{
    internal class ProducValidation : INotifyDataErrorInfo, INotifyPropertyChanged
    {
        private string _name;
        private decimal _price;
                private readonly Dictionary<string, List<string>> _errors = new();

        public string Name { get => _name; set
            {
                _name = value;
                ValidateProductName();
                OnPropertyChanged(nameof(Name));
            } 
        }
        public decimal Price { get => _price; set
            {
                _price = value;
                ValidatePrice();
                OnPropertyChanged(nameof(Price));
            }
        }


        public bool HasErrors => _errors.Any();
        public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged;
        public event PropertyChangedEventHandler? PropertyChanged;

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
            if (!decimal.TryParse(Price.ToString(),out dout))
                AddError(nameof(Price), "Invalid Price!!, Only Numbers accept .");
            if (Price == null)
                AddError(nameof(Price), "Price cannot be empty.");
            if (Price < 0)
                AddError(nameof(Price), "Invalid Price value.");


        }

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
        protected virtual void OnErrorsChanged([CallerMemberName] string? propertyName = null)
        {
            ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
        }

        public IEnumerable GetErrors(string? propertyName)
        {
            return _errors.GetValueOrDefault(propertyName ?? string.Empty, new List<string>());
        }
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

    }
}
