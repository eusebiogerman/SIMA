using SIMA.Domain.Models;
using SIMA.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Threading.Tasks;

namespace SIMA.Infrastructure.Repositories
{
    public class Paging
    {
        private JsonFile<String> _jsonLimit;
        private const int _defaulOffset = 50;
        private const int _defaulLimit = 100;
        private int _offset = _defaulOffset;
        private int _limit = _defaulLimit;
        private int _pagenumber = 1;
        private int _totalPage = 1;

        public Paging()
        {
            _jsonLimit = new JsonFile<String>("PageLimit.json");
            _jsonLimit.loadData();
        }

        public int DefaulOffset { get => _defaulOffset; }
        public int DefaulLimit { get => _defaulLimit; }
        public int Pagenumber { get => _pagenumber; set => _pagenumber = value; }
        public int Offset { get => _offset; set => _offset = value; }
        public int Limit { get => _limit; set => _limit = value; }
        public int TotalPage { get => _totalPage; set => _totalPage = value; }

        public void resetPage()
        {
            _offset = _defaulOffset;
            _limit = DefaulLimit;
            _pagenumber = 1;

        }
        public void parsePageData(int total)
        {
            _offset = (total > _offset) ? _offset : 1;
            int pagecount = (total / (_limit > 0 ? _limit : 1));
            _totalPage = pagecount > 0 ? pagecount : 1;


        }
        public async Task<IEnumerable<String>> GetLimitPaging()
        {
            return await Task.Run(() => _jsonLimit.ServicesList.Select(p => p).ToList());

        }

    }
}