using SIMA.Domain.Models;
using SIMA.Helper;
using SIMA.Presentation.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Markup;
using SIMA.ExtensionsHelper;

namespace SIMA.Infrastructure.Repositories
{
    public class ProductServices : IContextservices<Product>
    {
        private JsonFile<Product> _ProductFile;
        private int? _currentIdSave;
        public int? CurrentIdSave { get => _currentIdSave; }

        public Product DefeaultProduct
        {
            get => new Product
            {
                IdProduct = 0,
                Category = "".defaultCategory(),
                Name = "Select",
                Price = 0
            };
        }

        public ProductServices()
        {
            _ProductFile = new JsonFile<Product>("Products.json");
            _ProductFile.loadData();

        }

        private IEnumerable<Product> getProduct(Product param)
        {
            IEnumerable<Product> products =
                _ProductFile.ServicesList
                .Where(p => (param.IdProduct == null || p.IdProduct.Equals(param.IdProduct))
                && (param.Category == string.Empty || p.Category.Contains(param.Category, StringComparison.OrdinalIgnoreCase))
                && (param.Name == string.Empty || p.Name.Contains(param.Name, StringComparison.OrdinalIgnoreCase)));

            IEnumerable<Product> productsret = products.Union(new List<Product> { DefeaultProduct });

            return productsret.OrderBy(p => p.IdProduct);

        }

        #region Abstractions

        public Task<IEnumerable<Product>> GetAll(Paging page)
        {
            throw new NotImplementedException();
        }
        public async Task<IEnumerable<Product>> GetbyId(int? id)
        {
            return await Task.Run(() => getProduct(new Product { IdProduct = id, Category = string.Empty, Name = string.Empty }));
        }
        public async Task<int> Add(Product entity)
        {
            if (_ProductFile.ServicesList.Any(p => p.IdProduct == entity.IdProduct))
            {
                throw new InvalidOperationException($"The product exists with the ID {entity.IdProduct}");
            }
            else
                entity.IdProduct = await GetNextId();

            _currentIdSave = entity.IdProduct;
            _ProductFile.ServicesList.Add(entity);
            return await _ProductFile.SaveData() ? 1 : 0;
        }
        public async Task<bool> Update(Product entity)
        {
            var existingProduct = _ProductFile.ServicesList.FirstOrDefault(p => p.IdProduct == entity.IdProduct);

            if (existingProduct == null)
            {
                throw new InvalidOperationException($"Product Not found with ID {entity.IdProduct}");
            }

            // Actualizar propiedades
            _currentIdSave = entity.IdProduct;
            existingProduct.Name = entity.Name;
            existingProduct.Category = entity.Category;
            existingProduct.Price = entity.Price;

            return await _ProductFile.SaveData();
        }
        public async Task<int> Delete(int? id)
        {
            var product = _ProductFile.ServicesList.FirstOrDefault(p => p.IdProduct == id);

            if (product == null)
            {
                throw new InvalidOperationException($"Product Not found with ID  {id}");
            }

            _ProductFile.ServicesList.Remove(product);
            return await _ProductFile.SaveData() ? 1 : 0;
        }
        public async Task<bool> Set(Product entity)
        {
            var existingProduct = _ProductFile.ServicesList.FirstOrDefault(p => p.IdProduct == entity.IdProduct);
            return ((existingProduct == null) ? (await Add(entity) > 0) : await Update(entity));
        }
        #endregion

        #region Util Function and Methods

        public IEnumerable<Product> DefeaultProductList()
        {
            return new List<Product> { DefeaultProduct };
        }

        public async Task<IEnumerable<Product>> GetByFilter(Product param)
        {
            return await Task.Run(() => getProduct(param));


        }
        public async Task<int> GetTotalFound(Product param)
        {
            throw new NotImplementedException();
        }
        public async Task<decimal> GetTotalValue()
        {
            throw new NotImplementedException();
        }
        public async Task<int?> GetNextId()
        {
            return await Task.Run(() => _ProductFile.ServicesList.Any() ? _ProductFile.ServicesList.Max(p => p.IdProduct) + 1 : 1);
        }
        public async Task<int> GetIdIndex(string category, string name)
        {
            int id = 0;
            int retid = 0;
            await Task.Run(() =>
            {
                IEnumerable<Product> prod = getProduct(new Product { IdProduct = null, Category = category, Name = string.Empty });
                foreach (var item in prod)
                {
                    id++;
                    if (name.Equals(item.Name))
                    {
                        retid = id;
                    }
                }
            });
            return retid;
        }
        #endregion


    }
}
