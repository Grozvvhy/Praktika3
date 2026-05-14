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
    public partial class QualityControlWindow : Window, INotifyPropertyChanged
    {
        private readonly ApiService _api;
        private readonly string _analystName;
        private readonly int _entityId;
        private readonly bool _isRaw;
        private string _title;

        public ObservableCollection<TestParameter> Parameters { get; set; } = new ObservableCollection<TestParameter>();
        public new string Title { get => _title; set { _title = value; OnPropertyChanged(); } }

        public QualityControlWindow(ApiService api, string analystName, int entityId, bool isRaw)
        {
            InitializeComponent();
            _api = api;
            _analystName = analystName;
            _entityId = entityId;
            _isRaw = isRaw;
            Title = isRaw ? $"Контроль сырья (ID: {entityId})" : $"Контроль готовой продукции (ID: {entityId})";
            LoadStandards();
            DataContext = this;
        }

        private async void LoadStandards()
        {
            try
            {
                var standards = _isRaw ? await _api.GetRawMaterialStandardsAsync(_entityId) : await _api.GetProductStandardsAsync(_entityId);
                if (standards == null || standards.Count == 0)
                {
                    MessageBox.Show("Для данной партии не заданы нормативы. Контроль невозможен.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    Close();
                    return;
                }
                Parameters.Clear();
                foreach (var std in standards)
                {
                    Parameters.Add(new TestParameter
                    {
                        ParameterName = std.ParameterName,
                        StandardValue = std.StandardValue,
                        Unit = std.Unit,
                        MeasuredValue = "",
                        Result = "⏳"
                    });
                }
                foreach (var param in Parameters)
                    param.PropertyChanged += Parameter_PropertyChanged;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки нормативов: {ex.Message}");
                Close();
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
            bool pass = false;
            try
            {
                if (param.StandardValue.Contains("-"))
                {
                    var parts = param.StandardValue.Split('-');
                    if (parts.Length == 2 && double.TryParse(parts[0], out double min) && double.TryParse(parts[1], out double max))
                        pass = val >= min && val <= max;
                }
                else if (param.StandardValue.StartsWith(">"))
                {
                    if (double.TryParse(param.StandardValue.Substring(1), out double min))
                        pass = val > min;
                }
                else if (param.StandardValue.StartsWith("<"))
                {
                    if (double.TryParse(param.StandardValue.Substring(1), out double max))
                        pass = val < max;
                }
                else if (param.StandardValue.StartsWith("≤") || param.StandardValue.StartsWith("<="))
                {
                    var numStr = param.StandardValue.TrimStart('≤', '<', '=');
                    if (double.TryParse(numStr, out double max))
                        pass = val <= max;
                }
                else if (param.StandardValue.StartsWith("≥") || param.StandardValue.StartsWith(">="))
                {
                    var numStr = param.StandardValue.TrimStart('≥', '>', '=');
                    if (double.TryParse(numStr, out double min))
                        pass = val >= min;
                }
                else
                {
                    pass = param.MeasuredValue == param.StandardValue;
                }
            }
            catch { pass = false; }
            param.Result = pass ? "✅ pass" : "❌ fail";
        }

        private bool AllParametersValid()
        {
            if (Parameters.Count == 0) return false;
            return Parameters.All(p => !string.IsNullOrEmpty(p.MeasuredValue) && p.Result.Contains("pass"));
        }

        private async void Approve_Click(object sender, RoutedEventArgs e)
        {
            if (!AllParametersValid())
            {
                MessageBox.Show("Не все параметры введены или имеются отклонения.", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            await SaveResult("approved", "Допущено лаборантом");
        }

        private async void Block_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtBlockReason.Text))
            {
                MessageBox.Show("Укажите причину блокировки.", "Обязательное поле", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (!AllParametersValid())
            {
                MessageBox.Show("Заполните все показатели.", "Неполные данные", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            await SaveResult("blocked", txtBlockReason.Text);
        }

        private async Task SaveResult(string decision, string comment)
        {
            var request = new SaveTestResultRequest
            {
                SampleType = _isRaw ? "raw_material" : "final_product",
                Parameters = Parameters.Select(p => new TestParameterDto
                {
                    ParameterName = p.ParameterName,
                    MeasuredValue = p.MeasuredValue,
                    StandardValue = p.StandardValue,
                    Unit = p.Unit,
                    Result = p.Result.Contains("pass") ? "pass" : "fail"
                }).ToList(),
                Decision = decision,
                AnalystComment = comment,
                AnalystName = _analystName
            };
            if (_isRaw)
                request.RawMaterialBatchId = _entityId;
            else
                request.BatchId = _entityId;

            try
            {
                if (await _api.SaveTestResultsAsync(request))
                {
                    MessageBox.Show($"Результаты сохранены. Партия {(decision == "approved" ? "допущена" : "заблокирована")}.");
                    DialogResult = true;
                    Close();
                }
                else MessageBox.Show("Ошибка при сохранении.");
            }
            catch (Exception ex) { MessageBox.Show($"Ошибка: {ex.Message}"); }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e) => Close();

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}