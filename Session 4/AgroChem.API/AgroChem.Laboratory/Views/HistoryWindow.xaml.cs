using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using AgroChem.Laboratory.Models;
using AgroChem.Laboratory.Services;
using Newtonsoft.Json;

namespace AgroChem.Laboratory.Views
{
    public partial class HistoryWindow : Window, INotifyPropertyChanged
    {
        private readonly ApiService _api;
        private readonly object _batch;
        private readonly string _sampleType;
        private ObservableCollection<HistoryRecord> _records;
        private HistoryRecord _selectedRecord;
        private bool _canEdit;
        private string _title;

        public new string Title { get => _title; set { _title = value; OnPropertyChanged(); } }
        public bool CanEdit { get => _canEdit; set { _canEdit = value; OnPropertyChanged(); } }

        public HistoryWindow(ApiService api, object batch, string sampleType, string title)
        {
            InitializeComponent();
            _api = api;
            _batch = batch;
            _sampleType = sampleType;
            Title = title;
            DataContext = this;
            LoadHistory();
        }

        private async void LoadHistory()
        {
            try
            {
                int? batchId = null, rawId = null;
                if (_sampleType == "final_product")
                    batchId = (int)((dynamic)_batch).Id;
                else
                    rawId = (int)((dynamic)_batch).Id;

                var list = await _api.GetTestHistoryAsync(batchId, rawId);
                foreach (var rec in list)
                {
                    if (!string.IsNullOrEmpty(rec.TestResultsJson))
                    {
                        var pars = JsonConvert.DeserializeObject<System.Collections.Generic.List<TestParameterDto>>(rec.TestResultsJson);
                        rec.ParameterNames = string.Join(", ", pars.Select(p => $"{p.ParameterName}:{p.MeasuredValue}"));
                        rec.Parameters = pars;
                    }
                    else
                    {
                        rec.ParameterNames = $"{rec.ParameterName}:{rec.MeasuredValue}";
                        rec.Parameters = new System.Collections.Generic.List<TestParameterDto>
                        {
                            new TestParameterDto
                            {
                                ParameterName = rec.ParameterName,
                                MeasuredValue = rec.MeasuredValue,
                                StandardValue = rec.StandardValue,
                                Unit = rec.Unit,
                                Result = rec.Result
                            }
                        };
                    }
                }
                _records = new ObservableCollection<HistoryRecord>(list);
                dgHistory.ItemsSource = _records;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки истории: {ex.Message}");
            }
        }

        private void DgHistory_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            _selectedRecord = dgHistory.SelectedItem as HistoryRecord;
            CanEdit = _selectedRecord != null;
        }

        private void Edit_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedRecord == null) return;
            var editWin = new EditTestWindow(_api, _selectedRecord);
            editWin.Owner = this;
            if (editWin.ShowDialog() == true)
                LoadHistory();
        }

        private void Close_Click(object sender, RoutedEventArgs e) => Close();

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}