using SIMA.Domain.Models.Intefaces;

namespace SIMA.Domain.Models.Objects
{
    public class Users : IObjects
    {
        public int? IdUser { get; set;}
        public int? IdPerson { get; set; }
        public string? UserName { get; set;}
        public string? UserEmail { get; set;}
        public string UserPhone { get; set;}
        public string UserPassword { get; set;}
        public string CountryCode { get; set;}
        
    }
}