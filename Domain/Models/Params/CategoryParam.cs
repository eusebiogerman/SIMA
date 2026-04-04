using SIMA.Domain.Models.Intefaces;
using SIMA.Infrastructure.Repositories.Interfaces;
using System;

namespace SIMA.Domain.Models.Params
{
    public class CategoryParam : IParam
    {
        public int? IdCategory { get; set; } = null;
        public string? Name { get; set; } = null;
        public int? offset { get; set; } = 0;
        public int? limit { get; set; } = 10;

        public CategoryParam()
        {
        }
        public CategoryParam(IPaging page)
        {
            offset = page.Offset;
            limit = page.Limit;
        }

        public void SetPage(IPaging page)
        {
            offset = page.Offset;
            limit = page.Limit;
        }
        public void ResetParam(int? inoffset = 0, int? inlimit = 10)
        {
            throw new NotImplementedException();
        }
    }
}
