using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using SIMA.Helper;
using SIMA.Infrastructure.Repositories.Interfaces;
using SIMA.Presentation.ViewModel;
using SIMA.Templates;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace SIMA.Presentation.Repository
{
    public class WindowServicesBase<T, V, P> 
    {
        private IPaging _page;
        private readonly IConfiguration _config;
        private IContextservices<T, V, P> _service;

        private Util? _util;
        private bool _formState = false;


        public IConfiguration Config => _config;
        public IPaging Page { get => _page; set => _page = value; }
        public IContextservices<T, V, P> Service { get => _service; set => _service = value; }

        public ViewModelBase? DataContext { get; set; }
        public LovTextBox? ChildLovtextbox { get; set; }
        public Dictionary<int, string> ColumnsWidth { get; set; }
        public Window CurrentWindow { get; set; }
        public Border Form { get; set; }
        public ListView? GridListView { get; set; }
        public GridView? GridView { get; set; }
        public Func<V, bool>? IngorePredicate { get; set; }
        public PageNavigation? PageControl { get; set; }
        public LovTextBox? ParentLovtextbox { get; set; }
        public StatusbarLoading? Status { get; set; }
        public TextBlock? TxtResults { get; set; }
        public TextBox? TxtSearch { get; set; }
        public Type ModelType { private get; set; }
        public ObservableCollection<StatusItem> ObsrverStatus { get; set; }
        public Util? Util { get => _util; private set =>  _util = value; }

        public bool EditMode { get; set; }
        public bool FormState { get => _formState;  set => _formState = value; }
        public bool FromMain { get; set; }
        public bool Isloaded { get; set; }
        public string Domain { get; set; }
        public string StandarHeight { get; set; }

        public WindowServicesBase(IConfiguration config, IPaging page, IContextservices<T, V, P> services)
        {

            _config = config;
            _page = page;
            _util = new Util();
            _service = services;
            _formState = true;
        }


    }
}