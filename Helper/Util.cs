using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace SIMA.Helper
{
    public class Util
    {
        public void Loading_spimmer(System.Windows.Shapes.Ellipse spiner,bool show=false) {
            spiner.Visibility = show ? Visibility.Visible : Visibility.Hidden ;
        }

    }
}
