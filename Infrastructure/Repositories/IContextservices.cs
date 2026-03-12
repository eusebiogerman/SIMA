using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SIMA.Infrastructure.Repositories
{
    public interface IContextservices<T> 
    {

        Task<IEnumerable<T>> GetbyId(int id);

        Task<IEnumerable<T>> GetAll(Paging page);

        Task Add(T entitiy);

        Task Update(T entitiy);

        Task Delete(int id);
    }
}
