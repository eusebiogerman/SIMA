using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls.Primitives;
using SIMA.Domain.Models.Intefaces;

namespace SIMA.Domain.Models.Views
{
    public class StockProductView : IView
    {
        public int? IdStock { get; set; }
        public int? IdBrand { get; set; }
        public string Brands { get; set; }
        public int? IdProduct { get; set; }
        public string Products { get; set; }
        public int? IdCategory { get; set; }
        public string Categorys { get; set; }
        public decimal Price { get; set; }
        public int? Stock { get; set; }


    }
}
