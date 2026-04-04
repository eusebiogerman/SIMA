using SIMA.Domain.Models.Intefaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SIMA.Domain.Models.Objects
{
    public class Category : IObjects
    {
        public int? IdCategory { get; set; } = null;
        public string? Name { get; set; } = null;
    }
}
