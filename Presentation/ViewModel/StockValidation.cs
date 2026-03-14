using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace SIMA.Presentation.ViewModel
{
    public class StockValidation
    {
        private int? _stock;
        private readonly Dictionary<string, List<string>> _errors = new();

        public int? Stock
        {
            get => _stock; set
            {
                _stock = value;
                ValidateStock();
                OnPropertyChanged(nameof(Stock));
            }
        }

        public bool HasErrors => _errors.Any();
        public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged;
        public event PropertyChangedEventHandler? PropertyChanged;

        private void ValidateStock()
        {
            int dout;
            ClearErrors(nameof(Stock));
            if (!int.TryParse(Stock.ToString(), out dout))
                AddError(nameof(Stock), "Invalid Stock!!, Only Numbers accept .");
            if (Stock == null)
                AddError(nameof(Stock), "Stock cannot be empty.");
            if (Stock < 0)
                AddError(nameof(Stock), "Invalid Stock value.");


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
