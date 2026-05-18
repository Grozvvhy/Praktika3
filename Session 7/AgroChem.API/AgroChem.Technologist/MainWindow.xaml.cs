using System.Windows;

namespace AgroChem.Technologist
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            MainFrame.Navigate(new Pages.ProductsPage());
        }

        private void NavigateToProducts(object sender, RoutedEventArgs e) => MainFrame.Navigate(new Pages.ProductsPage());
        private void NavigateToRawMaterials(object sender, RoutedEventArgs e) => MainFrame.Navigate(new Pages.RawMaterialsPage());
        private void NavigateToEquipment(object sender, RoutedEventArgs e) => MainFrame.Navigate(new Pages.EquipmentPage());
        private void NavigateToUsers(object sender, RoutedEventArgs e) => MainFrame.Navigate(new Pages.UsersPage());
        private void NavigateToRoles(object sender, RoutedEventArgs e) => MainFrame.Navigate(new Pages.RolesPage());
        private void NavigateToRecipes(object sender, RoutedEventArgs e) => MainFrame.Navigate(new Pages.RecipesPage());
        private void NavigateToTechnologicalCards(object sender, RoutedEventArgs e) => MainFrame.Navigate(new Pages.TechnologicalCardsPage());
        private void NavigateToBatches(object sender, RoutedEventArgs e) => MainFrame.Navigate(new Pages.BatchesPage());
        private void NavigateToMonitoring(object sender, RoutedEventArgs e) => MainFrame.Navigate(new Pages.MonitoringPage());
        private void ExitApp(object sender, RoutedEventArgs e) => Application.Current.Shutdown();
    }
}