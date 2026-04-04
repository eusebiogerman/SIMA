using SIMA.Infrastructure.Repositories.Interfaces;
using SIMA.Domain.Models.Intefaces;

namespace SIMA.Domain.Models.Params
{
    public class StockProductParam : IParam
    {
        public int? idStock { get; set; } = null;
        public int? idBrand { get; set; } = null;
        public int? idProduct { get; set; } = null;
        public int? idCategory { get; set; } = null;
        public string? brands { get; set; } = null;
        public string? products { get; set; } = null;
        public string? categorys { get; set; } = null;
        public decimal? price { get; set; } = null;
        public int? stock { get; set; } = null;
        public int? offset { get; set; } = 0;
        public int? limit { get; set; } = 10;

        public StockProductParam()
        {
        }
        public StockProductParam(IPaging page)
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
            idStock = null;
            idBrand = null;
            idProduct = null;
            idCategory = null;
            brands = null;
            products = null;
            categorys = null;
            price = null;
            stock = null;
            offset = inoffset;
            limit = inlimit;

        }
    }
}