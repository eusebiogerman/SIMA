using SIMA.Infrastructure.Repositories;
using SIMA.Presentation.ViewModel;
using System.Threading.Tasks;

namespace SIMA.Helper
{
    public interface IUtilServices<out U,T>
    {
        U Result();
        T activeFilters();
        void FillCombobox(int? id = null);
        void Fill();
        void Filter();
        void Filter(T param );
        void FilterbyText(T param);
        void ClearFilters();
        void Edit(T param);

    }
}