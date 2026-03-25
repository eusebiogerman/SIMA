using System.IO;
using System.Windows;
using Microsoft.Extensions.Configuration;

namespace SIMA.Helper
{
    public class Util
    {
        public void Loading_spimmer(System.Windows.Shapes.Ellipse spiner, bool show = false)
        {
            spiner.Visibility = show ? Visibility.Visible : Visibility.Hidden;
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
