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
using SIMA.Domain.Models;
using SIMA.Helper;
using Dapper;
using SIMA.Presentation.Views;
using static Dapper.SqlMapper;
using SIMA.Infrastructure.Repositories.Interfaces;
using SIMA.ExtensionsHelper;

namespace SIMA.Infrastructure.Repositories
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
        public StockProductParam(Paging page) {
            offset = page.Offset;
            limit = page.Limit;
        }

        public void SetPage(Paging page)
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

    public class StockProductServices : IContextservices<StockProduct>
    {
        private readonly IConfiguration _config;
        private JsonFile<StockProduct> _stockProductFile;
        private int? _currentIdSave;

        public int? CurrentIdSave { get => _currentIdSave; }
        public StockProductServices()
        {
            _stockProductFile = new JsonFile<StockProduct>();
            _stockProductFile.loadData();
        }
        public StockProductServices(IConfiguration config)
        {
            _config = config;   
            _stockProductFile = new JsonFile<StockProduct>();
        }

        #region Database Action
        private async Task<IEnumerable<StockProductView>> getStock(StockProductParam param)
        {
            using (var conn = new SqlConnection(_config.GetConnectionString("DefaultConnection")))
            {
                object inparam = new
                {
                    idStock = param.idStock,
                    idBrand = param.idBrand,
                    idProduct = param.idProduct,
                    idCategory = param.idCategory,
                    textSearch = param.brands.isNull(param.products).isNull(param.categorys),
                    offset = param.offset,
                    limit = param.limit
                };
                return await conn.QueryAsync<StockProductView>("[dbo].[getStock]", inparam, commandType: System.Data.CommandType.StoredProcedure);
            }
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
        public async Task<IEnumerable<StockProductView>> GetViewAll(Paging page)
        {
            return await getStock(new StockProductParam(page));
        }
        public async Task<IEnumerable<StockProduct>> GetAll(Paging page)
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
        public async Task<int?> GetNextId()
        {
            throw new NotImplementedException();
        }
        #endregion

    }
}