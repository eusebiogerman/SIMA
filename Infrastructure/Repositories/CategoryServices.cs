using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using SIMA.Domain.Models.Intefaces;
using SIMA.Domain.Models.Objects;
using SIMA.Domain.Models.Params;
using SIMA.Domain.Models.Views;
using SIMA.ExtensionsHelper;
using SIMA.Helper;
using SIMA.Infrastructure.Repositories.Interfaces;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SIMA.Infrastructure.Repositories
{
    public class CategoryServices : IContextservices<Category, Category, CategoryParam>
    {
        private readonly IConfiguration _config;
        private readonly ICacheService _cache;
        private JsonFile<StockProduct> _stockProductFile;
        private int? _currentIdSave;
        private decimal _totalvalue;
        private int _totalfound;

        public int? CurrentIdSave => _currentIdSave;
        public int TotalFound => _totalfound;
        public decimal TotalValue => _totalvalue;

        public CategoryServices()
        {
            _stockProductFile = new JsonFile<StockProduct>();
            _stockProductFile.loadData();
        }
        public CategoryServices(IConfiguration config, ICacheService cache) {
            _config = config;
            _cache = cache;
        }

        #region DataBase Action
        private static string GetKey(CategoryParam param)
        {
            return $"product_{param.IdCategory}" +
                $"_{param.Name}" +
                $"_{param.offset}" +
                $"_{param.limit}";
        }
        private async Task<IEnumerable<Category>> getCategory(CategoryParam param)
        {
            IEnumerable<Category> result;
            try
            {
                string cacheKey = GetKey(param);
                var cached = _cache.Get<IEnumerable<Category>>(cacheKey);
                if (cached != null)
                {
                    _totalfound = cached.Count();
                    return cached;
                }

                using (var conn = new SqlConnection(_config.GetConnectionString("DefaultConnection")))
                {
                    result = await conn.QueryAsync<Category>("[dbo].[getCategory]", param, commandType: System.Data.CommandType.StoredProcedure);
                    _totalfound = result.Count();
                    _cache.Set(cacheKey, result, TimeSpan.FromMinutes(10));
                    return result;
                }
            }
            catch (Exception ex)
            {
                result = new List<Category>();
            }
            return result;

        }
        private async Task<int> setCategory(Category param) {
            using (var conn = new SqlConnection(_config.GetConnectionString("DefaultConnection")))
            {
                var test = await conn.ExecuteScalarAsync<int>("[dbo].[setCategory]", param, commandType: System.Data.CommandType.StoredProcedure);
                return test;
            }
        }
        #endregion

        #region Abstractions
        public Task<IEnumerable<Category>> GetViewAll(IPaging page)
        {
            throw new NotImplementedException();
        }
        public async Task<IEnumerable<Category>> GetAll(IPaging page)
        {
            return await getCategory(new CategoryParam(page));
        }
        public async Task<IEnumerable<Category>> GetbyId(int? id)
        {
            return await getCategory(new CategoryParam { IdCategory = id });
        }
        public async Task<int> Add(Category entitiy)
        {
            return await setCategory(entitiy);
        }
        public async Task<bool> Update(Category entitiy)
        {
            return (await setCategory(entitiy) > 0);
        }
        public async Task<int> Delete(int? id)
        {
            using var conn = new SqlConnection(_config.GetConnectionString("DefaultConnection"));
            return await conn.ExecuteScalarAsync<int>("[dbo].[delCategory]", new { IdCategory = id }, commandType: System.Data.CommandType.StoredProcedure);
        }
        public async Task<bool> Set(Category entitiy)
        {
          return (await setCategory(entitiy) > 0);
            
        }
        #endregion

        #region Util Function and Methods
        public async Task<IEnumerable<Category>> GetByFilter(CategoryParam param)
        {
            return await getCategory(param);

        }
        public async Task<int> GetTotalFound(CategoryParam param)
        {
            param.offset = 0;
            param.limit  = 1000;
            IEnumerable<Category> res = await getCategory(param);
            return res.Where(p=> p.IdCategory != null).Count();
        }
        public Task<decimal> GetTotalValue()
        {
            throw new NotImplementedException();
        }
        public async Task<object?> GetNextId()
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
