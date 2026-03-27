namespace SIMA.Domain.Models
{
    public class ProductView
    {
        public int? IdProduct { get; set; }
        public string? Name { get; set; }
        public int? IdCategory { get; set; }
        public string? Categorys { get; set; }
        public decimal? Price { get; set; }
    }
}