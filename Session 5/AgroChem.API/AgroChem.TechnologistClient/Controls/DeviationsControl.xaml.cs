using System.Windows.Controls;
using AgroChem.TechnologistClient.Services;

namespace AgroChem.TechnologistClient.Controls
{
    public partial class DeviationsControl : UserControl
    {
        private ApiService _api;

        public DeviationsControl(ApiService api)
        {
            InitializeComponent();
            _api = api;
            LoadDeviations();
        }

        private async void LoadDeviations()
        {
            var deviations = await _api.GetDeviationsAsync();
            dgDeviations.ItemsSource = deviations;
        }
    }
}