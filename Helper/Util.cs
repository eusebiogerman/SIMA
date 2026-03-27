using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Extensions.Configuration;

namespace SIMA.Helper
{
    public class Util
    {
        public  void Loading_spimmer(System.Windows.Shapes.Ellipse spiner, bool show = false,int lapse = 3000)
        {
            if (show && spiner.Visibility == Visibility.Hidden)
            {
                spiner.Visibility = Visibility.Visible ;
                Thread.Sleep(lapse);
            }
            else
            {
                spiner.Visibility = show ? Visibility.Visible : Visibility.Hidden;
            }
        }

        public IConfiguration CustomConfiguration()
        {
        JsonFile<object> _configfile = new JsonFile<object>("appsettings.json", "/Infrastructure/Config");
        var builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile(_configfile.FilePath, optional: false, reloadOnChange: true)
            .AddEnvironmentVariables();

            return builder.Build();
        }

    }

}
