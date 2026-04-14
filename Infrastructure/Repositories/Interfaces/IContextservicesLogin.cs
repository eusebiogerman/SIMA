using System.Threading.Tasks;

namespace SIMA.Infrastructure.Repositories.Interfaces
{
    public interface IContextservicesLogin<T, V, P> : IContextservices<T, V, P>
    {
        Task<bool> LoginAsync(P param);
    }
}