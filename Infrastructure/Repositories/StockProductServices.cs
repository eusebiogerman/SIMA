using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Navigation;
using System.Xml;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Microsoft.Data.SqlClient;
using SIMA.Helper;
using Dapper;
using SIMA.Presentation.Views;
using static Dapper.SqlMapper;
using SIMA.Infrastructure.Repositories.Interfaces;
using SIMA.ExtensionsHelper;
using SIMA.Domain.Models.Objects;
using SIMA.Domain.Models.Views;
using SIMA.Domain.Models.Params;

namespace SIMA.Infrastructure.Repositories
{

    public class StockProductServices : IContextservices<StockProduct, StockProductView, StockProductParam>
    {
        private IConfiguration _config;
        private JsonFile<StockProduct> _stockProductFile;
        private int? _currentIdSave;
        private int _totalfound;
        private decimal _totalvalue;

        public int? CurrentIdSave { get => _currentIdSave; }
        public int TotalFound => _totalfound;
        public decimal TotalValue => _totalvalue;

        public StockProductServices()
        {
            _config = new Util().CustomConfiguration();
            _stockProductFile = new JsonFile<StockProduct>();
            _stockProductFile.loadData();
        }
        public StockProductServices(IConfiguration config)
        {
            _config = config;   
            _stockProductFile = new JsonFile<StockProduct>();
        }

        /// <summary>
        /// Cast to Sotck Param
        /// </summary>
        class GetStockParam 
        {
            public int? idStock { get; private set; } = null;
            public int? idBrand { get; private set; } = null;
            public int? idProduct { get; private set; } = null;
            public int? idCategory { get; private set; } = null;
            public string? textSearch { get; private set; } = null;
            public int? offset { get; private set; } = null;
            public int? limit { get; private set; } = null;

            public GetStockParam(StockProductParam param)
            {
                idStock = param?.idStock;
                idBrand = param?.idBrand;
                idProduct = param?.idProduct;
                idCategory = param?.idCategory;
                textSearch = param?.brands.isNull(param?.products.isNull(param?.categorys));
                offset = param?.offset;
                limit = param?.limit;
            }
        }

        #region Database Action
        private async Task<IEnumerable<StockProductView>> getStock(StockProductParam param)
        {
            IEnumerable<StockProductView> result;
             var getparam = new GetStockParam(param);
            try
            {

                using (var conn = new SqlConnection(_config.GetConnectionString("DefaultConnection")))
                {
                    result = await conn.QueryAsync<StockProductView>("[dbo].[getStock]", getparam, commandType: System.Data.CommandType.StoredProcedure);
                    _totalfound = result.Count();
                }

            }
            catch (Exception ex)
            {
                result = new List<StockProductView>();
            }
            return result;
        }
        private async Task<int> setStock(StockProduct param)
        {

            using (var conn = new SqlConnection(_config.GetConnectionString("DefaultConnection")))
            {
                object inparam = new { idStock = param.IdStock, idBrand = param.IdBrand, stock = param.Stock };
                return await conn.ExecuteScalarAsync<int>("[dbo].[setStock]", inparam, commandType: System.Data.CommandType.StoredProcedure);
            }

        }
        #endregion

        #region Abstractions
        public async Task<int> Add(StockProduct entitiy)
        {
            return await setStock(entitiy);
        }
        public async Task<bool> Update(StockProduct entity)
        {
            return (await setStock(entity) > 0);
        }
        public async Task<bool> Set(StockProduct entitiy)
        {
            return (await setStock(entitiy) > 0);
        }
        public async Task<int> Delete(int? id)
        {
            using (var conn = new SqlConnection(_config.GetConnectionString("DefaultConnection")))
            {
                return await conn.ExecuteScalarAsync<int>("[dbo].[delStock]", new { IdStock = id }, commandType: System.Data.CommandType.StoredProcedure);
            }
        }
        public async Task<IEnumerable<StockProductView>> GetViewAll(IPaging page)
        {
            return await getStock(new StockProductParam(page));
        }
        public async Task<IEnumerable<StockProduct>> GetAll(IPaging page)
        {
            throw new NotImplementedException();
        }
        public async Task<IEnumerable<StockProduct>> GetbyId(int? id)
        {
            throw new NotImplementedException();
        }
        #endregion

        #region Util Function and Methods
        public async Task<IEnumerable<StockProductView>> GetByFilter(StockProductParam param)
        {
            return await getStock(param);

        }
        public async Task<int> GetTotalFound(StockProductParam param)
        {
            param.offset = 0;
            param.limit = 1000;
            IEnumerable<StockProductView> res = await getStock(param);
            return res.Where(p => p.IdBrand != null).Count();
  
        }
        public async Task<decimal> GetTotalValue()
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