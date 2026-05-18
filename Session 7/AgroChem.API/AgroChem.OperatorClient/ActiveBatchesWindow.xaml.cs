using System;
using System.Collections.Generic;
using System.Windows;
using AgroChem.OperatorClient.Models;
using AgroChem.OperatorClient.Services;

namespace AgroChem.OperatorClient
{
    public partial class ActiveBatchesWindow : Window
    {
        private ApiService _api;
        private string _operatorName;
        private List<ActiveBatch> _batches;

        public ActiveBatchesWindow(ApiService api, string operatorName)
        {
            InitializeComponent();
            _api = api;
            _operatorName = operatorName;
            this.Title = $"Активные партии – {operatorName}";
            LoadBatches();
        }

        private async void LoadBatches()
        {
            try
            {
                _batches = await _api.GetActiveBatchesAsync();
                dgBatches.ItemsSource = _batches;
                dgBatches.SelectionChanged += (s, e) => btnSelect.IsEnabled = dgBatches.SelectedItem != null;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки: {ex.Message}");
            }
        }

        private void BtnSelect_Click(object sender, RoutedEventArgs e)
        {
            if (dgBatches.SelectedItem is ActiveBatch selected)
            {
                var programWindow = new BatchProgramWindow(_api, _operatorName, selected.BatchId);
                programWindow.Show();
                this.Close();
            }
        }
    }
}