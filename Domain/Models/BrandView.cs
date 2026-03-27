namespace SIMA.Domain.Models
{
    public class BrandView
    {
        public int? IdBrand { get; set; }
        public string? Name { get; set; }
        public int? IdProduct { get; set; }
        public string? Products { get; set; }
        public int? IdCategory { get; set; }
        public string? Categorys { get; set; }
        public decimal? Price { get; set; }
    }
}