using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WpfJEG.net6;

namespace SIMA.Infrastructure.Repositories.Interfaces
{
    public enum DIRECTION
    {
        previous = 1,
        next = 2
    }
    public interface IPaging
    {

        int DefaultOffset { get;}
        int DefaultLimit { get;}
        int Pagenumber { get; set; }
        int Offset { get; set; }
        int Limit { get; set; }
        int TotalPage { get; set; }


        void resetPage();
        void parsePageData(int total = -1);
        bool isvalidPaging();
        void movePage(DIRECTION direction);
        Task<IEnumerable<String>> GetLimitPaging();
        Task FillLimitPageVal(PageNavigation pageControl);
    }
}