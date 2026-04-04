using SIMA.Domain.Models.Intefaces;

namespace SIMA.Domain.Models.Views
{
    public class ProductView : IView
    {
        public int? IdProduct { get; set; }
        public string? Name { get; set; }
        public int? IdCategory { get; set; }
        public string? Categorys { get; set; }
        public decimal? Price { get; set; }
    }
}