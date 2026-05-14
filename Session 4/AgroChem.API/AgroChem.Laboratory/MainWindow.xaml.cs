using System;
using System.Windows;
using System.Windows.Controls;
using AgroChem.Laboratory.Models;
using AgroChem.Laboratory.Services;
using AgroChem.Laboratory.Views;

namespace AgroChem.Laboratory
{
    public partial class MainWindow : Window
    {
        private ApiService _api;
        private object _selectedBatch;
        private string _analystName;

        public MainWindow(ApiService api, string analystName)
        {
            InitializeComponent();
            _api = api;
            _analystName = analystName;
            lblAnalyst.Text = $"Лаборант: {analystName}";
            LoadData();
        }

        private async void LoadData()
        {
            try
            {
                if (rbRaw.IsChecked == true)
                {
                    var data = await _api.GetRawMaterialBatchesAsync();
                    listRaw.ItemsSource = data;
                }
                else
                {
                    var data = await _api.GetFinalProductBatchesAsync();
                    listFinal.ItemsSource = data;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки: {ex.Message}");
            }
        }

        private void rbRaw_Checked(object sender, RoutedEventArgs e)
        {
            listRaw.Visibility = Visibility.Visible;
            listFinal.Visibility = Visibility.Collapsed;
            btnQuality.IsEnabled = false;
            btnHistory.IsEnabled = false;
            _selectedBatch = null;
            LoadData();
        }

        private void rbFinal_Checked(object sender, RoutedEventArgs e)
        {
            listRaw.Visibility = Visibility.Collapsed;
            listFinal.Visibility = Visibility.Visible;
            btnQuality.IsEnabled = false;
            btnHistory.IsEnabled = false;
            _selectedBatch = null;
            LoadData();
        }

        private void list_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender == listRaw && listRaw.SelectedItem != null)
            {
                _selectedBatch = listRaw.SelectedItem;
                btnQuality.IsEnabled = true;
                btnHistory.IsEnabled = true;
            }
            else if (sender == listFinal && listFinal.SelectedItem != null)
            {
                _selectedBatch = listFinal.SelectedItem;
                btnQuality.IsEnabled = true;
                btnHistory.IsEnabled = true;
            }
            else
            {
                btnQuality.IsEnabled = false;
                btnHistory.IsEnabled = false;
                _selectedBatch = null;
            }
        }

        private void btnQuality_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedBatch == null) return;
            QualityControlWindow win;
            if (rbRaw.IsChecked == true)
                win = new QualityControlWindow(_api, _analystName, (RawMaterialBatchForQC)_selectedBatch);
            else
                win = new QualityControlWindow(_api, _analystName, (FinalProductBatchForQC)_selectedBatch);
            win.Owner = this;
            win.ShowDialog();
            LoadData();
            btnQuality.IsEnabled = false;
            btnHistory.IsEnabled = false;
            _selectedBatch = null;
        }

        private void btnHistory_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedBatch == null) return;
            string title = rbRaw.IsChecked == true
                ? $"История сырья {((RawMaterialBatchForQC)_selectedBatch).BatchNumber}"
                : $"История партии {((FinalProductBatchForQC)_selectedBatch).BatchNumber}";
            var win = new HistoryWindow(_api, _selectedBatch, rbRaw.IsChecked == true ? "raw_material" : "final_product", title);
            win.Owner = this;
            win.ShowDialog();
        }
    }
}