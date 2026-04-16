using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using SIMA.Domain.Models.Objects;
using SIMA.Domain.Models.Views;
using SIMA.Infrastructure.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SIMA.Infrastructure.Repositories
{
    public class MenuServices : IContextservicesMenu<MenuSima>
    {
        private readonly IConfiguration _config;
        private readonly ICacheService _cache;
        private int? _currentIdSave;
        private int _totalfound;
        private decimal _totalvalue;

        public int? CurrentIdSave { get => _currentIdSave; }
        public int TotalFound => _totalfound;
        public decimal TotalValue => _totalvalue;

        public MenuServices(IConfiguration config,ICacheService cache) 
        {
         _config = config;
         _cache = cache;
        }

        private static string GetKey(int iduser)
        {
            return $"menu_{iduser}";
        }
        public async Task<IEnumerable<MenuSima>> GetMenu(int iduser)
        {
            IEnumerable<MenuSima> result;
            try
            {
                string cacheKey = GetKey(iduser);
                var cached = _cache.Get<IEnumerable<MenuSima>>(cacheKey);
                if (cached != null)
                {
                    _totalfound = cached.Count();
                    return cached;
                }

                using (var conn = new SqlConnection(_config.GetConnectionString("DefaultConnection")))
                {
                    result = await conn.QueryAsync<MenuSima>("[SIMA].[getMenu]", new { idUser = iduser } , commandType: System.Data.CommandType.StoredProcedure);
                    _totalfound = result.Count();
                    _cache.Set(cacheKey, result, TimeSpan.FromMinutes(10));
                    return result;
                }
            }
            catch (Exception ex)
            {
                result = new List<MenuSima>();
            }
            return result;
        }
    }
}