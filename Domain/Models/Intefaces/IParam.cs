using SIMA.Infrastructure.Repositories.Interfaces;

namespace SIMA.Domain.Models.Intefaces
{
    public interface IParam
    {
        void SetPage(IPaging page);
        void ResetParam(int? inoffset = 0, int? inlimit = 10);
    }

}