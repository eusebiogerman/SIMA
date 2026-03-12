using SIMA.Domain.Models;
using SIMA.Infrastructure.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SIMA.Presentation
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private StockProductServices _stockservices;

        public MainWindow()
        {
            _stockservices = new StockProductServices();
            InitializeComponent();
            cmbCategoria.SelectedIndex = 0;
            Fillcat();
            FillStock();

        }

        private async void Fillcat()
        {
            using (Task<IEnumerable<Category>> cat = new CategoryServices().GetAll())
            {
                foreach (var ct in await cat)
                {
                    cmbCategoria.Items.Add(ct.Name);
                }
            }
        }

        private async void FillStock() {
             
            gridProducts.ItemsSource = await _stockservices.GetAll(); 
        }

        private void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }
}
