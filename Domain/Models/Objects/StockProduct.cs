using SIMA.Domain.Models.Intefaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SIMA.Domain.Models.Objects
{
    public class StockProduct : IObjects
    {
        public int? IdStock { get; set; }
        public int? IdBrand { get; set; }
        public int Stock { get; set; }
    }
}
