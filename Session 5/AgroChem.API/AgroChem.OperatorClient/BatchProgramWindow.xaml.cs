using AgroChem.OperatorClient.Models;
using AgroChem.OperatorClient.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace AgroChem.OperatorClient
{
    public partial class BatchProgramWindow : Window
    {
        private ApiService _api;
        private string _operatorName;
        private int _batchId;
        private List<BatchProgramStep> _steps;
        private BatchProgramStep _currentStep;
        private DispatcherTimer _telemetryTimer;
        private string _equipmentLine = "Экструдер Линия 1";

        public BatchProgramWindow(ApiService api, string operatorName, int batchId)
        {
            InitializeComponent();
            _api = api;
            _operatorName = operatorName;
            _batchId = batchId;
            Title = $"Программа партии {batchId} – {operatorName}";
            LoadProgram();

            _telemetryTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(3) };
            _telemetryTimer.Tick += async (s, e) => await LoadTelemetry();
            _telemetryTimer.Start();
        }

        private async void LoadProgram()
        {
            try
            {
                _steps = await _api.GetBatchProgramAsync(_batchId);
                listSteps.ItemsSource = _steps;
                if (_steps.Any())
                {
                    var active = _steps.FirstOrDefault(s => s.Status == "in_progress");
                    if (active != null)
                        listSteps.SelectedItem = active;
                    else
                        listSteps.SelectedItem = _steps.FirstOrDefault(s => s.Status == "not_started");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки программы: {ex.Message}");
            }
        }

        private async Task LoadTelemetry()
        {
            try
            {
                var tele = await _api.GetTelemetryAsync(_equipmentLine);
                txtTeleTemp.Text = tele.Temperature.HasValue ? $"{tele.Temperature} °C" : "—";
                txtTelePressure.Text = tele.Pressure.HasValue ? $"{tele.Pressure} бар" : "—";
                txtTeleSpeed.Text = tele.ScrewSpeed.HasValue ? $"{tele.ScrewSpeed} об/мин" : "—";
                txtTeleLastUpdate.Text = tele.LastUpdate.ToString("HH:mm:ss");
            }
            catch { }
        }

        private void ListSteps_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (listSteps.SelectedItem is BatchProgramStep step)
            {
                _currentStep = step;
                txtInstruction.Text = step.Instruction;
                txtPlanTemp.Text = step.PlannedTempC?.ToString() ?? "—";
                txtPlanPressure.Text = step.PlannedPressureBar?.ToString() ?? "—";
                txtPlanDuration.Text = step.PlannedDurationMin?.ToString() ?? "—";
                txtActualTemp.Text = step.ActualTempC?.ToString() ?? "";
                txtActualPressure.Text = step.ActualPressureBar?.ToString() ?? "";
                txtActualDuration.Text = step.ActualDurationMin?.ToString() ?? "";
                txtComment.Text = step.OperatorComment ?? "";

                btnStartStep.IsEnabled = step.Status == "not_started";
                btnCompleteStep.IsEnabled = step.Status == "in_progress";
            }
        }

        private async void BtnStartStep_Click(object sender, RoutedEventArgs e)
        {
            if (_currentStep == null) return;
            try
            {
                if (await _api.StartStepAsync(_batchId, _currentStep.StepOrder))
                {
                    _currentStep.Status = "in_progress";
                    RefreshStepList();
                    btnStartStep.IsEnabled = false;
                    btnCompleteStep.IsEnabled = true;
                }
                else
                    MessageBox.Show("Не удалось начать шаг.");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}");
            }
        }

        private async void BtnCompleteStep_Click(object sender, RoutedEventArgs e)
        {
            if (_currentStep == null) return;
            try
            {
                decimal? temp = null, pressure = null;
                int? duration = null;
                if (!string.IsNullOrWhiteSpace(txtActualTemp.Text))
                    temp = decimal.Parse(txtActualTemp.Text);
                if (!string.IsNullOrWhiteSpace(txtActualPressure.Text))
                    pressure = decimal.Parse(txtActualPressure.Text);
                if (!string.IsNullOrWhiteSpace(txtActualDuration.Text))
                    duration = int.Parse(txtActualDuration.Text);

                if (await _api.CompleteStepAsync(_batchId, _currentStep.StepOrder, temp, pressure, duration, txtComment.Text))
                {
                    _currentStep.Status = "completed";
                    RefreshStepList();
                    btnStartStep.IsEnabled = false;
                    btnCompleteStep.IsEnabled = false;

                    if (_steps.All(s => s.Status == "completed"))
                    {
                        _telemetryTimer.Stop();
                        MessageBox.Show("Все шаги выполнены! Партия завершена.");
                        Close();
                    }
                }
                else
                    MessageBox.Show("Не удалось завершить шаг.");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}");
            }
        }

        private void RefreshStepList()
        {
            listSteps.ItemsSource = null;
            listSteps.ItemsSource = _steps;
            if (_currentStep != null)
                listSteps.SelectedItem = _currentStep;
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            _telemetryTimer.Stop();
            Close();
        }
    }
}