using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Navigation;
using System.Xml;
using Newtonsoft.Json;
using SIMA.Domain.Models;
using SIMA.Helper;

namespace SIMA.Infrastructure.Repositories
{
    public class StockProductServices : IContextservices<StockProduct>
    {
        private JsonFile<StockProduct> _stockProductFile;
        private int? _currentIdSave;

        public int? CurrentIdSave { get => _currentIdSave; }
        public StockProductServices()
        {
            _stockProductFile = new JsonFile<StockProduct>();
            _stockProductFile.loadData();
        }

        private IEnumerable<StockProduct> getStock(StockProduct param)
        {
            return _stockProductFile.ServicesList.Where(p =>
                               (param.Category == string.Empty || p.Category.Contains(param.Category, StringComparison.OrdinalIgnoreCase))
                            && (param.Name == string.Empty || p.Name.Contains(param.Name, StringComparison.OrdinalIgnoreCase)));
        }

        #region Abstractions
        public async Task<IEnumerable<StockProduct>> GetbyId(int? id)
        {
            return await Task.Run(() => _stockProductFile.ServicesList.Where(p => p.IdStock == id));
        }
        public async Task<IEnumerable<StockProduct>> GetAll(Paging page)
        {
            return await Task.Run(() => _stockProductFile.ServicesList.AsEnumerable().Skip(page.Offset).Take(page.Limit));
        }
        public async Task<int> Add(StockProduct entity)
        {
            if (_stockProductFile.ServicesList.Any(p => p.IdStock == entity.IdStock))
            {
                throw new InvalidOperationException($"The Stock exists with the ID {entity.IdStock}");
            }
            else
                entity.IdStock = await GetNextId();


            _currentIdSave = entity.IdStock;
            _stockProductFile.ServicesList.Add(entity);
            return await _stockProductFile.SaveData() ? 1 : 0;
        }
        public async Task<bool> Update(StockProduct entity)
        {
            var existingProduct = _stockProductFile.ServicesList.FirstOrDefault(p => p.IdStock == entity.IdStock);

            if (existingProduct == null)
            {
                throw new InvalidOperationException($"Stock Not found with ID {entity.IdStock}");
            }

            _currentIdSave = entity.IdStock;
            existingProduct.Name = entity.Name;
            existingProduct.Category = entity.Category;
            existingProduct.Price = entity.Price;
            existingProduct.Stock = entity.Stock;

            return await _stockProductFile.SaveData();
        }
        public async Task<int> Delete(int? id)
        {
            var product = _stockProductFile.ServicesList.FirstOrDefault(p => p.IdStock == id);

            if (product == null)
            {
                throw new InvalidOperationException($"Stock Not found with ID {id}");
            }

            _stockProductFile.ServicesList.Remove(product);
            return await _stockProductFile.SaveData() ? 1 : 0;
        }
        public async Task<bool> Set(StockProduct entitiy)
        {
            var existingProduct = _stockProductFile.ServicesList.FirstOrDefault(p => p.IdStock == entitiy.IdStock);
            return ((existingProduct == null) ? (await Add(entitiy) > 0) : await Update(entitiy));
        }
        #endregion

        #region Util Function and Methods
        public async Task<IEnumerable<StockProduct>> GetByFilter(StockProduct param, Paging page)
        {
            return await Task.Run(() =>
              {
                  IEnumerable<StockProduct> stock = getStock(param).Skip(page.Offset).Take(page.Limit);
                  return stock.Any() ? stock : getStock(param).Skip(1).Take(page.Limit);
              });


        }
        public async Task<IEnumerable<StockProduct>> GetLowStock(int threshold = 10)
        {
            return await Task.Run(() => _stockProductFile.ServicesList.Where(p => p.Stock <= threshold));
        }
        public async Task<int> GetTotalFound(StockProduct param)
        {
            return await Task.Run(() => getStock(param).Count());
        }
        public async Task<decimal> GetTotalValue()
        {
            return await Task.Run(() => _stockProductFile.ServicesList.Sum(p => p.Price * p.Stock));
        }
        public async Task<int?> GetNextId()
        {
            return await Task.Run(() => _stockProductFile.ServicesList.Any() ? _stockProductFile.ServicesList.Max(p => p.IdStock) + 1 : 1);
        }
        public async Task<IEnumerable<string>> GetUniqueCategories()
        {
            return await Task.Run(() =>
                _stockProductFile.ServicesList
                    .Select(p => p.Category)
                    .Distinct()
                    .OrderBy(c => c)
                    .ToList()
            );
        }
        #endregion

    }
}