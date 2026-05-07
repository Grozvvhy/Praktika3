using System.Windows;

namespace AgroChem.Technologist
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void OnRecipesClick(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new Pages.RecipesPage());
        }

        private void OnCardsClick(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new Pages.TechnologicalCardsPage());
        }

        private void OnBatchesClick(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new Pages.ProductionBatchesPage());
        }

        private void OnLabClick(object sender, RoutedEventArgs e)
        {
            MainFrame.Navigate(new Pages.LabTestsPage());
        }
    }
}