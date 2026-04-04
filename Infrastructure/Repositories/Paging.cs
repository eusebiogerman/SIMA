using SIMA.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SIMA.ExtensionsHelper;
using SIMA.Templates;
using SIMA.Infrastructure.Repositories.Interfaces;

namespace SIMA.Infrastructure.Repositories
{
    public class Paging : IPaging
    {
        private JsonFile<String> _jsonLimit;
        private const int _defaulLimit = 10;
        private const int _defaulOffset = 0;
        private int _offset = _defaulOffset;
        private int _limit = _defaulLimit;
        private int _pagenumber = 1;
        private int _totalPage = 1;

        public Paging()
        {
            _jsonLimit = new JsonFile<String>("PageLimit.json");
            _jsonLimit.loadData();
        }

        public int DefaultOffset { get => _defaulOffset; }
        public int DefaultLimit { get => _defaulLimit; }
        public int Pagenumber { get => _pagenumber; set => _pagenumber = value; }
        public int Offset { get => _offset; set => _offset = value; }
        public int Limit { get => _limit; set => _limit = value; }
        public int TotalPage { get => _totalPage; set => _totalPage = value; }

        /// <summary>
        /// Initialize the pagini values
        /// </summary>
        public void resetPage()
        {
            _offset = _defaulOffset;
            _limit = DefaultLimit;
            _pagenumber = 1;

        }
        /// <summary>
        /// Verified and correct the Paging values given the Total of data rendered
        /// </summary>
        /// <param name="total"></param>
        public void parsePageData(int total = -1)
        {
            int pagecount = int.Clamp(total.isNone(_totalPage) / (_limit > 0 ? _limit : 1),1,99999999);
            _totalPage = pagecount > 0 ? pagecount : 1;
            _pagenumber = _pagenumber <= pagecount ? _pagenumber : 1 ;


        }
        /// <summary>
        /// Control the Page number greater thatn 0 and lower thant Total data render
        /// </summary>
        /// <returns></returns>
        public bool isvalidPaging()
        {
            return (_pagenumber > 0) && (_pagenumber <= _totalPage);
        }
        /// <summary>
        /// Increase or Decrease the paging values given the DIRECTION
        /// </summary>
        /// <param name="direction"></param>
        public void movePage(DIRECTION direction) {
            switch (direction)
            {
                case DIRECTION.previous:
                    _pagenumber = _pagenumber - 1;
                    _offset     = Math.Abs(_offset - _defaulOffset);
                    break;
                case DIRECTION.next:
                    _pagenumber = _pagenumber + 1;
                    _offset     = Math.Abs(_offset  + _defaulOffset);
                    break;
                default:
                    _pagenumber = 1;
                    _offset     = _defaulOffset;
                    break;
            }
        }
        /// <summary>
        /// returns the dynamic configuration of the posibles offset Values 
        /// </summary>
        /// <returns></returns>
        public async Task<IEnumerable<String>> GetLimitPaging()
        {
            return await Task.Run(() => _jsonLimit.ServicesList.Select(p => p).ToList());

        }
        /// <summary>
        /// Fill the Page Limit Values
        /// </summary>
        public async Task FillLimitPageVal(PageNavigation pageControl)
        {
            IEnumerable<string> lim = await GetLimitPaging();
            pageControl.SetItemsPerPageSource(lim);
            var selected = pageControl.GetSelectedItemsPerPage();
            var _lim = selected != null ? selected.ToString() : _defaulLimit.ToString();
            _limit = int.Parse(_lim.ToString());
        }
    }
}