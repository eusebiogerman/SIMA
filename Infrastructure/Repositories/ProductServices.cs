using SIMA.Helper;
using SIMA.Presentation.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Markup;
using SIMA.ExtensionsHelper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Dapper;
using static Dapper.SqlMapper;
using SIMA.Infrastructure.Repositories.Interfaces;
using SIMA.Domain.Models.Objects;
using SIMA.Domain.Models.Views;
using SIMA.Domain.Models.Params;

namespace SIMA.Infrastructure.Repositories
{
    public class ProductServices : IContextservices<Product, ProductView, ProductParam>
    {
        private readonly IConfiguration _config;
        private readonly ICacheService _cache;
        private JsonFile<Product> _ProductFile;
        private int? _currentIdSave;
        private decimal _totalvalue;
        private int _totalfound;

        public int? CurrentIdSave => _currentIdSave; 
        public int TotalFound => _totalfound;
        public decimal TotalValue => _totalvalue;

        public ProductServices()
        {
            _ProductFile = new JsonFile<Product>("Products.json");
            _ProductFile.loadData();

        }
        public ProductServices(IConfiguration config, ICacheService cache)
        {
            _config = config;
            _cache = cache;
        }

        #region DataBase Action
        private static string GetKey(ProductParam param)
        {
            return $"brand_{param.idProduct}" +
                $"_{param.idCategory}" +
                $"_{param.categorys}" +
                $"_{param.name}" +
                $"_{param.price}" +
                $"_{param.offset}" +
                $"_{param.limit}";
        }
        private async Task<IEnumerable<ProductView>> getProduct(ProductParam param)
        {
            IEnumerable<ProductView> result;
            try
            {
                string cacheKey = GetKey(param);
                var cached = _cache.Get<IEnumerable<ProductView>>(cacheKey);
                if (cached != null)
                {
                    _totalfound = cached.Count();
                    return cached;
                }

                using (var conn = new SqlConnection(_config.GetConnectionString("DefaultConnection")))
            {
                 result = await conn.QueryAsync<ProductView>("[dbo].[getProduct]", param, commandType: System.Data.CommandType.StoredProcedure);
                _totalfound = result.Count();
                _cache.Set(cacheKey, result, TimeSpan.FromMinutes(10));
                return result;
            }
            }
            catch (Exception ex)
            {
                result = new List<ProductView>();
            }
            return result;

        }
        private async Task<int> setProduct(Product param)
        {

            using (var conn = new SqlConnection(_config.GetConnectionString("DefaultConnection")))
            {
                return await conn.ExecuteScalarAsync<int>("[dbo].[setProduct]", param, commandType: System.Data.CommandType.StoredProcedure);
            }

        }
        #endregion

        #region Abstractions
        public async Task<int> Add(Product entitiy)
        {
            return await setProduct(entitiy);
        }
        public async Task<bool> Update(Product entity)
        {
            return (await setProduct(entity) > 0);
        }
        public async Task<int> Delete(int? id)
        {
            using (var conn = new SqlConnection(_config.GetConnectionString("DefaultConnection")))
            {
                return await conn.ExecuteScalarAsync<int>("[dbo].[delProduct]", new { IdProduct = id }, commandType: System.Data.CommandType.StoredProcedure);
            }
        }
        public async Task<IEnumerable<ProductView>> GetViewAll(IPaging page)
        {
            return await getProduct(new ProductParam(page));
        }
        public async Task<IEnumerable<Product>> GetAll(IPaging page)
        {
            throw new NotImplementedException();
        }
        public async Task<IEnumerable<Product>> GetbyId(int? id)
        {
            throw new NotImplementedException();
        }
        public async Task<bool> Set(Product entity)
        {
            return (await setProduct(entity) > 0);
        }
        #endregion

        #region Util Function and Methods
        public async Task<IEnumerable<ProductView>> GetByFilter(ProductParam param)
        {
            return await getProduct(param);
        }
        public async Task<int> GetTotalFound(ProductParam param)
        {
            param.offset = 0;
            param.limit = 1000;
            IEnumerable<ProductView> res = await getProduct(param);
            return res.Where(p => p.IdProduct != null).Count();
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
