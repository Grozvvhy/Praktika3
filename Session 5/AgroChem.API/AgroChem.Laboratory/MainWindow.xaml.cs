using System;
using System.Collections.Generic;
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
        private string _analystName;
        private object _selectedBatch;

        // Единственный конструктор – принимает ApiService и имя лаборанта
        public MainWindow(ApiService api, string analystName)
        {
            InitializeComponent();
            _api = api ?? throw new ArgumentNullException(nameof(api));
            _analystName = analystName;
            txtAnalyst.Text = $"Лаборант: {analystName}";
            LoadData();
        }

        private async void LoadData()
        {
            try
            {
                if (rbRaw.IsChecked == true)
                {
                    var data = await _api.GetRawMaterialBatchesAsync();
                    var items = new List<object>();
                    foreach (var item in data)
                    {
                        items.Add(new { DisplayName = item.BatchNumber, Info = $"{item.MaterialName} | {item.QuantityKg} кг", Id = item.Id, Type = "raw" });
                    }
                    listBatches.ItemsSource = items;
                }
                else
                {
                    var data = await _api.GetFinalProductBatchesAsync();
                    var items = new List<object>();
                    foreach (var item in data)
                    {
                        items.Add(new { DisplayName = item.BatchNumber, Info = $"{item.ProductName} | {item.ActualQuantityKg} кг", Id = item.Id, Type = "final" });
                    }
                    listBatches.ItemsSource = items;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки: {ex.Message}");
            }
        }

        private void Tab_Checked(object sender, RoutedEventArgs e)
        {
            LoadData();
            btnQuality.IsEnabled = false;
            _selectedBatch = null;
        }

        private void ListBatches_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (listBatches.SelectedItem != null)
            {
                _selectedBatch = listBatches.SelectedItem;
                btnQuality.IsEnabled = true;
            }
            else
            {
                btnQuality.IsEnabled = false;
                _selectedBatch = null;
            }
        }

        private void BtnQuality_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedBatch == null) return;

            dynamic selected = _selectedBatch;
            int id = selected.Id;
            string type = selected.Type;

            QualityControlWindow win;
            if (type == "raw")
                win = new QualityControlWindow(_api, _analystName, id, true);
            else
                win = new QualityControlWindow(_api, _analystName, id, false);

            win.Owner = this;
            win.ShowDialog();
            LoadData();          // обновить список после контроля
            btnQuality.IsEnabled = false;
            _selectedBatch = null;
        }
    }
}