using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Identity.Client;
using SIMA.Helper;
using SIMA.Presentation;
using SIMA.Presentation.Repository;
using SIMA.Presentation.ViewModel;
using SIMA.Presentation.Views;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace SIMA
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {

        private static AuthService _authapp = new();
        public static AuthService AuthApp { get => _authapp; private set => _authapp = value; }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            var login = new WLogin();
            login.Show();
        }
        private void Application_Startup(object sender, StartupEventArgs e)
        {


        }
    }
}
