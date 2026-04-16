using SIMA.Domain.Models;
using SIMA.Domain.Models.Intefaces;
using SIMA.Domain.Models.Objects;
using SIMA.Domain.Models.Params;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SIMA.Infrastructure.Repositories.Interfaces
{
    public interface IContextservices<T, V, P>
    {
        int? CurrentIdSave { get; }
        int TotalFound { get; }
        decimal TotalValue { get; }

        Task<IEnumerable<V>> GetViewAll(IPaging page);
        Task<IEnumerable<T>> GetbyId(int? id);
        Task<IEnumerable<T>> GetAll(IPaging page);
        Task<IEnumerable<V>> GetByFilter(P param);
        Task<int> Add(T entitiy);
        Task<bool> Update(T entitiy);
        Task<int> Delete(int? id);
        Task<bool> Set(T entitiy);

        Task<int> GetTotalFound(P param);
        Task<decimal> GetTotalValue();
        Task<object?> GetNextId();
    }
}