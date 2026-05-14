using AgroChem.TechnologistClient.Services;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Threading;

namespace AgroChem.TechnologistClient.Controls
{
    public partial class MonitoringControl : UserControl
    {
        private ApiService _api;
        private DispatcherTimer _timer;

        public MonitoringControl(ApiService api)
        {
            InitializeComponent();
            _api = api;
            _timer = new DispatcherTimer { Interval = System.TimeSpan.FromSeconds(10) };
            _timer.Tick += async (s, e) => await LoadActiveBatchesAsync();  // Здесь исправлено
            _timer.Start();
            _ = LoadActiveBatchesAsync(); // Запуск без ожидания (fire and forget)
        }

        // Метод возвращает Task, а не void
        private async Task LoadActiveBatchesAsync()
        {
            try
            {
                var batches = await _api.GetActiveBatchesAsync();
                dgActiveBatches.ItemsSource = batches;
            }
            catch (System.Exception ex)
            {
                System.Windows.MessageBox.Show($"Ошибка загрузки активных партий: {ex.Message}", "Ошибка",
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }
    }
}