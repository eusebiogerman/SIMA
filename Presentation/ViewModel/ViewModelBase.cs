using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using SIMA.Helper;
using SIMA.Infrastructure.Repositories;
using SIMA.Infrastructure.Repositories.Interfaces;
using SIMA.Presentation.Interfaces;
using SIMA.Presentation.Repository;
using SIMA.Templates;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Media;

namespace SIMA.Presentation.ViewModel
{
    public abstract class ViewModelBase : INotifyDataErrorInfo, INotifyPropertyChanged,IViewModel
    {
        
        private IConfiguration _config;
        private IPaging _page;
        private ICacheService _cache;

        private ObservableCollection<StatusItem> _obsrverstatus;
        private readonly Dictionary<string, List<string>> _errors = new();
        private object _passingParameter;
        private bool _isSupressed;
        private bool _isBusy;
        private string _message;
        private int _delay;

        #region Event Validation Properties
        public bool HasErrors => _errors.Any();
        public bool IsSupressed { get => _isSupressed; set => _isSupressed = value; }
        public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged;
        public event PropertyChangedEventHandler? PropertyChanged;
        public event Action<string> ShowErrorFromModel;
        public event Action<object> ObjectEventFromModel;
        public event Action<Action> ActionEventFromModel;
        public IEnumerable GetErrors(string? propertyName)
        {
            return _errors.GetValueOrDefault(propertyName ?? string.Empty, new List<string>());
        }
        public IConfiguration Config { get => _config; set => _config = value; }
        public IPaging Page { get => _page; set => _page = value; }
        public int Delay { set => _delay = value; }
        public object PassingParameter { get => _passingParameter; set => _passingParameter = value; }
        #endregion

        #region Observable Property
        public bool IsBusy
        {
            get => _isBusy;
            set => SetProperty(ref _isBusy, value, nameof(IsBusy));
        }
        public string Message
        {
            get => _message;
            set => SetProperty(ref _message, value, nameof(Message));
        }
        public ObservableCollection<StatusItem> ObsrverStatus
        {
            get => _obsrverstatus;
            set { _obsrverstatus = value; OnPropertyChanged(nameof(ObsrverStatus)); }
        }
        #endregion

        protected ViewModelBase(ICacheService cache)
        {
            InitBase(cache:cache);
        }
        protected ViewModelBase(IPaging page, ICacheService cache)
        {
            InitBase(page:page, cache: cache);
        }
        protected ViewModelBase(IPaging page,IConfiguration config, ICacheService cache)
        {
            InitBase(page: page, config: config, cache: cache);
        }

        #region View Model Events
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        protected virtual void OnErrorsChanged([CallerMemberName] string? propertyName = null)
        {
            ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
        }
        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
                return false;

            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
        #endregion

        #region Util Methods
        private void InitBase(IPaging? page = null, IConfiguration? config=null, ICacheService? cache = null)
        {
            IsBusy = false;
            _page = page ?? new Paging();
            _config = config ?? new Util().CustomConfiguration();
            _cache = cache ?? new MemoryCacheService();
        }
        public void SetLoadingState(bool isbusy, string message = "Loading....", int delay = 1000) {
             IsBusy = isbusy;
             Message = message;
            _delay = delay;
        }
        public async void InitializeModel(Action? initComponents)
        {
            _isSupressed = true;
            _isBusy = true;
            await Task.Delay( _delay); // Simulación
             initComponents?.Invoke();
            _isBusy = false;
            _isSupressed = false;
        }
        public void InvokeError(string errormeassage) {
            ShowErrorFromModel?.Invoke(errormeassage);
        }
        public void InvokeEvent(Action execute)
        {
            ActionEventFromModel?.Invoke(execute);
        }
        public void InvokeEvent(object execute)
        {
            ObjectEventFromModel?.Invoke(execute);
        }
        public void AddStatusLog(string texto, Brush color)
        {
            //StatusItems.Insert(0,new StatusItem
            //{
            //    Message = texto,
            //    Color = color
            //});
        }
        #endregion

        #region Handler Errors Methods 
        public void AddError(string propertyName, string error)
        {
            if (!_errors.ContainsKey(propertyName))
                _errors[propertyName] = new List<string>();

            _errors[propertyName].Add(error);
            OnErrorsChanged(propertyName);
        }
        public void ClearErrors(string propertyName)
        {
            if (_errors.Remove(propertyName))
                OnErrorsChanged(propertyName);
        }
        #endregion

    }

}