using SIMA.Domain.Models;
using SIMA.ExtensionsHelper;
using SIMA.Helper;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Dapper;
using SIMA.Infrastructure.Repositories.Interfaces;

namespace SIMA.Infrastructure.Repositories
{
    public class BrandParam : IParam
    {
        public int? idBrand { get; set; } = null;
        public int? idProduct { get; set; } = null;
        public int? idCategory { get; set; } = null;
        public string? name { get; set; } = null;
        public string? products { get; set; } = null;
        public string? categorys { get; set; } = null;
        public decimal? price { get; set; } = null;
        public int? offset { get; set; } = 0;
        public int? limit { get; set; } = 10;

        public BrandParam()
        {
        }
        public BrandParam(IPaging page)
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
    public class BrandServices : IContextservices<Brand, BrandView, BrandParam>
    {
        private readonly IConfiguration _config;
        private JsonFile<Product> _ProductFile;
        private int? _currentIdSave;
        private decimal _totalvalue;
        private int _totalfound;

        public int? CurrentIdSave => _currentIdSave;
        public int TotalFound => _totalfound;
        public decimal TotalValue => _totalvalue;

        public BrandServices()
        {
            _ProductFile = new JsonFile<Product>("Brands.json");
            _ProductFile.loadData();

        }
        public BrandServices(IConfiguration config)
        {
            _config = config;
        }

        #region DataBase Action
        private async Task<IEnumerable<BrandView>> getBrand(BrandParam param)
        {
            using (var conn = new SqlConnection(_config.GetConnectionString("DefaultConnection")))
            {
                IEnumerable<BrandView> result = await conn.QueryAsync<BrandView>("[dbo].[getBrand]", param, commandType: System.Data.CommandType.StoredProcedure);
                _totalfound = result.Count();
                return result;
            }

        }
        private async Task<int> setBrand(Brand param)
        {

            using (var conn = new SqlConnection(_config.GetConnectionString("DefaultConnection")))
            {
                return await conn.ExecuteScalarAsync<int>("[dbo].[setBrand]", param, commandType: System.Data.CommandType.StoredProcedure);
            }

        }
        #endregion

        #region Abstractions
        public async Task<int> Add(Brand entitiy)
        {
            return await setBrand(entitiy);
        }
        public async Task<bool> Update(Brand entity)
        {
            return (await setBrand(entity) > 0);
        }
        public async Task<bool> Set(Brand entity)
        {
            return (await setBrand(entity) > 0);
        }
        public async Task<int> Delete(int? id)
        {
            using (var conn = new SqlConnection(_config.GetConnectionString("DefaultConnection")))
            {
                return await conn.ExecuteScalarAsync<int>("[dbo].[delBrand]", new { IdProduct = id }, commandType: System.Data.CommandType.StoredProcedure);
            }
        }
        public async Task<IEnumerable<BrandView>> GetViewAll(IPaging page)
        {
            return await getBrand(new BrandParam(page));
        }
        public async Task<IEnumerable<Brand>> GetAll(IPaging page)
        {
            throw new NotImplementedException();
        }
        public async Task<IEnumerable<Brand>> GetbyId(int? id)
        {
            throw new NotImplementedException();
        }
        #endregion

        #region Util Function and Methods
        public async Task<IEnumerable<BrandView>> GetByFilter(BrandParam param)
        {
            return await getBrand(param);
        }
        public async Task<int> GetTotalFound(BrandParam param)
        {
            param.offset = 0;
            param.limit = 1000;
            IEnumerable<BrandView> res = await getBrand(param);
            return res.Where(p => p.IdBrand != null).Count();
        }
        public async Task<decimal> GetTotalValue()
        {
            throw new NotImplementedException();
        }
        public Task<object?> GetNextId()
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}