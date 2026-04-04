using Microsoft.Extensions.Configuration;
using SIMA.Helper;
using SIMA.Infrastructure.Repositories.Interfaces;
using SIMA.Infrastructure.Repositories;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System;
using System.Collections;

namespace SIMA.Presentation.Interfaces
{
    public interface IViewModel
    {
        bool HasErrors { get; }
        bool IsSupressed { get; set; }
        event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged;
        event PropertyChangedEventHandler? PropertyChanged;
        event Action<string> ShowErrorFromModel;
        event Action<object> ObjectEventFromModel;
        event Action<Action> ActionEventFromModel;
        IEnumerable GetErrors(string? propertyName);
        IConfiguration Config { get; set; }
        IPaging Page { get; set; }
        int Delay { set; }
        object PassingParameter { get; set; }
        bool IsBusy { get; set; }
        string Message { get; set; }

        void SetLoadingState(bool isbusy, string message = "Loading....", int delay = 1000);
        void InitializeModel(Action? initComponents);
        void InvokeError(string errormeassage);
        void InvokeEvent(Action execute);
        void InvokeEvent(object execute);
        void AddError(string propertyName, string error);
        void ClearErrors(string propertyName);
    }
}