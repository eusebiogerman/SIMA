using SIMA.Infrastructure.Repositories;
using SIMA.Presentation.ViewModel;
using System.Windows.Controls;
using SIMA.Infrastructure.Repositories.Interfaces;

namespace SIMA.Helper.Interfaces
{
    internal interface IUtil
    {
        void SupressEventComboBox(bool val = true);
        string getResultMsgAsync(int total);
        void pagingLabels(int total);
        void NavigationGrid(DIRECTION direction);
        void UpdatePaging(int total = 0);
        void ResizeGrid(string gridheight);


    }
}