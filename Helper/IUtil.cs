using SIMA.Infrastructure.Repositories;
using SIMA.Infrastructure.Repositories.Interfaces;
using SIMA.Presentation.ViewModel;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace SIMA.Helper
{
    internal interface IUtil
    {
        void SupressEventComboBox(bool val = true);
        string getResultMessage(int total);
        void pagingLabels(int total);
        void NavigationGrid(DIRECTION direction);
        void UpdatePaging(int total = 0);
        void ResizeGrid(string gridheight);
        
        
    }
}