using SIMA.Domain.Models;
using SIMA.ExtensionsHelper;
using SIMA.Helper;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

namespace SIMA.Infrastructure.Repositories
{
    public class BrandServices : IContextservices<Brand>
    {
        private JsonFile<StockProduct> _stockProductFile;

        public BrandServices() {
            _stockProductFile = new JsonFile<StockProduct>();
            _stockProductFile.loadData();
        }

        public Task<int> Add(Brand entitiy)
        {
            throw new System.NotImplementedException();
        }

        public Task<int> Delete(int? id)
        {
            throw new System.NotImplementedException();
        }

        public async Task<IEnumerable<Brand>> GetAll(Paging page)
        {
            int id = 0;
            List<Brand> ls = new List<Brand>();

            await Task.Run(() =>
            {
                List<string> distc = _stockProductFile.ServicesList
                 .Select(p => p.Name)
                 .Distinct()
                 .OrderBy(c => c)
                 .ToList();

                ls.Add(new Brand { IdBrand = id++, Name = "" });

                foreach (var item in distc)
                {
                    var cat = new Brand();
                    cat.IdBrand = id++;
                    cat.Name = item;
                    ls.Add(cat);
                }
            });

            return ls;
        }

        public Task<IEnumerable<Brand>> GetbyId(int? id)
        {
            throw new System.NotImplementedException();
        }

        public Task<bool> Set(Brand entitiy)
        {
            throw new System.NotImplementedException();
        }

        public Task<bool> Update(Brand entitiy)
        {
            throw new System.NotImplementedException();
        }
    }
}