using System.Collections.Generic;
using System.Threading.Tasks;

namespace SIMA.Infrastructure.Repositories.Interfaces
{
    public interface IContextservicesMenu<T> 
    {
        int? CurrentIdSave { get; }
        int TotalFound { get; }
        decimal TotalValue { get; }

        Task<IEnumerable<T>> GetMenu(int iduser);

    }
}