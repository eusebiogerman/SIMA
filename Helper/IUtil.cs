using SIMA.Infrastructure.Repositories;
using SIMA.Presentation.ViewModel;
using System.Windows.Controls;

namespace SIMA.Helper
{
    internal interface IUtil
    {
        void SupressEventComboBox(bool val = true);
        string getResultMsgAsync(int total);
        void pagingLabels(int total);
        void NavigationGrid(Paging.DIRECTION direction);
        void UpdatePaging(int total = 0);
        void ResizeGrid(string gridheight);
        
        
    }
}