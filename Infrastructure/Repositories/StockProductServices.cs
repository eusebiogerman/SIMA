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
        public async Task<IEnumerable<StockProduct>> GetbyId(int id)
        {
            return await Task.Run(() => _stockProductFile.ServicesList.Where(p => p.IdStock == id));
        }

        public async Task<IEnumerable<StockProduct>> GetAll(Paging page)
        {
            return await Task.Run(() => _stockProductFile.ServicesList.AsEnumerable().Skip(page.Offset).Take(page.Limit));
        }

        public async Task Add(StockProduct entity)
        {
            // Verificar si ya existe un producto con el mismo ID
            if (_stockProductFile.ServicesList.Any(p => p.IdStock == entity.IdStock))
            {
                throw new InvalidOperationException($"Ya existe un producto con el ID {entity.IdStock}");
            }

            _stockProductFile.ServicesList.Add(entity);
            await _stockProductFile.SaveData();
        }

        public async Task Update(StockProduct entity)
        {
            var existingProduct = _stockProductFile.ServicesList.FirstOrDefault(p => p.IdStock == entity.IdStock);

            if (existingProduct == null)
            {
                throw new InvalidOperationException($"No se encontró el producto con ID {entity.IdStock}");
            }

            // Actualizar propiedades
            existingProduct.Name = entity.Name;
            existingProduct.Category = entity.Category;
            existingProduct.Price = entity.Price;
            existingProduct.Stock = entity.Stock;

            await _stockProductFile.SaveData();
        }

        public async Task Delete(int id)
        {
            var product = _stockProductFile.ServicesList.FirstOrDefault(p => p.IdStock == id);

            if (product == null)
            {
                throw new InvalidOperationException($"No se encontró el producto con ID {id}");
            }

            _stockProductFile.ServicesList.Remove(product);
            await _stockProductFile.SaveData();
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

        public async Task<int> GetNextId()
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