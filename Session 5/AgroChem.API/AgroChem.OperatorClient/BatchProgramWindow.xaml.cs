using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using AgroChem.OperatorClient.Models;
using AgroChem.OperatorClient.Services;

namespace AgroChem.OperatorClient
{
    public partial class BatchProgramWindow : Window
    {
        private ApiService _api;
        private string _operatorName;
        private int _batchId;
        private List<BatchProgramStep> _steps;
        private BatchProgramStep _currentStep;

        public BatchProgramWindow(ApiService api, string operatorName, int batchId)
        {
            InitializeComponent();
            _api = api;
            _operatorName = operatorName;
            _batchId = batchId;
            Title = $"Программа партии {batchId} – {operatorName}";
            LoadProgram();
        }

        private async void LoadProgram()
        {
            try
            {
                _steps = await _api.GetBatchProgramAsync(_batchId);
                listSteps.ItemsSource = _steps;
                if (_steps.Any())
                {
                    listSteps.SelectedItem = _steps.FirstOrDefault(s => s.Status == "in_progress") ?? _steps.FirstOrDefault(s => s.Status == "not_started");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки программы: {ex.Message}");
            }
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

                    // Проверка, все ли шаги завершены
                    if (_steps.All(s => s.Status == "completed"))
                    {
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

        private void Close_Click(object sender, RoutedEventArgs e) => Close();
    }
}