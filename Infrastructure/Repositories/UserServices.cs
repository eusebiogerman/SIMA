using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using SIMA.Domain.Models.Objects;
using SIMA.Domain.Models.Params;
using SIMA.Domain.Models.Views;
using SIMA.Helper;
using SIMA.Infrastructure.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SIMA.Infrastructure.Repositories
{
    public class UserServices : IContextservicesLogin<Users, UsertView, UserParam>
    {

        private readonly ICacheService _cache;
        private readonly IConfiguration _config;
        private int? _currentIdSave;
        private decimal _totalvalue;
        private int _totalfound;

        public UserServices(IConfiguration config, ICacheService cache)
        {
            _config = config;
            _cache = cache;
        }

        public int? CurrentIdSave => _currentIdSave;
        public int TotalFound => _totalfound;
        public decimal TotalValue => _totalvalue;

        #region DataBase Action
        private static string GetKey(UserParam param)
        {
            return $"User_{param.UserId}" +
                $"_{param.UserName}" +
                $"_{param.UserEmail}" +
                $"_{param.UserPhone}" +
                $"_{param.CountryCode}";
        }
        private async Task<IEnumerable<UsertView>> getBrand(UserParam param)
        {
            IEnumerable<UsertView> result;
            try
            {
                string cacheKey = GetKey(param);
                var cached = _cache.Get<IEnumerable<UsertView>>(cacheKey);
                if (cached != null)
                {
                    _totalfound = cached.Count();
                    return cached;
                }

                using (var conn = new SqlConnection(_config.GetConnectionString("DefaultConnection")))
                {
                    result = await conn.QueryAsync<UsertView>("[dbo].[getUser]", param, commandType: System.Data.CommandType.StoredProcedure);
                    _totalfound = result.Count();
                    _cache.Set(cacheKey, result, TimeSpan.FromMinutes(10));
                    return result;
                }
            }
            catch (Exception ex)
            {
                result = new List<UsertView>();
            }
            return result;

        }
        private async Task<int> setBrand(Users param)
        {

            using (var conn = new SqlConnection(_config.GetConnectionString("DefaultConnection")))
            {
                return await conn.ExecuteScalarAsync<int>("[dbo].[setUser]", param, commandType: System.Data.CommandType.StoredProcedure);
            }

        }
        #endregion

        #region Abstractions
        public async Task<int> Add(Users entitiy)
        {
            return await setBrand(entitiy);
        }
        public async Task<bool> Update(Users entity)
        {
            return (await setBrand(entity) > 0);
        }
        public async Task<bool> Set(Users entity)
        {
            return (await setBrand(entity) > 0);
        }
        public async Task<int> Delete(int? id)
        {
            using (var conn = new SqlConnection(_config.GetConnectionString("DefaultConnection")))
            {
                return await conn.ExecuteScalarAsync<int>("[dbo].[delBrand]", new { @IdBrand = id }, commandType: System.Data.CommandType.StoredProcedure);
            }
        }
        public async Task<IEnumerable<UsertView>> GetViewAll(IPaging page)
        {
            return await getBrand(new UserParam(page));
        }
        public async Task<IEnumerable<Users>> GetAll(IPaging page)
        {
            throw new NotImplementedException();
        }
        public async Task<IEnumerable<Users>> GetbyId(int? id)
        {
            throw new NotImplementedException();
        }
        #endregion

        #region Util Function and Methods
        public async Task<bool> LoginAsync(UserParam param) {
            return true; 
        }
        public async Task<IEnumerable<UsertView>> GetByFilter(UserParam param)
        {
            return await getBrand(param);
        }
        public async Task<int> GetTotalFound(UserParam param)
        {
            param.offset = 0;
            param.limit = 1000;
            IEnumerable<UsertView> res = await getBrand(param);
            return res.Where(p => p.UserId != null).Count();
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