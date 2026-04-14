using SIMA.Domain.Models.Intefaces;

namespace SIMA.Domain.Models.Views
{
    public class UsertView : IView
    {
        public int? UserId { get; set; }
        public string? UserName { get; set; }
        public string? UserEmail { get; set; }
        public string? UserPhone { get; set; }
        public string? UserPassword { get; set; }
        public string? CountryCode { get; set; }
    }
}