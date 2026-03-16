using SIMA.Domain.Models;
using SIMA.ExtensionsHelper;
using SIMA.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SIMA.Infrastructure.Repositories
{
    public class CategoryServices : IContextservices<Category>
    {
        private JsonFile<StockProduct> _stockProductFile;

        public CategoryServices()
        {
            _stockProductFile = new JsonFile<StockProduct>();
            _stockProductFile.loadData();
        }

        #region Abstractions
        public Task<int> Add(Category entitiy)
        {
            throw new NotImplementedException();

        }
        public Task<bool> Update(Category entitiy)
        {
            throw new NotImplementedException();
        }
        public Task<int> Delete(int? id)
        {
            throw new NotImplementedException();
        }
        public async Task<IEnumerable<Category>> GetAll(Paging page)
        {
            int id = 0;
            List<Category> ls = new List<Category>();

            await Task.Run(() =>
            {
                List<string> distc = _stockProductFile.ServicesList
                 .Select(p => p.Category)
                 .Distinct()
                 .OrderBy(c => c)
                 .ToList();

                ls.Add(new Category { Idcategory = id++, Name = "".defaultCategory() });

                foreach (var item in distc)
                {
                    var cat = new Category();
                    cat.Idcategory = id++;
                    cat.Name = item;
                    ls.Add(cat);
                }
            });

            return ls;

        }
        public Task<IEnumerable<Category>> GetbyId(int? id)
        {
            throw new NotImplementedException();
        }
        public Task<bool> Set(Category entitiy)
        {
            throw new NotImplementedException();
        }
        public async Task<int> GetIdIndex(string category)
        {
            int id = 0;
            int retid = 0;
            List<Category> ls = new List<Category>();

            await Task.Run(() =>
            {
                List<string> distc = _stockProductFile.ServicesList
                 .Select(p => p.Category)
                 .Distinct()
                 .OrderBy(c => c)
                 .ToList();

                foreach (var item in distc)
                {
                    id++;
                    if (category.Equals(item))
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
