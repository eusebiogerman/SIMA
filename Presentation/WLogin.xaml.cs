using Microsoft.Extensions.Configuration;
using SIMA.Domain.Models.Objects;
using SIMA.Domain.Models.Params;
using SIMA.Domain.Models.Views;
using SIMA.Helper;
using SIMA.Infrastructure.Repositories;
using SIMA.Infrastructure.Repositories.Interfaces;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace SIMA.Presentation
{
    /// <summary>
    /// Interaction logic for WLogin.xaml
    /// </summary>
    public partial class WLogin : Window
    {
        private ICacheService _cache;
        private IContextservicesLogin<Users, UsertView, UserParam> _services;

        public WLogin()
        {
            InitializeComponent();

        }

        #region Util
        private async Task Login(bool loginazure=false)
        {
            bool ok = false;

            if (loginazure)
                ok = await App.AuthApp.LoginAsync();
            else
                ok = await _services.LoginAsync(new UserParam { UserName = txtUserName.Text, UserPassword = (string)txtPass.Tag }); 

            if (ok)
            {
                progress.StopProgress();
                // Usuario authenticated
                var main = new MainWindow("joel.eusebio@sima.com");
                main.Show();

                // close login
                Application.Current.Windows
                    .OfType<WLogin>()
                    .FirstOrDefault()?.Close();
            }
            else
            {
                MessageBox.Show("No se pudo iniciar sesión.");
            }
        }
        #endregion

        #region Events
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
           

           _cache = new MemoryCacheService();
            var _util = new Util();
            IConfiguration _config = _util.CustomConfiguration();
            _services = new UserServices(_config, _cache);

        }
        private void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            progress.SetLoadingState(async () => {
                await Login();
            }, true, "Athenticating...", 3000);
        }
        private void btnsClose_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Windows
            .OfType<WLogin>()
            .FirstOrDefault()?.Close();
        }
        private void LinkClick_azureLogin(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            progress.SetLoadingState(async () => {
                await Login(true);
            }, true, "Athenticating...", 3000);


        }
        #endregion
    }
}
