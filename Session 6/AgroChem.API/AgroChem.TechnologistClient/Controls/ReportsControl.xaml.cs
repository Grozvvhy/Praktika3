using System.Windows;
using System.Windows.Controls;
using AgroChem.TechnologistClient.Services;
using Microsoft.Win32;

namespace AgroChem.TechnologistClient.Controls
{
    public partial class ReportsControl : UserControl
    {
        private ApiService _api;

        public ReportsControl(ApiService api)
        {
            InitializeComponent();
            _api = api;
        }

        private async void ExportBatches_Click(object sender, RoutedEventArgs e)
        {
            var batches = await _api.GetBatchesAsync();
            var saveDialog = new SaveFileDialog { Filter = "CSV файлы|*.csv", FileName = "batches_export.csv" };
            if (saveDialog.ShowDialog() == true)
            {
                using (var writer = new System.IO.StreamWriter(saveDialog.FileName, false, System.Text.Encoding.UTF8))
                {
                    writer.WriteLine("BatchNumber;OrderNumber;Status;ActualQuantityKg;StartTime");
                    foreach (var b in batches)
                    {
                        writer.WriteLine($"{b.BatchNumber};{b.OrderNumber};{b.Status};{b.ActualQuantityKg};{b.StartTime}");
                    }
                }
                MessageBox.Show("Экспорт завершён", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private async void ExportDeviations_Click(object sender, RoutedEventArgs e)
        {
            var deviations = await _api.GetDeviationsAsync();
            var saveDialog = new SaveFileDialog { Filter = "CSV файлы|*.csv", FileName = "deviations_export.csv" };
            if (saveDialog.ShowDialog() == true)
            {
                using (var writer = new System.IO.StreamWriter(saveDialog.FileName, false, System.Text.Encoding.UTF8))
                {
                    writer.WriteLine("BatchNumber;EventTime;EventType;Description;Severity");
                    foreach (var d in deviations)
                    {
                        writer.WriteLine($"{d.BatchNumber};{d.EventTime};{d.EventType};{d.Description};{d.Severity}");
                    }
                }
                MessageBox.Show("Экспорт завершён", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
    }
}