using System.Windows.Controls;
using AgroChem.TechnologistClient.Services;

namespace AgroChem.TechnologistClient.Controls
{
    public partial class OrdersControl : UserControl
    {
        private ApiService _api;

        public OrdersControl(ApiService api)
        {
            InitializeComponent();
            _api = api;
            LoadOrders();
        }

        private async void LoadOrders()
        {
            var orders = await _api.GetOrdersAsync();
            dgOrders.ItemsSource = orders;
        }
    }
}