using SIMA.Domain.Models.Intefaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SIMA.Domain.Models.Objects
{

    public class Product : IObjects
    {
        public int? IdProduct { get; set; }
        public string Name { get; set; }
        public int? IdCategory { get; set; }
        public decimal Price { get; set; }
    }
}
