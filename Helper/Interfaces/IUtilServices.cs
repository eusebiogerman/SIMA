using SIMA.Infrastructure.Repositories;
using SIMA.Presentation.ViewModel;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace SIMA.Helper.Interfaces
{
    public interface IUtilServices<U, T>
    {
        U Result();
        T activeFilters();
        void FillCombobox(int? id = null);
        void Fill();
        void Filter();
        Task Filter(T param);
        void FilterbyText(T param);
        void ClearFilters();
        void Edit(T param);

    }
}