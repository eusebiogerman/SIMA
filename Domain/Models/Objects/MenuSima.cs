using SIMA.Domain.Models.Intefaces;

namespace SIMA.Domain.Models.Objects
{
    public class MenuSima : IObjects
    {
        public int?  IdParent { get; set; } = null;
        public string? ParentName { get; set; } = null;
        public int? IdChild { get; set; } = null;
        public int? IdApp { get; set; } = null;
        public string? AppName { get; set; } = null;
        public string? ObjectName { get; set; } = null;
        public string? Type { get; set; } = null;
        public string? IconClass { get; set; } = null;
        
    }
}