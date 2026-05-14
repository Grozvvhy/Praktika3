using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using AgroChem.Laboratory.Models;
using AgroChem.Laboratory.Services;

namespace AgroChem.Laboratory.Views
{
    public partial class EditTestWindow : Window, INotifyPropertyChanged
    {
        private readonly ApiService _api;
        private readonly HistoryRecord _record;
        private ObservableCollection<TestParameter> _parameters;
        private string _title;

        public ObservableCollection<TestParameter> Parameters { get => _parameters; set { _parameters = value; OnPropertyChanged(); } }
        public new string Title { get => _title; set { _title = value; OnPropertyChanged(); } }

        public EditTestWindow(ApiService api, HistoryRecord record)
        {
            InitializeComponent();
            _api = api;
            _record = record;
            Title = $"Редактирование от {record.AnalysisDate:dd.MM.yyyy HH:mm}";
            LoadParameters();
            txtComment.Text = record.AnalystComment;
            DataContext = this;
        }

        private void LoadParameters()
        {
            Parameters = new ObservableCollection<TestParameter>();
            foreach (var p in _record.Parameters)
            {
                var param = new TestParameter
                {
                    ParameterName = p.ParameterName,
                    StandardValue = p.StandardValue,
                    Unit = p.Unit,
                    MeasuredValue = p.MeasuredValue,
                    Result = p.Result == "pass" ? "✅ pass" : "❌ fail"
                };
                param.PropertyChanged += Parameter_PropertyChanged;
                Parameters.Add(param);
            }
        }

        private void Parameter_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "MeasuredValue")
                EvaluateParameter((TestParameter)sender);
        }

        private void EvaluateParameter(TestParameter param)
        {
            if (string.IsNullOrWhiteSpace(param.MeasuredValue))
            {
                param.Result = "⏳";
                return;
            }

            double val;
            if (!double.TryParse(param.MeasuredValue, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out val))
            {
                param.Result = "❌ не число";
                return;
            }

            var std = param.StandardValue;
            bool pass = false;
            try
            {
                if (std.Contains("-"))
                {
                    var parts = std.Split('-');
                    if (parts.Length == 2 && double.TryParse(parts[0], out double min) && double.TryParse(parts[1], out double max))
                        pass = val >= min && val <= max;
                }
                else if (std.StartsWith(">"))
                {
                    if (double.TryParse(std.Substring(1), out double min))
                        pass = val > min;
                }
                else if (std.StartsWith("<"))
                {
                    if (double.TryParse(std.Substring(1), out double max))
                        pass = val < max;
                }
                else if (std.StartsWith("≤") || std.StartsWith("<="))
                {
                    var numStr = std.TrimStart('≤', '<', '=');
                    if (double.TryParse(numStr, out double max))
                        pass = val <= max;
                }
                else if (std.StartsWith("≥") || std.StartsWith(">="))
                {
                    var numStr = std.TrimStart('≥', '>', '=');
                    if (double.TryParse(numStr, out double min))
                        pass = val >= min;
                }
                else
                {
                    pass = param.MeasuredValue == std;
                }
            }
            catch { pass = false; }

            param.Result = pass ? "✅ pass" : "❌ fail";
        }

        private bool AllParametersValid()
        {
            return Parameters.All(p => !string.IsNullOrEmpty(p.MeasuredValue) && p.Result.Contains("pass"));
        }

        private async void Save_Click(object sender, RoutedEventArgs e)
        {
            if (!AllParametersValid())
            {
                MessageBox.Show("Есть отклонения от нормы или не все значения введены.");
                return;
            }

            var request = new SaveTestResultRequest
            {
                SampleType = _record.SampleType,
                Parameters = Parameters.Select(p => new TestParameterDto
                {
                    ParameterName = p.ParameterName,
                    MeasuredValue = p.MeasuredValue,
                    StandardValue = p.StandardValue,
                    Unit = p.Unit,
                    Result = p.Result.Contains("pass") ? "pass" : "fail"
                }).ToList(),
                Decision = _record.Decision,
                AnalystComment = txtComment.Text,
                AnalystName = _record.AnalystName
            };

            // Используем BatchId / RawMaterialBatchId из записи
            if (_record.SampleType == "final_product")
                request.BatchId = _record.BatchId;
            else
                request.RawMaterialBatchId = _record.RawMaterialBatchId;

            if (!request.BatchId.HasValue && !request.RawMaterialBatchId.HasValue)
            {
                MessageBox.Show("Не удалось определить идентификатор партии. Редактирование невозможно.");
                return;
            }

            try
            {
                var success = await _api.UpdateTestResultAsync(_record.Id, request);
                if (success)
                {
                    MessageBox.Show("Изменения сохранены.");
                    DialogResult = true;
                    Close();
                }
                else
                    MessageBox.Show("Ошибка при сохранении.");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}");
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e) => Close();

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}