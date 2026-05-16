using System.Windows.Controls;
using AgroChem.TechnologistClient.Services;

namespace AgroChem.TechnologistClient.Controls
{
    public partial class BatchesControl : UserControl
    {
        private ApiService _api;

        public BatchesControl(ApiService api)
        {
            InitializeComponent();
            _api = api;
            LoadBatches();
        }

        private async void LoadBatches()
        {
            var batches = await _api.GetBatchesAsync();
            dgBatches.ItemsSource = batches;
        }
    }
}