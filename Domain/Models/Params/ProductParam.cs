using System;
using SIMA.Infrastructure.Repositories.Interfaces;
using SIMA.Domain.Models.Intefaces;

namespace SIMA.Domain.Models.Params
{
    public class ProductParam : IParam
    {
        public int? idProduct { get; set; } = null;
        public int? idCategory { get; set; } = null;
        public string? name { get; set; } = null;
        public string? categorys { get; set; } = null;
        public decimal? price { get; set; } = null;
        public int? offset { get; set; } = 0;
        public int? limit { get; set; } = 10;

        public ProductParam()
        {
        }
        public ProductParam(IPaging page)
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
