using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SIMA.Domain.Models
{
    public class Brand
    {
        public int? IdBrand { get; set; } = null;
        public int? IdProduct { get; set; } = null;
        public string? Name { get; set; } = null;
        public decimal? Price { get; set; } = null;
    }
}

