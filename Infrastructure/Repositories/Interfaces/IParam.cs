namespace SIMA.Infrastructure.Repositories.Interfaces
{
    public interface IParam
    {
        void SetPage(IPaging page);
        void ResetParam(int? inoffset = 0, int? inlimit = 10);
     }
        
}