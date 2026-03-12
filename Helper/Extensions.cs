using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SIMA.ExtensionsHelper
{
    public static class Extensions  
    {
        public static string defaultCategory(this string str)
        {
            return "All Category"; 
        }

        public static string getDefaultEmptyCat(this string str) {
            return str.Equals( str.defaultCategory()) ? string.Empty : str ;
        }

    }
}
