using System.Windows;
using System.Windows.Controls;
using AgroChem.TechnologistClient.Controls;
using AgroChem.TechnologistClient.Services;

namespace AgroChem.TechnologistClient
{
    public partial class MainWindow : Window
    {
        private ApiService _api;
        private string _currentUser;

        public MainWindow(ApiService api, string username)
        {
            InitializeComponent();
            _api = api;
            _currentUser = username;
            LoadUserControl(new DashboardControl(_api));
        }

        private void LoadUserControl(UserControl uc)
        {
            MainContent.Content = uc;
        }

        private void Products_Click(object sender, RoutedEventArgs e) => LoadUserControl(new ProductsControl(_api));
        private void Recipes_Click(object sender, RoutedEventArgs e) => LoadUserControl(new RecipesControl(_api));
        private void TechMaps_Click(object sender, RoutedEventArgs e) => LoadUserControl(new TechMapControl(_api));
        private void Orders_Click(object sender, RoutedEventArgs e) => LoadUserControl(new OrdersControl(_api));
        private void Batches_Click(object sender, RoutedEventArgs e) => LoadUserControl(new BatchesControl(_api));
        private void Extruder_Click(object sender, RoutedEventArgs e) => LoadUserControl(new ExtruderControl(_api));
        private void Monitoring_Click(object sender, RoutedEventArgs e) => LoadUserControl(new MonitoringControl(_api));
        private void Deviations_Click(object sender, RoutedEventArgs e) => LoadUserControl(new DeviationsControl(_api));
        private void Reports_Click(object sender, RoutedEventArgs e) => LoadUserControl(new ReportsControl(_api));
    }
}