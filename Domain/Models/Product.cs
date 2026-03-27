using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SIMA.Domain.Models
{

    public class Product
    {
        public int? IdProduct { get; set; }
        public string Name { get; set; }
        public int? IdCategory { get; set; }
        public decimal Price { get; set; }
    }
}
