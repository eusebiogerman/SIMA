using SIMA.Infrastructure.Repositories;
using SIMA.Presentation.ViewModel;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Controls;
using WpfJEG.net6;

namespace SIMA.Helper.Interfaces
{
    public interface IUtilServices<V, P>
    {
        P activeFilters();
        P activeFilters(Func<P> actparam);
        P activeFilters(string FilterFieldClass);
        void FillChildCombobox(IEnumerable<LovObject> result);
        void Fill(IEnumerable<V> result);
        Task Edit(string rizenumber = "40%", Action? editraction = null);

    }
}